#include "cameras/VideoCameraSource.h"

#include <chrono>
#include <thread>
#include <opencv2/imgproc.hpp>
#include "utils/Logger.h"

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

using namespace ImageProcessing;

VideoCameraSource::VideoCameraSource(const std::string &src)
    : src_(src), frame_counter_(0), frame_()
{
    // https://gist.github.com/yohhoy/f0444d3fc47f2bb2d0e2


    // initialize FFmpeg library
    av_register_all();
    //  av_log_set_level(AV_LOG_DEBUG);
    int ret;

    // open input file context
    AVFormatContext* inctx = nullptr;
    ret = avformat_open_input(&inctx, src.c_str(), nullptr, nullptr);
    if (ret < 0) {
        throw std::exception("...");
    }
    // retrive input stream information
    ret = avformat_find_stream_info(inctx, nullptr);
    if (ret < 0) {
        throw std::exception("...");
    }

    // find primary video stream
    AVCodec* vcodec = nullptr;
    ret = av_find_best_stream(inctx, AVMEDIA_TYPE_VIDEO, -1, -1, &vcodec, 0);
    if (ret < 0) {
        throw std::exception("...");
    }
    const int vstrm_idx = ret;
    AVStream* vstrm = inctx->streams[vstrm_idx];

    auto pCodec = avcodec_find_decoder(vstrm->codecpar->codec_id);
    auto pCodecCtx = avcodec_alloc_context3(pCodec);

    // open video decoder context
    ret = avcodec_open2(pCodecCtx, pCodec, nullptr);
    if (ret < 0) {
        throw std::exception("...");
    }

    // print input video stream informataion
    //std::cout
    //    << "infile: " << infile << "\n"
    //    << "format: " << inctx->iformat->name << "\n"
    //    << "vcodec: " << vcodec->name << "\n"
    //    << "size:   " << vstrm->codec->width << 'x' << vstrm->codec->height << "\n"
    //    << "fps:    " << av_q2d(vstrm->codec->framerate) << " [fps]\n"
    //    << "length: " << av_rescale_q(vstrm->duration, vstrm->time_base, { 1,1000 }) / 1000. << " [sec]\n"
    //    << "pixfmt: " << av_get_pix_fmt_name(vstrm->codec->pix_fmt) << "\n"
    //    << "frame:  " << vstrm->nb_frames << "\n"
    //    << std::flush;

    // initialize sample scaler
    const int dst_width = pCodecCtx->width;
    const int dst_height = pCodecCtx->height;
    //const AVPixelFormat dst_pix_fmt = AV_PIX_FMT_BGR24;
    const AVPixelFormat dst_pix_fmt = AV_PIX_FMT_BGRA;
    SwsContext* swsctx = sws_getCachedContext(
        nullptr, pCodecCtx->width, pCodecCtx->height, pCodecCtx->pix_fmt,
        dst_width, dst_height, dst_pix_fmt, SWS_BICUBIC, nullptr, nullptr, nullptr);
    if (!swsctx) {
        throw std::exception("...");
    }
    //std::cout << "output: " << dst_width << 'x' << dst_height << ',' << av_get_pix_fmt_name(dst_pix_fmt) << std::endl;

    // allocate frame buffer for output
    //AVFrame* frame = av_frame_alloc();


    /* start https://stackoverflow.com/questions/35678041/what-is-linesize-alignment-meaning  */
    AVFrame             *_pictureFrame;
    uint8_t             *_pictureFrameData;

    _pictureFrame = av_frame_alloc();
    _pictureFrame->width = pCodecCtx->width;
    _pictureFrame->height = pCodecCtx->height;
    _pictureFrame->format = dst_pix_fmt;

    //int size = av_image_get_buffer_size(_pictureFrame->format, _pictureFrame->width, _pictureFrame->height, 1);
    int align = 32;
    int size = av_image_get_buffer_size(dst_pix_fmt, dst_width, dst_height, align);


    //dont forget to free _pictureFrameData at last
    _pictureFrameData = (uint8_t*)av_malloc(size);

    av_image_fill_arrays(_pictureFrame->data,
        _pictureFrame->linesize,
        _pictureFrameData,
        dst_pix_fmt,
        dst_width,
        dst_height,
        align);
    /* end */

    //std::vector<uint8_t> framebuf(av_image_get_buffer_size(dst_pix_fmt, dst_width, dst_height, 32));
    //avpicture_fill(reinterpret_cast<AVPicture*>(frame), framebuf.data(), dst_pix_fmt, dst_width, dst_height);

    // decoding loop
    AVFrame* decframe = av_frame_alloc();
    unsigned nb_frames = 0;
    bool end_of_stream = false;
    int got_pic = 0;
    AVPacket pkt;
    do {
        if (!end_of_stream) {
            // read packet from input file
            ret = av_read_frame(inctx, &pkt);
            if (ret < 0 && ret != AVERROR_EOF) {
                throw std::exception("...");
            }
            if (ret == 0 && pkt.stream_index != vstrm_idx)
                goto next_packet;
            end_of_stream = (ret == AVERROR_EOF);
        }
        if (end_of_stream) {
            // null packet for bumping process
            av_init_packet(&pkt);
            pkt.data = nullptr;
            pkt.size = 0;
        }
        // decode video frame
        avcodec_send_packet(pCodecCtx, &pkt);
        got_pic = avcodec_receive_frame(pCodecCtx, decframe);
        //avcodec_decode_video2(vstrm->codec, decframe, &got_pic, &pkt);
        if (!got_pic)
            goto next_packet;
        // convert frame to OpenCV matrix
        sws_scale(swsctx, decframe->data, decframe->linesize, 0, decframe->height, _pictureFrame->data, _pictureFrame->linesize);
        {
            cv::Mat image(dst_height, dst_width, CV_8UC4, _pictureFrame->data, _pictureFrame->linesize[0]);
            //cv::Mat image(dst_height, dst_width, CV_8UC3, framebuf.data(), frame->linesize[0]);
            //cv::imshow("press ESC to exit", image);
            //if (cv::waitKey(1) == 0x1b)
            //    break;
        }
        //std::cout << nb_frames << '\r' << std::flush;  // dump progress
        ++nb_frames;
    next_packet:
        av_packet_unref(&pkt);
    } while (!end_of_stream || got_pic);
    //std::cout << nb_frames << " frames decoded" << std::endl;

    av_frame_free(&decframe);
    av_frame_free(&_pictureFrame);
    avcodec_close(pCodecCtx);
    avformat_close_input(&inctx);
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
