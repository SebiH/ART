#include "cameras/VideoCameraSource.h"

#include <chrono>
#include <thread>
#include <opencv2/imgproc.hpp>
#include "utils/Logger.h"

using namespace ImageProcessing;

VideoCameraSource::VideoCameraSource(const std::string &src)
    : src_(src), frame_counter_(0), frame_()
{
    camera_ = std::make_unique<cv::VideoCapture>(src);

    if (GetFrameWidth() == 0)
    {
        throw std::exception("Could not open video file");
    }
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
