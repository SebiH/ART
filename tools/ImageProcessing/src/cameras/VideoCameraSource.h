#pragma once

#include <memory>
#include <mutex>
#include <opencv2/videoio.hpp>
#include "cameras/CameraSourceInterface.h"

namespace ImageProcessing
{
    class VideoCameraSource : public CameraSourceInterface
    {
    private:
        std::mutex mutex_;
        int frame_counter_;
        std::string src_;
        std::unique_ptr<cv::VideoCapture> camera_;

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
