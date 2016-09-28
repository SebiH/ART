#pragma once

#include <string>
#include <vector>

#include <opencv2/opencv.hpp>

namespace ImageProcessing
{
	class ArucoTools
	{
	public:
		static std::vector<cv::Mat> GenerateMarkers(std::string dictionary_name, int pixel_size);
		static std::vector<std::string> GetDictionaries();
	};
}
