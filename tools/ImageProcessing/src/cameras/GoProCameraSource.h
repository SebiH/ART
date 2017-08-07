#pragma once
#pragma comment(lib, "ws2_32.lib")

#include <memory>
#include <string>
#include <thread>

#include <opencv2/videoio.hpp>
#include "cameras/CameraSourceInterface.h"

namespace ImageProcessing
{
    class GoProCameraSource : public CameraSourceInterface
    {
    private:
        std::unique_ptr<cv::VideoCapture> camera_;
        std::string src_;
        int port_;

        std::thread thread_;
        bool run_thread_ = false;

        void KeepAlive();

    public:
        GoProCameraSource(const std::string &src, const int port);
        ~GoProCameraSource();

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
