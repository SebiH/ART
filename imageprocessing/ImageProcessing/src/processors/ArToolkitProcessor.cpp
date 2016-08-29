#include "ArToolkitProcessor.h"

#include <vector>
#include <opencv2/imgproc.hpp>
#include <AR/param.h>

#include "frames/JsonFrameData.h"
#include "utils/Logger.h"

using namespace ImageProcessing;
using json = nlohmann::json;

ArToolkitProcessor::ArToolkitProcessor(std::string config)
	: initialized_size_(-1, -1, -1)
{
	auto json_config = json::parse(config);

	calib_path_left_ = json_config["config"]["calibration_left"].get<std::string>();
	calib_path_right_ = json_config["config"]["calibration_right"].get<std::string>();

	for (auto &json_marker : json_config["markers"])
	{
		SetupMarker(json_marker);
	}
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

	//		pose = {

	//			{"m00", markersSquare[i].pose.T[0]},
	//			{"m10", markersSquare[i].pose.T[1]},
	//			{"m20", markersSquare[i].pose.T[2]},
	//			{"m30", markersSquare[i].pose.T[3]},

	//			{"m01", markersSquare[i].pose.T[4]},
	//			{"m11", markersSquare[i].pose.T[5]},
	//			{"m21", markersSquare[i].pose.T[6]},
	//			{"m31", markersSquare[i].pose.T[7]},

	//			{"m02", markersSquare[i].pose.T[8]},
	//			{"m12", markersSquare[i].pose.T[9]},
	//			{"m22", markersSquare[i].pose.T[10]},
	//			{"m32", markersSquare[i].pose.T[11]},

	//			{"m03", markersSquare[i].pose.T[12]},
	//			{"m13", markersSquare[i].pose.T[13]},
	//			{"m23", markersSquare[i].pose.T[14]},
	//			{"m33", markersSquare[i].pose.T[15]}
	//		};

	//		/*
	//			{"m00", markersSquare[i].pose.T[0]},
	//			{"m01", markersSquare[i].pose.T[1]},
	//			{"m02", markersSquare[i].pose.T[2]},
	//			{"m03", markersSquare[i].pose.T[3]},

	//			{"m10", markersSquare[i].pose.T[4]},
	//			{"m11", markersSquare[i].pose.T[5]},
	//			{"m12", markersSquare[i].pose.T[6]},
	//			{"m13", markersSquare[i].pose.T[7]},

	//			{"m20", markersSquare[i].pose.T[8]},
	//			{"m21", markersSquare[i].pose.T[9]},
	//			{"m22", markersSquare[i].pose.T[10]},
	//			{"m23", markersSquare[i].pose.T[11]},

	//			{"m30", markersSquare[i].pose.T[12]},
	//			{"m31", markersSquare[i].pose.T[13]},
	//			{"m32", markersSquare[i].pose.T[14]},
	//			{"m33", markersSquare[i].pose.T[15]}
	//
	//		*/
	//	}
	//}


	bool marker_detected = false;
	json payload;

	if (marker_num_l > 0)
	{
		ARdouble transform_matrix[3][4];
		arGetTransMatSquare(ar_3d_handle_l_, &(marker_info_l[0]), 4.4, transform_matrix);

		json pose {
			{ "transform_matrix",
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
		};

		payload["pose_left"] = pose;
		marker_detected = true;
	}


	if (marker_num_r > 0)
	{
		ARdouble transform_matrix[3][4];
		arGetTransMatSquare(ar_3d_handle_r_, &(marker_info_r[0]), 4.4, transform_matrix);

		json pose {
			{ "transform_matrix",
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
		};

		payload["pose_right"] = pose;
		marker_detected = true;
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

	if (ar_pattern_handle_ != nullptr)
	{
		// already initialized this part
		return;
	}

	ar_pattern_handle_ = arPattCreateHandle();
	if (!ar_pattern_handle_)
	{
		DebugLog("Error creating pattern handle.");
		throw std::exception("Error - See log.");
	}

	ar_handle_l_ = arCreateHandle(c_param_lt_l_);
	ar_handle_r_ = arCreateHandle(c_param_lt_r_);
	if (!ar_handle_l_ || !ar_handle_r_)
	{
		DebugLog("Error creating AR handle.");
		throw std::exception("Error - See log.");
	}


	arPattAttach(ar_handle_l_, ar_pattern_handle_);
	arPattAttach(ar_handle_r_, ar_pattern_handle_);

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

	// Markers setup.

	//newMarkers("C:/code/resources/markers.dat", gARPattHandle, &markersSquare, &markersSquareCount, &gARPattDetectionMode);
	for (auto &marker : markers_)
	{
		if (marker.initialized)
		{
			arPattFree(ar_pattern_handle_, marker.pattern_id);
		}

		marker.pattern_id = arPattLoad(ar_pattern_handle_, marker.pattern_path.c_str());

		if (marker.pattern_id < 0)
		{
			throw std::exception((std::string("Unable to load marker pattern ") + marker.pattern_path).c_str());
		}

		marker.initialized = true;
	}
	

	//
	// Other ARToolKit setup.
	//

	arSetMarkerExtractionMode(ar_handle_l_, AR_USE_TRACKING_HISTORY_V2);
	arSetMarkerExtractionMode(ar_handle_r_, AR_USE_TRACKING_HISTORY_V2);

	// Set the pattern detection mode (template (pictorial) vs. matrix (barcode) based on
	// the marker types as defined in the marker config. file.
	arSetPatternDetectionMode(ar_handle_l_, AR_TEMPLATE_MATCHING_COLOR);
	arSetPatternDetectionMode(ar_handle_r_, AR_TEMPLATE_MATCHING_COLOR);
	// or: AR_MATRIX_CODE_DETECTION AR_TEMPLATE_MATCHING_COLOR_AND_MATRIX

	// Other application-wide marker options. Once set, these apply to all markers in use in the application.
	// If you are using standard ARToolKit picture (template) markers, leave commented to use the defaults.
	// If you are usign a different marker design (see http://www.artoolworks.com/support/app/marker.php )
	// then uncomment and edit as instructed by the marker design application.
	//arSetLabelingMode(gARHandleL, AR_LABELING_BLACK_REGION); // Default = AR_LABELING_BLACK_REGION
	//arSetLabelingMode(gARHandleR, AR_LABELING_BLACK_REGION); // Default = AR_LABELING_BLACK_REGION
	//arSetBorderSize(gARHandleL, 0.25f); // Default = 0.25f
	//arSetBorderSize(gARHandleR, 0.25f); // Default = 0.25f
	//arSetMatrixCodeType(gARHandleL, AR_MATRIX_CODE_3x3); // Default = AR_MATRIX_CODE_3x3
	//arSetMatrixCodeType(gARHandleR, AR_MATRIX_CODE_3x3); // Default = AR_MATRIX_CODE_3x3
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
	for (auto &marker : markers_)
	{
		if (marker.initialized)
		{
			arPattFree(ar_pattern_handle_, marker.pattern_id);
		}

		if (marker.filter)
		{
			arFilterTransMatFinal(marker.ftmi);
		}
	}

	arPattDetach(ar_handle_l_);
	arPattDetach(ar_handle_r_);
	arPattDeleteHandle(ar_pattern_handle_);
	ar3DDeleteHandle(&ar_3d_handle_l_);
	ar3DDeleteHandle(&ar_3d_handle_r_);
	arDeleteHandle(ar_handle_l_);
	arDeleteHandle(ar_handle_r_);
	arParamLTFree(&c_param_lt_l_);
	arParamLTFree(&c_param_lt_r_);
}


void ArToolkitProcessor::SetupMarker(json &json_marker)
{
	Marker marker;

	marker.pattern_path = json_marker["pattern_path"].get<std::string>();
	marker.size = json_marker["size"].get<double>();
	marker.type = json_marker["type"].get<std::string>();

	if (json_marker.count("filter") > 0)
	{
		marker.filter = true; 
		marker.filter_cutoff_freq = json_marker["filter"].get<double>();
		marker.filter_sample_rate = AR_FILTER_TRANS_MAT_SAMPLE_RATE_DEFAULT;
		marker.ftmi = arFilterTransMatInit(marker.filter_sample_rate, marker.filter_cutoff_freq);
	}
	else
	{
		marker.filter = false;
	}

	markers_.push_back(marker);
}
