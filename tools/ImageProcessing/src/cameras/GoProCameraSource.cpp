#include "cameras/GoProCameraSource.h"

#include <chrono>
#include <thread>

#include <boost/asio/io_service.hpp>
#include <boost/asio/ip/udp.hpp>
#include <boost/asio/placeholders.hpp>
#include <boost/bind.hpp>
#include <opencv2/imgproc.hpp>

#include "utils/Logger.h"

using namespace ImageProcessing;

GoProCameraSource::GoProCameraSource(const std::string &src, const int port)
    : src_(src), port_(port)
{
    camera_ = std::make_unique<cv::VideoCapture>();
}

GoProCameraSource::~GoProCameraSource()
{
    Close();
}

void GoProCameraSource::PrepareNextFrame()
{
    camera_->grab();
    std::this_thread::sleep_for(std::chrono::milliseconds(10));
}

void GoProCameraSource::GrabFrame(unsigned char *left_buffer, unsigned char *right_buffer)
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

void GoProCameraSource::Open()
{
    if (camera_ && !IsOpen())
    {
        bool open_success = camera_->open(std::string("udp://@") + src_ + std::string(":") + std::to_string(port_));

        if (!open_success)
        {
            throw std::exception("Unable to open GoPro camera");
        }

        thread_ = std::thread(&GoProCameraSource::KeepAlive, this);
    }
}

void GoProCameraSource::Close()
{
    if (camera_ && IsOpen())
    {
        camera_->release();
        camera_ = nullptr;
    }
}

bool GoProCameraSource::IsOpen() const
{
    return camera_->isOpened();
}

int GoProCameraSource::GetFrameWidth() const
{
    return static_cast<int>(camera_->get(cv::CAP_PROP_FRAME_WIDTH));
}

int GoProCameraSource::GetFrameHeight() const
{
    return static_cast<int>(camera_->get(cv::CAP_PROP_FRAME_HEIGHT));
}

int GoProCameraSource::GetFrameChannels() const
{
    //return static_cast<int>(camera_->get(cv::CAP_PROP_ ? ));
    return 4;
}

float GoProCameraSource::GetFocalLength() const
{
    return 0.1f;
}

nlohmann::json GoProCameraSource::GetProperties() const
{
    // NYI
    return nlohmann::json();
}

void GoProCameraSource::SetProperties(const nlohmann::json & json_config)
{
    // NYI
}

void GoProCameraSource::SendToCB(const boost::system::error_code& ec)
{
    DebugLog(std::to_string(ec.value()));
}

void GoProCameraSource::KeepAlive()
{
    // See: https://gist.github.com/Seanmatthews/261d1e3c59237fef7be3a148a5713992
    try
    {
        boost::asio::io_service ioService;
        boost::asio::ip::udp::resolver resolver(ioService);
        boost::asio::ip::udp::endpoint dest(boost::asio::ip::address::from_string(src_), 8554);
        boost::asio::ip::udp::socket sock(ioService, boost::asio::ip::udp::v4());

        for (;;)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
            sock.async_send_to(boost::asio::buffer("_GPHD_:0:0:2:0.000000\n", 22), dest,
                boost::bind(&GoProCameraSource::SendToCB, boost::asio::placeholders::error));
        }
    }
    catch (boost::exception& e)
    {
        DebugLog("KeepAlive socket error");
    }
}
