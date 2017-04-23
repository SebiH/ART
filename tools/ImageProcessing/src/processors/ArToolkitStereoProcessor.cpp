#include "processors/ArToolkitStereoProcessor.h"

#include "frames/JsonFrameData.h"
#include "utils/Logger.h"

using namespace ImageProcessing;
using json = nlohmann::json;

ArToolkitStereoProcessor::ArToolkitStereoProcessor(std::string config)
{
	auto json_config = json::parse(config);

	calib_path_left_ = json_config["calibration_left"].get<std::string>();
	calib_path_right_ = json_config["calibration_right"].get<std::string>();
	calib_path_stereo_ = json_config["calibration_stereo"].get<std::string>();

	SetProperties(json_config);
}

ArToolkitStereoProcessor::~ArToolkitStereoProcessor()
{
	Cleanup();
}

std::shared_ptr<const FrameData> ArToolkitStereoProcessor::Process(const std::shared_ptr<const FrameData>& frame)
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

	std::vector<std::tuple<ARMarkerInfo, ARMarkerInfo>> stereo_markers;
	for (int i = 0; i < marker_num_l; i++)
	{
		auto info_l = marker_info_l[i];
		if (info_l.cf > min_confidence_ && info_l.id >= 0)
		{
			for (int j = 0; j < marker_num_r; j++)
			{
				auto info_r = marker_info_r[j];
				if (info_l.id == info_r.id)
				{
					stereo_markers.push_back(std::make_tuple(info_l, info_r));
					break;
				}
			}
		}
	}

	bool marker_detected = stereo_markers.size() > 0;
	json payload{
		{ "markers", json::array() },
	};


	for (const auto &stereo_marker : stereo_markers)
	{
		auto marker_l = std::get<0>(stereo_marker);
		auto marker_r = std::get<1>(stereo_marker);
		auto pose = ProcessMarkerInfo(marker_l, marker_r);
		DrawMarker(marker_l, frame->size, frame->buffer_left.get());
		DrawMarker(marker_r, frame->size, frame->buffer_right.get());
		payload["markers"].push_back(pose);
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

// taken from ARToolkit
static void arglCameraViewRH(const ARdouble para[3][4], ARdouble m_modelview[16], const ARdouble scale)
{
	m_modelview[0 + 0 * 4] = para[0][0]; // R1C1
	m_modelview[0 + 1 * 4] = para[0][1]; // R1C2
	m_modelview[0 + 2 * 4] = para[0][2];
	m_modelview[0 + 3 * 4] = para[0][3];
	m_modelview[1 + 0 * 4] = -para[1][0]; // R2
	m_modelview[1 + 1 * 4] = -para[1][1];
	m_modelview[1 + 2 * 4] = -para[1][2];
	m_modelview[1 + 3 * 4] = -para[1][3];
	m_modelview[2 + 0 * 4] = -para[2][0]; // R3
	m_modelview[2 + 1 * 4] = -para[2][1];
	m_modelview[2 + 2 * 4] = -para[2][2];
	m_modelview[2 + 3 * 4] = -para[2][3];
	m_modelview[3 + 0 * 4] = 0.0;
	m_modelview[3 + 1 * 4] = 0.0;
	m_modelview[3 + 2 * 4] = 0.0;
	m_modelview[3 + 3 * 4] = 1.0;
	if (scale != 0.0) {
		m_modelview[12] *= scale;
		m_modelview[13] *= scale;
		m_modelview[14] *= scale;
	}
}


json ArToolkitStereoProcessor::ProcessMarkerInfo(ARMarkerInfo &marker_l, ARMarkerInfo &marker_r)
{
	ARdouble transform_matrix[3][4];
	auto match_error = arGetStereoMatchingErrorSquare(ar_3d_stereo_handle_, &marker_l, &marker_r);
	double trans_error = arGetTransMatSquareStereo(ar_3d_stereo_handle_, &marker_l, &marker_r, marker_size_, transform_matrix);

	const auto filter = filters_[marker_l.id];
	if (use_filters_)
	{
		arFilterTransMat(filter.ftmi, transform_matrix, filter.missed_frames >= max_missed_frames_ ? 1 : 0);
	}

	ARdouble mat[16];
	// mm (artoolkit) -> m (unity)
	const double scale = 0.001;
	arglCameraViewRH(transform_matrix, mat, scale);

	return json{
		{ "id", marker_l.id },
		{ "confidence", std::min(marker_l.cfMatrix, marker_r.cfMatrix) },
		{ "match_error", match_error },
		{ "trans_error", trans_error },
		{ "transformation_matrix",{
			mat[0], mat[1], mat[2], mat[3],
			mat[4], mat[5], mat[6], mat[7],
			mat[8], mat[9], mat[10], mat[11],
			mat[12], mat[13], mat[14], mat[15] }
		}
	};
}

void ArToolkitStereoProcessor::DrawMarker(const ARMarkerInfo &marker, const FrameSize & size, unsigned char * buffer)
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



void ArToolkitStereoProcessor::Initialize(const int sizeX, const int sizeY, const int depth)
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
	pattern_error -= arSetMatrixCodeType(ar_handle_l_, MARKER_TYPE);
	pattern_error -= arSetPatternDetectionMode(ar_handle_l_, AR_MATRIX_CODE_DETECTION);
	pattern_error -= arSetMatrixCodeType(ar_handle_r_, MARKER_TYPE);
	pattern_error -= arSetPatternDetectionMode(ar_handle_r_, AR_MATRIX_CODE_DETECTION);
	pattern_error -= arSetBorderSize(ar_handle_l_, MARKER_BORDER_SIZE); // Default = 0.25f
	pattern_error -= arSetBorderSize(ar_handle_r_, MARKER_BORDER_SIZE); // Default = 0.25f

	for (int i = 0; i < MAX_MARKER_ID; i++)
	{
		MarkerFilter mf;
		mf.id = i;
		mf.ftmi = arFilterTransMatInit(90, 15);
		filters_.push_back(mf);
	}

	if (pattern_error < 0)
	{
		DebugLog("Error setting matrix type");
		throw std::exception("Error - See log.");
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

	// Stereo handle
	if (arParamLoadExt(calib_path_stereo_.c_str(), trans_l2r_) < 0)
	{
		DebugLog("Error loading stereo calibration");
		throw std::exception("Error - See log.");
	}
	arParamDispExt(trans_l2r_);
	ar_3d_stereo_handle_ = ar3DStereoCreateHandle(&(c_param_lt_l_->param), &(c_param_lt_r_->param), AR_TRANS_MAT_IDENTITY, trans_l2r_);

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

bool ArToolkitStereoProcessor::SetupCamera(const std::string filename, const int sizeX, const int sizeY, ARParamLT ** cparamLT_p)
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

void ArToolkitStereoProcessor::Cleanup()
{
	arPattDetach(ar_handle_l_);
	arPattDetach(ar_handle_l_);
	ar3DStereoDeleteHandle(&ar_3d_stereo_handle_);
	ar3DDeleteHandle(&ar_3d_handle_l_);
	ar3DDeleteHandle(&ar_3d_handle_r_);
	arDeleteHandle(ar_handle_l_);
	arDeleteHandle(ar_handle_r_);
	arParamLTFree(&c_param_lt_l_);
	arParamLTFree(&c_param_lt_r_);
}




json ArToolkitStereoProcessor::GetProperties()
{
	return json{
		{ "min_confidence", min_confidence_ },
		{ "marker_size", marker_size_ }
	};
}

void ArToolkitStereoProcessor::SetProperties(const json & config)
{
	if (config.count("min_confidence"))
	{
		min_confidence_ = config["min_confidence"].get<double>();
	}

	if (config.count("marker_size"))
	{
		marker_size_ = config["marker_size"].get<double>();
	}
}
