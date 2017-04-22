#include "ArToolkitProcessor.h"

#include <stdio.h>
#include <iostream>
#include <vector>
#include <opencv2/imgproc.hpp>
#include <AR/param.h>

#include "frames/JsonFrameData.h"
#include "utils/Logger.h"

using namespace ImageProcessing;
using json = nlohmann::json;

/*
 *	Generate markers via http://www.artoolworks.com/support/applications/marker/
 */

ArToolkitProcessor::ArToolkitProcessor(std::string config)
	: initialized_size_(-1, -1, -1)
{
	auto json_config = json::parse(config);

	calib_path_left_ = json_config["calibration_left"].get<std::string>();
	calib_path_right_ = json_config["calibration_right"].get<std::string>();

	SetProperties(json_config);
}

ArToolkitProcessor::~ArToolkitProcessor()
{
	Cleanup();
}


std::shared_ptr<const FrameData> ArToolkitProcessor::Process(const std::shared_ptr<const FrameData> &frame)
{
	if (initialized_size_ != frame->size)
	{
		if (!is_first_initialization_)
		{
			Cleanup();
		}

		try
		{
			Initialize(frame->size.width, frame->size.height, frame->size.depth);
			is_first_initialization_ = false;
			initialized_size_ = frame->size;
		}
		catch (const std::exception &e)
		{
			DebugLog(std::string("Could not initialize ARToolkit: ") + e.what());
		}
	}

	// Detect the markers in the video frame.
	if (arDetectMarker(ar_handle_l_, frame->buffer_left.get()) < 0 || arDetectMarker(ar_handle_r_, frame->buffer_right.get()) < 0)
	{
		DebugLog("Error detecting markers");
		return frame;
	}

	// Get detected markers
	auto marker_info_l = arGetMarker(ar_handle_l_);
	auto marker_info_r = arGetMarker(ar_handle_r_);
	auto marker_num_l = arGetMarkerNum(ar_handle_l_);
	auto marker_num_r = arGetMarkerNum(ar_handle_r_);

	bool marker_detected = false;
	json payload{
		{ "markers_left", json::array() },
		{ "markers_right", json::array() }
	};

	for (auto &filter : filters_l_)
	{
		filter.missed_frames++;
	}

	for (auto &filter : filters_r_)
	{
		filter.missed_frames++;
	}

	for (auto i = 0; i < marker_num_l; i++)
	{
		try
		{
			auto info = marker_info_l[i];

			if (info.cf > min_confidence_)
			{
				auto pose = ProcessMarkerInfo(info, filters_l_[info.id]);
				DrawMarker(info, frame->size, frame->buffer_left.get());
				payload["markers_left"].push_back(pose);
				marker_detected = true;
				filters_l_[info.id].missed_frames = 0;
			}
		}
		catch (const std::exception &e)
		{
			DebugLog(std::string("Failed to process marker: ") + e.what());
		}
	}


	for (auto i = 0; i < marker_num_r; i++)
	{
		auto info = marker_info_r[i];

		try
		{
			if (info.cf > min_confidence_)
			{
				auto pose = ProcessMarkerInfo(info, filters_r_[info.id]);
				DrawMarker(info, frame->size, frame->buffer_right.get());
				payload["markers_right"].push_back(pose);
				marker_detected = true;
				filters_r_[info.id].missed_frames = 0;
			}
		}
		catch (const std::exception &e)
		{
			DebugLog(std::string("Failed to process marker: ") + e.what());
		}
	}


	if (marker_detected)
	{
		return std::make_shared<const JsonFrameData>(frame.get(), payload);
	}
	else
	{
		return frame;
	}
}


