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

namespace Params
{
	static const auto SizeX = "sizeX";
	static const auto SizeY = "sizeY";
	static const auto MarkerIds = "markerIds";
	static const auto ImageFilename = "imgFilename";
	static const auto ConfigFilename = "configFilename";
	static const auto DictionaryName = "dictionaryName";

	static const auto PixelSize = "pixelSize";
	static const auto InterMarkerDistance = "interMarkerDistance";
}

void ArucoTools::GenerateMarkerMap(nlohmann::json config)
{

	int size_x = config[Params::SizeX].get<int>();
	int size_y = config[Params::SizeY].get<int>();

	auto img_filename = config[Params::ImageFilename].get<std::string>();
	auto config_filename = config[Params::ConfigFilename].get<std::string>();

	auto dict = aruco::Dictionary::loadPredefined(config[Params::DictionaryName].get<std::string>());
	const int type_markermap = 0; // 1 for chessboard
	
	int pixel_size = 500;
	if (config.count(Params::PixelSize))
	{
		pixel_size = config[Params::PixelSize].get<int>();
	}

	float inter_marker_distance = 0.2f;
	if (config.count(Params::InterMarkerDistance))
	{
		inter_marker_distance = config[Params::InterMarkerDistance].get<float>();
	}

	if ((inter_marker_distance > 1.f) || (inter_marker_distance < 0.f))
	{
		throw std::exception("InterMarkerDistance has to be between 0 and 1");
	}

	auto marker_ids = config[Params::MarkerIds].get<std::vector<int>>();
	aruco::MarkerMap markermap = dict.createMarkerMap(cv::Size(size_x, size_y), pixel_size, pixel_size * inter_marker_distance, marker_ids, type_markermap == 1);
	//create a printable image to save
	cv::Mat img_markermap = markermap.getImage();

	//save
	markermap.saveToFile(config_filename);
	cv::imwrite(img_filename, img_markermap);
}
