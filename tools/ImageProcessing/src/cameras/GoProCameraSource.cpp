#include <WinSock2.h>
#include <windows.h>
#include <ws2tcpip.h>

#include "cameras/GoProCameraSource.h"

#include <chrono>
#include <thread>

#include <opencv2/imgproc.hpp>

#include "utils/Logger.h"

using namespace ImageProcessing;

GoProCameraSource::GoProCameraSource(const std::string &src, const int port)
    : src_(src), port_(port)
{
    camera_ = std::make_unique<cv::VideoCapture>();

    WSADATA wsaData;
    auto wVersionRequested = MAKEWORD(2, 2);
    auto err = WSAStartup(wVersionRequested, &wsaData);
    if (err != 0) {
        throw std::exception((std::string("WSAStartup failed with error: ") + std::to_string(err)).c_str());
    }
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

        run_thread_ = true;
        thread_ = std::thread(&GoProCameraSource::KeepAlive, this);
    }
}

void GoProCameraSource::Close()
{
    if (camera_ && IsOpen())
    {
        camera_->release();
        camera_ = nullptr;
        run_thread_ = false;
        thread_.join();
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

void GoProCameraSource::KeepAlive()
{
    // See: https://stackoverflow.com/a/24560310/4090817
    // See: https://gist.github.com/Seanmatthews/261d1e3c59237fef7be3a148a5713992 

    int result = 0;
    SOCKET sock = socket(AF_INET, SOCK_DGRAM, 0);

    char szIP[100];

    //sockaddr_in addrListen = {}; // zero-int, sin_port is 0, which picks a random port for bind.
    //addrListen.sin_family = AF_INET;
    //result = bind(sock, (sockaddr*)&addrListen, sizeof(addrListen));
    //if (result == -1)
    //{
    //    int lasterror = errno;
    //    std::cout << "error: " << lasterror;
    //    exit(1);
    //}


    sockaddr_storage addrDest = {};


    addrinfo *result_list = nullptr;
    addrinfo hints = {};
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_DGRAM; // without this flag, getaddrinfo will return 3x the number of addresses (one for each socket type).
    result = getaddrinfo(src_.c_str(), std::to_string(port_).c_str(), &hints, &result_list);
    if (result == 0)
    {
        //ASSERT(result_list->ai_addrlen <= sizeof(sockaddr_in));
        memcpy(&addrDest, result_list->ai_addr, result_list->ai_addrlen);
        freeaddrinfo(result_list);
    }

    if (result != 0)
    {
        int lasterror = errno;
        std::cout << "error: " << lasterror;
        exit(1);
    }

    const char* msg = "_GPHD_:0:0:2:0.000000\n";
    size_t msg_length = strlen(msg);


    while (run_thread_ && result >= 0)
    {
        result = sendto(sock, msg, msg_length, 0, (sockaddr*)&addrDest, sizeof(addrDest));
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }

    closesocket(sock);
}
