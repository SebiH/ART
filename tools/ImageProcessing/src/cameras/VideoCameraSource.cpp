#include "cameras/VideoCameraSource.h"

#include <chrono>
#include <thread>
#include <opencv2/imgproc.hpp>
#include "utils/Logger.h"

#pragma warning(push)
#pragma warning(disable: 4996)

extern "C"
{
    #include <libavformat/avformat.h>
    #include <libavcodec/avcodec.h>
    #include <libavutil/avutil.h>
    #include <libavutil/pixdesc.h>
    #include <libswscale/swscale.h>
    #include <libavutil/frame.h>
    #include <libavutil/imgutils.h>
    #include <libavutil/mem.h>
}

#include <opencv2/highgui.hpp>

using namespace ImageProcessing;

VideoCameraSource::VideoCameraSource(const std::string &src)
    : src_(src), frame_counter_(0), frame_()
{
    // http://dranger.com/ffmpeg

    // Initalizing these to NULL prevents segfaults!
    AVFormatContext   *pFormatCtx = NULL;
    int               i, videoStream;
    AVCodecContext    *pCodecCtxOrig = NULL;
    AVCodecContext    *pCodecCtx = NULL;
    AVCodec           *pCodec = NULL;
    AVFrame           *pFrame = NULL;
    AVFrame           *pFrameRGB = NULL;
    AVPacket          packet;
    int               frameFinished;
    int               numBytes;
    uint8_t           *buffer = NULL;
    struct SwsContext *sws_ctx = NULL;

    // Register all formats and codecs
    av_register_all();

    // Open video file
    if (avformat_open_input(&pFormatCtx, src.c_str(), NULL, NULL) != 0)
    {
        throw std::exception("Could not open file");
    }

                   // Retrieve stream information
    if (avformat_find_stream_info(pFormatCtx, NULL) < 0)
    {
        throw std::exception("Could not find stream info");
    }

   // Dump information about file onto standard error
    //av_dump_format(pFormatCtx, 0, src.c_str(), 0);

    // Find the first video stream
    videoStream = -1;
    for (i = 0; i<pFormatCtx->nb_streams; i++)
        if (pFormatCtx->streams[i]->codec->codec_type == AVMEDIA_TYPE_VIDEO) {
            videoStream = i;
            break;
        }
    if (videoStream == -1)
    {
        throw std::exception("...");
        //return -1; // Didn't find a video stream
    }

                   // Get a pointer to the codec context for the video stream
    pCodecCtxOrig = pFormatCtx->streams[videoStream]->codec;
    // Find the decoder for the video stream
    pCodec = avcodec_find_decoder(pCodecCtxOrig->codec_id);
    if (pCodec == NULL) {
        fprintf(stderr, "Unsupported codec!\n");
        throw std::exception("...");
        //return -1; // Codec not found
    }
    // Copy context
    pCodecCtx = avcodec_alloc_context3(pCodec);
    if (avcodec_copy_context(pCodecCtx, pCodecCtxOrig) != 0) {
        fprintf(stderr, "Couldn't copy codec context");
        throw std::exception("...");
        //return -1; // Error copying codec context
    }

    // Open codec
    if (avcodec_open2(pCodecCtx, pCodec, NULL) < 0)
    {
        throw std::exception("...");
        //return -1; // Could not open codec
    }

    // Allocate video frame
    pFrame = av_frame_alloc();

    // Allocate an AVFrame structure
    pFrameRGB = av_frame_alloc();
    if (pFrameRGB == NULL)
    {
        throw std::exception("...");
        //return -1;
    }

    // Determine required buffer size and allocate buffer
    numBytes = avpicture_get_size(AV_PIX_FMT_RGB24, pCodecCtx->width,
        pCodecCtx->height);
    buffer = (uint8_t *)av_malloc(numBytes * sizeof(uint8_t));

    // Assign appropriate parts of buffer to image planes in pFrameRGB
    // Note that pFrameRGB is an AVFrame, but AVFrame is a superset
    // of AVPicture
    avpicture_fill((AVPicture *)pFrameRGB, buffer, AV_PIX_FMT_RGB24,
        pCodecCtx->width, pCodecCtx->height);

    // initialize SWS context for software scaling
    sws_ctx = sws_getContext(pCodecCtx->width,
        pCodecCtx->height,
        pCodecCtx->pix_fmt,
        pCodecCtx->width,
        pCodecCtx->height,
        AV_PIX_FMT_RGB24,
        SWS_BILINEAR,
        NULL,
        NULL,
        NULL
    );

    // Read frames and save first five frames to disk
    i = 0;
    while (av_read_frame(pFormatCtx, &packet) >= 0) {
        // Is this a packet from the video stream?
        if (packet.stream_index == videoStream) {
            // Decode video frame
            avcodec_decode_video2(pCodecCtx, pFrame, &frameFinished, &packet);

            // Did we get a video frame?
            if (frameFinished) {
                // Convert the image from its native format to RGB
                sws_scale(sws_ctx, (uint8_t const * const *)pFrame->data,
                    pFrame->linesize, 0, pCodecCtx->height,
                    pFrameRGB->data, pFrameRGB->linesize);

                // Save the frame to disk
                //if (++i <= 5)
                //    SaveFrame(pFrameRGB, pCodecCtx->width, pCodecCtx->height, i);

                cv::Mat x(pCodecCtx->height, pCodecCtx->width, CV_8UC3, pFrameRGB->data[0], pFrameRGB->linesize[0]);
                cv::imshow("x", x);
                cv::waitKey(1);
            }
        }

        // Free the packet that was allocated by av_read_frame
        av_free_packet(&packet);
    }

    // Free the RGB image
    av_free(buffer);
    av_frame_free(&pFrameRGB);

    // Free the YUV frame
    av_frame_free(&pFrame);

    // Close the codecs
    avcodec_close(pCodecCtx);
    avcodec_close(pCodecCtxOrig);

    // Close the video file
    avformat_close_input(&pFormatCtx);

}