json ArToolkitProcessor::ProcessMarkerInfo(ARMarkerInfo &info, const MarkerFilter &filter)
{
	ARdouble transform_matrix[3][4];
	arGetTransMatSquare(ar_3d_handle_l_, &info, marker_size_, transform_matrix);

	if (use_filters_)
	{
		arFilterTransMat(filter.trans, transform_matrix, filter.missed_frames >= max_missed_frames_ ? 1 : 0);
	}

	return json{
		{ "id", info.id },
		{ "confidence", info.cf },
		{ "pos", { info.pos[0], info.pos[1] }},
		{ "corners",
			{
				{"topleft", { info.vertex[0][0], info.vertex[0][1] } },
				{"topright", { info.vertex[1][0], info.vertex[1][1] } },
				{"bottomleft", { info.vertex[2][0], info.vertex[2][1] } },
				{"bottomright", { info.vertex[3][0], info.vertex[3][1] } },
			}
		},
		{ "transform_matrix", {
				{"m00", transform_matrix[0][0]},
				{"m01", transform_matrix[0][1]},
				{"m02", transform_matrix[0][2]},
				{"m03", transform_matrix[0][3]},

				{"m10", transform_matrix[1][0]},
				{"m11", transform_matrix[1][1]},
				{"m12", transform_matrix[1][2]},
				{"m13", transform_matrix[1][3]},

				{"m20", transform_matrix[2][0]},
				{"m21", transform_matrix[2][1]},
				{"m22", transform_matrix[2][2]},
				{"m23", transform_matrix[2][3]}
			}
		}
	};
}



void ArToolkitProcessor::DrawMarker(const ARMarkerInfo &marker, const FrameSize &size, unsigned char *buffer)
{
	cv::Mat img = cv::Mat(cv::Size(size.width, size.height), size.CvType(), buffer);
	cv::circle(img, cv::Point(marker.pos[0], marker.pos[1]), 5, cv::Scalar(0, 0, 255, 255), 1);
	cv::putText(img, std::to_string(marker.id), cv::Point(marker.pos[0] + 10, marker.pos[1] + 10), CV_FONT_HERSHEY_PLAIN, 1, cv::Scalar(0, 0, 255, 255));

	for (int j = 0; j < 4; j++)
	{
		auto cornerPos = cv::Point(marker.vertex[j][0], marker.vertex[j][1]);
		cv::circle(img, cornerPos, 3, cv::Scalar(255, 0, 0, 255), 1);
	}
}



