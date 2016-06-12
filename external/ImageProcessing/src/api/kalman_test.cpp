#include <map>
#include <memory>
#include <opencv2/video/tracking.hpp>

#include "Unity/IUnityInterface.h"

int _handleCounter = 0;
std::map<int, std::unique_ptr<cv::KalmanFilter>> _filters;


extern "C" UNITY_INTERFACE_EXPORT int init(const int dynamParams, const int measureParams, const int controlParams)
{
	auto kalmanFilter = std::make_unique<cv::KalmanFilter>(dynamParams, measureParams, controlParams, CV_64F);
	auto handle = _handleCounter++;

	_filters[handle] = std::move(kalmanFilter);

	return handle;
}

extern "C" UNITY_INTERFACE_EXPORT int* predict(const int handle)
{
	auto result = _filters[handle]->predict();
	return 0;
}

extern "C" UNITY_INTERFACE_EXPORT void correct(const int handle, const int* measurement, const int measureCount)
{
	auto correctionMatrix = cv::Mat(measureCount / 2, measureCount / 2, CV_64F);

	_filters[handle]->correct(correctionMatrix);
}
