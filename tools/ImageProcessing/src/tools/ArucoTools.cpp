#include "tools/ArucoTools.h"

#include "aruco/aruco.h"

using namespace ImageProcessing;

std::vector<cv::Mat> ArucoTools::GenerateMarkers(std::string dictionary_name, int pixel_size)
{
	auto dictionary = aruco::Dictionary::loadPredefined(dictionary_name);
	auto output = std::vector<cv::Mat>();
	auto counter = 0;

	for (const auto &m : dictionary.getMapCode())
	{
		output.push_back(dictionary.getMarkerImage_id(counter, pixel_size));
		counter++;
	}

	return output;
}

std::vector<std::string> ArucoTools::GetDictionaries()
{
	return aruco::Dictionary::getDicTypes();
}