void ArToolkitProcessor::Initialize(const int sizeX, const int sizeY, const int depth)
{
	// TODO: reinitialization might break things!
	// Cameras
	if (!SetupCamera(calib_path_left_, sizeX, sizeY, &c_param_lt_l_))
	{
		throw std::exception("Unable to setup left camera");
	}

	if (!SetupCamera(calib_path_right_, sizeX, sizeY, &c_param_lt_r_))
	{
		throw std::exception("Unable to setup right camera");
	}

	ar_handle_l_ = arCreateHandle(c_param_lt_l_);
	ar_handle_r_ = arCreateHandle(c_param_lt_r_);
	if (!ar_handle_l_ || !ar_handle_r_)
	{
		DebugLog("Error creating AR handle.");
		throw std::exception("Error - See log.");
	}


	int pattern_error = 0;
	AR_MATRIX_CODE_TYPE matrixType = AR_MATRIX_CODE_4x4_BCH_13_9_3;
	pattern_error -= arSetMatrixCodeType(ar_handle_l_, matrixType);
	pattern_error -= arSetPatternDetectionMode(ar_handle_l_, AR_MATRIX_CODE_DETECTION);
	pattern_error -= arSetMatrixCodeType(ar_handle_r_, matrixType);
	pattern_error -= arSetPatternDetectionMode(ar_handle_r_, AR_MATRIX_CODE_DETECTION);
	pattern_error -= arSetBorderSize(ar_handle_l_, 0.1f); // Default = 0.25f
	pattern_error -= arSetBorderSize(ar_handle_r_, 0.1f); // Default = 0.25f

	if (pattern_error < 0)
	{
		DebugLog("Error setting matrix type");
		throw std::exception("Error - See log.");
	}

	const int MAX_MARKERS = 512;
	for (int i = 0; i < MAX_MARKERS; i++)
	{
		MarkerFilter mf;
		mf.id = i;
		mf.trans = arFilterTransMatInit(90, 15);
		filters_l_.push_back(mf);
		filters_r_.push_back(mf);
	}


	AR_PIXEL_FORMAT format;

	if (depth == 1) { format = AR_PIXEL_FORMAT_MONO; } // TODO: might not be correct?
	if (depth == 3) { format = AR_PIXEL_FORMAT_BGR; }
	else { format = AR_PIXEL_FORMAT_BGRA; }

	if (arSetPixelFormat(ar_handle_l_, format) < 0 || arSetPixelFormat(ar_handle_r_, format) < 0)
	{
		DebugLog("Error setting pixel format.");
		throw std::exception("Error - See log.");
	}

	ar_3d_handle_l_ = ar3DCreateHandle(&c_param_lt_l_->param);
	ar_3d_handle_r_ = ar3DCreateHandle(&c_param_lt_r_->param);
	if (!ar_3d_handle_l_ || !ar_3d_handle_r_)
	{
		DebugLog("Error creating 3D handle.");
		throw std::exception("Error - See log.");
	}
	

	//
	// Other ARToolKit setup.
	//

	int extraction_error = 0;
	extraction_error -= arSetMarkerExtractionMode(ar_handle_l_, AR_USE_TRACKING_HISTORY_V2);
	extraction_error -= arSetMarkerExtractionMode(ar_handle_r_, AR_USE_TRACKING_HISTORY_V2);

	if (extraction_error < 0)
	{
		DebugLog("Error setting marker extraction mode");
		throw std::exception("Error - See log.");
	}
}

bool ArToolkitProcessor::SetupCamera(const std::string filename, const int sizeX, const int sizeY, ARParamLT ** cparamLT_p)
{
	ARParam cparam;

	if (arParamLoad(filename.c_str(), 1, &cparam) < 0)
	{
		DebugLog(std::string("Unable to load ") + filename);
		return false;
	}

	if (cparam.xsize != sizeX || cparam.ysize != sizeY)
	{
		arParamChangeSize(&cparam, sizeX, sizeY, &cparam);
	}

	if ((*cparamLT_p = arParamLTCreate(&cparam, AR_PARAM_LT_DEFAULT_OFFSET)) == NULL)
	{
		DebugLog("Unable to create ParamLT");
		return false;
	}

	return true;
}

void ArToolkitProcessor::Cleanup()
{
	arPattDetach(ar_handle_l_);
	arPattDetach(ar_handle_r_);
	ar3DDeleteHandle(&ar_3d_handle_l_);
	ar3DDeleteHandle(&ar_3d_handle_r_);
	arDeleteHandle(ar_handle_l_);
	arDeleteHandle(ar_handle_r_);
	arParamLTFree(&c_param_lt_l_);
	arParamLTFree(&c_param_lt_r_);
}


nlohmann::json ArToolkitProcessor::GetProperties()
{
	return json{
		{ "min_confidence", min_confidence_ },
		{ "marker_size", marker_size_ },
		{ "use_filters", use_filters_ },
		{ "max_missed_frames", max_missed_frames_ }
	};
}



void ArToolkitProcessor::SetProperties(const nlohmann::json &config)
{
	if (config.count("min_confidence"))
	{
		min_confidence_ = config["min_confidence"].get<double>();
	}

	if (config.count("marker_size"))
	{
		marker_size_ = config["marker_size"].get<double>();
	}

	if (config.count("use_filters"))
	{
		use_filters_ = config["use_filters"].get<bool>();
	}

	if (config.count("max_missed_frames"))
	{
		use_filters_ = config["max_missed_frames"].get<bool>();
	}
}
