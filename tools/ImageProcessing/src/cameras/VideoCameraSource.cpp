#include "cameras/VideoCameraSource.h"

#include <chrono>
#include <thread>
#include <opencv2/imgproc.hpp>
#include "utils/Logger.h"

using namespace ImageProcessing;

VideoCameraSource::VideoCameraSource(const std::string &src)
    : src_(src)
{
    camera_ = std::make_unique<cv::VideoCapture>(src);
}

VideoCameraSource::~VideoCameraSource()
{
    Close();
}

void VideoCameraSource::PrepareNextFrame()
{
    camera_->grab();
    int fps = static_cast<int>(camera_->get(cv::CAP_PROP_FPS));
    std::this_thread::sleep_for(std::chrono::milliseconds(1000/fps));
}

void VideoCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
    cv::Mat frame;
    camera_->retrieve(frame);

    if (frame.channels() == 3)
    {
        // convert back to 4-channel BGRA for easier unity handling
        cv::cvtColor(frame, frame, CV_BGR2BGRA);
    }
    else if (frame.channels() != GetFrameChannels())
    {
        DebugLog("Camera provided unknown amount of channels");
        //throw std::exception("Camera provided unexpected amount of channels");
    }

    auto buffer_size = GetFrameWidth() * GetFrameHeight() * GetFrameChannels();
    memcpy(left_buffer, frame.data, buffer_size);
    memcpy(right_buffer, frame.data, buffer_size);
}

void VideoCameraSource::Open()
{
    if (camera_ && !IsOpen())
    {
        bool open_success = camera_->open(0);

        if (!open_success)
        {
            throw std::exception("Unable to open OpenCV camera");
        }
    }
}

void VideoCameraSource::Close()
{
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
