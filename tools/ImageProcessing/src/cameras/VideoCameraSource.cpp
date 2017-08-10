#include "cameras/VideoCameraSource.h"

#include <chrono>
#include <thread>
#include <opencv2/imgproc.hpp>
#include "utils/Logger.h"

#pragma warning(push)
#pragma warning(disable: 4996)

using namespace ImageProcessing;

VideoCameraSource::VideoCameraSource(const std::string &src, TimeCallback time_hack)
    : src_(src), frame_counter_(0), frame_(), time_hack_(time_hack),
      last_frame_time_(std::chrono::high_resolution_clock::now())
{
    // Register all formats and codecs
    av_register_all();
    Open();
}

VideoCameraSource::~VideoCameraSource()
{
    Close();
}

void VideoCameraSource::PrepareNextFrame()
{
    bool hasFrame = false;

    {
        std::lock_guard<std::mutex> lock(mutex_);

        // Read frames and save first five frames to disk
        while (!hasFrame && av_read_frame(pFormatCtx, &packet) >= 0)
        {
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

                    frame_ = cv::Mat(pCodecCtx->height, pCodecCtx->width, CV_8UC3, pFrameRGB->data[0], pFrameRGB->linesize[0]);
                    frame_counter_++;
                    hasFrame = true;
                }
            }

            // Free the packet that was allocated by av_read_frame
            av_free_packet(&packet);
        }
    }

    auto framerate = pCodecCtx->framerate.num / (double) pCodecCtx->framerate.den;

    if (time_hack_)
    {
        double time = frame_counter_ / framerate;
        time_hack_(time);
    }

    if (!hasFrame)
    {
        Close();
        Open();
        PrepareNextFrame();
    }

    auto time_for_frame = (int)(1000 / framerate);
    auto now = std::chrono::high_resolution_clock::now();
    auto actual_duration = std::chrono::duration_cast<std::chrono::milliseconds>(now - last_frame_time_).count();

    const int overhead = 1;

    if (actual_duration < time_for_frame - overhead)
    {
        // -2 to account for overhead
        std::this_thread::sleep_for(std::chrono::milliseconds(time_for_frame - actual_duration - overhead));
    }

    last_frame_time_ = std::chrono::high_resolution_clock::now();
}

void VideoCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
    std::lock_guard<std::mutex> lock(mutex_);
    if (IsOpen())
    {
        auto buffer_size = GetFrameWidth() * GetFrameHeight() * GetFrameChannels();
        memcpy(left_buffer, frame_.data, buffer_size);
        memcpy(right_buffer, frame_.data, buffer_size);
    }
}

void VideoCameraSource::Open()
{
    std::lock_guard<std::mutex> lock(mutex_);
    if (!IsOpen())
    {
        frame_counter_ = 0;

        // Open video file
        if (avformat_open_input(&pFormatCtx, src_.c_str(), NULL, NULL) != 0)
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
        for (i = 0; i < pFormatCtx->nb_streams; i++)
        {
            if (pFormatCtx->streams[i]->codec->codec_type == AVMEDIA_TYPE_VIDEO)
            {
                videoStream = i;
                break;
            }
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
        if (pCodec == NULL)
        {
            fprintf(stderr, "Unsupported codec!\n");
            throw std::exception("...");
            //return -1; // Codec not found
        }
        // Copy context
        pCodecCtx = avcodec_alloc_context3(pCodec);
        if (avcodec_copy_context(pCodecCtx, pCodecCtxOrig) != 0)
        {
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
        numBytes = avpicture_get_size(AV_PIX_FMT_RGB32, pCodecCtx->width, pCodecCtx->height);
        buffer = (uint8_t *)av_malloc(numBytes * sizeof(uint8_t));

        // Assign appropriate parts of buffer to image planes in pFrameRGB
        // Note that pFrameRGB is an AVFrame, but AVFrame is a superset
        // of AVPicture
        avpicture_fill((AVPicture *)pFrameRGB, buffer, AV_PIX_FMT_RGB32, pCodecCtx->width, pCodecCtx->height);

        // initialize SWS context for software scaling
        sws_ctx = sws_getContext(pCodecCtx->width,
            pCodecCtx->height,
            pCodecCtx->pix_fmt,
            pCodecCtx->width,
            pCodecCtx->height,
            AV_PIX_FMT_RGB32,
            SWS_BILINEAR,
            NULL,
            NULL,
            NULL
        );

    }
}

void VideoCameraSource::Close()
{
    std::lock_guard<std::mutex> lock(mutex_);
    if (IsOpen())
    {
        // Free the RGB image
        av_free(buffer);
        av_frame_free(&pFrameRGB);

        // Free the YUV frame
        av_frame_free(&pFrame);

        // Close the codecs
        avcodec_close(pCodecCtx);
        pCodecCtx = NULL;
        avcodec_close(pCodecCtxOrig);

        // Close the video file
        avformat_close_input(&pFormatCtx);
    }
}

bool VideoCameraSource::IsOpen() const
{
    return pCodecCtx != NULL;
}

int VideoCameraSource::GetFrameWidth() const
{
    return pCodecCtx->width;
}

int VideoCameraSource::GetFrameHeight() const
{
    return pCodecCtx->height;
}

int VideoCameraSource::GetFrameChannels() const
{
    //return static_cast<int>(camera_->get(cv::CAP_PROP_ ? ));
    return 4;
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
