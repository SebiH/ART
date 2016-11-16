#pragma once

#include <string>
#include <vector>

#include <opencv2/opencv.hpp>
#include <json/json.hpp>

namespace ImageProcessing
{
	class ArucoTools
	{
	public:
		static std::vector<cv::Mat> GenerateMarkers(std::string dictionary_name, int pixel_size);
		static std::vector<std::string> GetDictionaries();
		
		/**
		 *	int sizeX, int sizeY, std::string imgFilename, std::string configFilename
		 *	{
		 *		sizeX: number,
		 *		sizeY: number,
		 *		markerIds: number[],
		 *		imgFilename: string,
		 *		configFilename: string,
		 *		dictionaryName: string,
		 *		
		 *		pixelSize: number?,
		 *		interMarkerDistance: number(0,1)?
		 *  }
		 */
		static void GenerateMarkerMap(nlohmann::json config);
	};
}
