/**
 *	Only relevant for GUI project, not for use in unity
 */

#include <json/json.hpp>
#include <Unity/IUnityInterface.h>
#include "tools/ArucoTools.h"
#include "utils/Logger.h"

using namespace ImageProcessing;
using json = nlohmann::json;

typedef void(__stdcall * ToolCallback) (const char *json_str);

extern "C" UNITY_INTERFACE_EXPORT void GetArucoDictionaries(ToolCallback callback)
{
	auto dictionaries = ArucoTools::GetDictionaries();
	json output(dictionaries);
	callback(output.dump().c_str());
}

extern "C" UNITY_INTERFACE_EXPORT void GenerateArucoMarkers(const char* dictionary_name, const char *output_dir, int pixel_size)
{
	auto markers = ArucoTools::GenerateMarkers(std::string(dictionary_name), pixel_size);
	int counter = 0;

	std::string dictionary_filename = std::string(dictionary_name);
	std::transform(dictionary_filename.begin(), dictionary_filename.end(), dictionary_filename.begin(), ::tolower);

	for (const auto &marker : markers)
	{
		std::string number = std::to_string(counter);
		counter++;

		while (number.size() != 5) number = "0" + number;

		std::stringstream name;
		name << output_dir << "/" << dictionary_filename << "_" << number << ".png";
		cv::imwrite(name.str(), marker);
	}
}

extern "C" UNITY_INTERFACE_EXPORT void GenerateMarkerMap(const char* json_config_str)
{
	try
	{
		auto config = json::parse(json_config_str);
		ArucoTools::GenerateMarkerMap(config);
	}
	catch (const std::exception &e)
	{
		DebugLog(e.what());
	}
}