VideoCameraSource::~VideoCameraSource()
{
    Close();
}

void VideoCameraSource::PrepareNextFrame()
{
    std::lock_guard<std::mutex> lock(mutex_);
    camera_->grab();

    int fps = static_cast<int>(camera_->get(cv::CAP_PROP_FPS));
    std::this_thread::sleep_for(std::chrono::milliseconds(1000/fps));

    frame_counter_++;
    if (frame_counter_ >= camera_->get(cv::CAP_PROP_FRAME_COUNT))
    {
        Close();

        {
            std::lock_guard<std::mutex> lock(mutex_);
            camera_ = std::make_unique<cv::VideoCapture>(src_);
            camera_->grab();
            frame_counter_ = 1;
        }
    }


    camera_->retrieve(frame_);
}

void VideoCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
    std::lock_guard<std::mutex> lock(mutex_);
    if (camera_ && IsOpen())
    {
        auto buffer_size = GetFrameWidth() * GetFrameHeight() * GetFrameChannels();
        memcpy(left_buffer, frame_.data, buffer_size);
        memcpy(right_buffer, frame_.data, buffer_size);
    }
}

void VideoCameraSource::Open()
{
    std::lock_guard<std::mutex> lock(mutex_);
    if (camera_ && !IsOpen())
    {
        bool open_success = camera_->open(src_);

        if (!open_success)
        {
            throw std::exception("Unable to open OpenCV camera");
        }
    }
}

void VideoCameraSource::Close()
{
    std::lock_guard<std::mutex> lock(mutex_);
    if (camera_ && IsOpen())
    {
        camera_->release();
    }
}

bool VideoCameraSource::IsOpen() const
{
    return camera_->isOpened();
}

int VideoCameraSource::GetFrameWidth() const
{
    return static_cast<int>(camera_->get(cv::CAP_PROP_FRAME_WIDTH));
}

int VideoCameraSource::GetFrameHeight() const
{
    return static_cast<int>(camera_->get(cv::CAP_PROP_FRAME_HEIGHT));
}

int VideoCameraSource::GetFrameChannels() const
{
    //return static_cast<int>(camera_->get(cv::CAP_PROP_ ? ));
    return 3;
}

float VideoCameraSource::GetFocalLength() const
{
    return 0.1f;
}

nlohmann::json VideoCameraSource::GetProperties() const
{
    // NYI
    return nlohmann::json();
}

void VideoCameraSource::SetProperties(const nlohmann::json & json_config)
{
    // NYI
}

#pragma warning(pop)
