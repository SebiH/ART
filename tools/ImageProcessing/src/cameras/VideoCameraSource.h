#pragma once

#include <memory>
#include <mutex>
#include <opencv2/videoio.hpp>
#include "cameras/CameraSourceInterface.h"


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

namespace ImageProcessing
{
    class VideoCameraSource : public CameraSourceInterface
    {
    private:
        std::mutex mutex_;
        cv::Mat frame_;
        int frame_counter_;
        std::string src_;


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

    public:
        VideoCameraSource(const std::string &src);
        ~VideoCameraSource();

        // Inherited via CameraSourceInterface
        virtual void PrepareNextFrame() override;
        virtual void GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer) override;
        virtual void Open() override;
        virtual void Close() override;
        virtual bool IsOpen() const override;
        virtual int GetFrameWidth() const override;
        virtual int GetFrameHeight() const override;
        virtual int GetFrameChannels() const override;
        virtual float GetFocalLength() const override;

        virtual nlohmann::json GetProperties() const override;
        virtual void SetProperties(const nlohmann::json &json_config) override;
    };
}
