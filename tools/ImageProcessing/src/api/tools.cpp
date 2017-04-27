/**
 *	Only relevant for GUI project, not for use in unity
 */

#include <json/json.hpp>
#include <Unity/IUnityInterface.h>
#include "cameras/ActiveCamera.h"
#include "tools/ArToolkitCalibrator.h"
#include "tools/ArToolkitStereoCalibrator.h"
#include "tools/StandardCalibrator.h"
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

	auto dictionary_filename = std::string(dictionary_name);
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

extern "C" UNITY_INTERFACE_EXPORT void PerformStandardCalibration(const char* save_filename, int corners_num_x, int corners_num_y, int calib_image_count, double pattern_width, double screen_size_margin)
{
	try
	{
		auto cam = ActiveCamera::Instance()->GetSource();

		auto calibrator = StandardCalibrator();
		calibrator.corners_num_x = corners_num_x;
		calibrator.corners_num_y = corners_num_y;
		calibrator.calib_image_count = calib_image_count;
		calibrator.pattern_width = pattern_width;
		calibrator.screen_size_margin = screen_size_margin;

		calibrator.Calibrate(cam, std::string(save_filename));
	}
	catch (const std::exception &e)
	{
		DebugLog(e.what());
	}
}


extern "C" UNITY_INTERFACE_EXPORT void PerformArToolkitCalibration(const char* save_filename, int corners_num_x, int corners_num_y, int calib_image_count, double pattern_width, double screen_size_margin)
{
	try
	{
		auto cam = ActiveCamera::Instance()->GetSource();

		auto calibrator = ArToolkitCalibrator();
		calibrator.corners_num_x = corners_num_x;
		calibrator.corners_num_y = corners_num_y;
		calibrator.calib_image_count = calib_image_count;
		calibrator.pattern_width = pattern_width;
		calibrator.screen_size_margin = screen_size_margin;

		calibrator.Calibrate(cam, std::string(save_filename));
	}
	catch (const std::exception &e)
	{
		DebugLog(e.what());
	}
}

extern "C" UNITY_INTERFACE_EXPORT void PerformArToolkitStereoCalibration(const char* save_filename, int corners_num_x, int corners_num_y, int calib_image_count, double pattern_width, double screen_size_margin, const char* calib_left, const char *calib_right)
{
	try
	{
		auto cam = ActiveCamera::Instance()->GetSource();

		auto calibrator = ArToolkitStereoCalibrator();
		calibrator.corners_num_x = corners_num_x;
		calibrator.corners_num_y = corners_num_y;
		calibrator.calib_image_count = calib_image_count;
		calibrator.pattern_width = pattern_width;
		calibrator.screen_size_margin = screen_size_margin;
		calibrator.calibration_file_left = std::string(calib_left);
		calibrator.calibration_file_right = std::string(calib_right);

		calibrator.Calibrate(cam, std::string(save_filename));
	}
	catch (const std::exception &e)
	{
		DebugLog(e.what());
	}
}
