#include "cameras/OvrvisionCameraSource.h"

using namespace ImageProcessing;


OvrvisionCameraSource::OvrvisionCameraSource(OVR::Camprop quality, OVR::Camqt process_mode)
	: ovr_camera_(std::make_unique<OVR::OvrvisionPro>()),
	  quality_(quality),
	  process_mode_(process_mode)
{
	ovr_camera_->SetCameraSyncMode(true);
}


OvrvisionCameraSource::~OvrvisionCameraSource()
{
	Close();
}



void OvrvisionCameraSource::PrepareNextFrame()
{
	std::lock_guard<std::mutex> lock(mutex_);
	if (IsOpen())
	{
		ovr_camera_->PreStoreCamData(process_mode_);
	}
	else
	{
		throw std::exception("Camera is closed");
	}
}


void OvrvisionCameraSource::GrabFrame(unsigned char * left_buffer, unsigned char * right_buffer)
{
	std::lock_guard<std::mutex> lock(mutex_);
	if (IsOpen())
	{
		ovr_camera_->GetCamImageBGRA(left_buffer, OVR::Cameye::OV_CAMEYE_LEFT);
		ovr_camera_->GetCamImageBGRA(right_buffer, OVR::Cameye::OV_CAMEYE_RIGHT);

		if (use_auto_contrast_)
		{
			auto width = GetFrameWidth();
			auto height = GetFrameHeight();

			cv::Mat left(cv::Size(width, height), CV_8UC4, left_buffer);
			cv::Mat right(cv::Size(width, height), CV_8UC4, right_buffer);
			BrightnessAndContrastAuto(left, right, auto_contrast_clip_percent_);
		}
	}
}




void OvrvisionCameraSource::Open()
{
	std::lock_guard<std::mutex> lock(mutex_);
	if (!IsOpen())
	{
		auto open_success = ovr_camera_->Open(0, quality_);

		if (!open_success)
		{
			throw std::exception("Could not open OVRvision camera");
		}
	}
}

void OvrvisionCameraSource::Close()
{
	std::lock_guard<std::mutex> lock(mutex_);
	if (IsOpen())
	{
		ovr_camera_->Close();
	}
}

bool OvrvisionCameraSource::IsOpen() const
{
	return ovr_camera_->isOpen();
}



int OvrvisionCameraSource::GetFrameWidth() const
{
	return ovr_camera_->GetCamWidth();
}

int OvrvisionCameraSource::GetFrameHeight() const
{
	return ovr_camera_->GetCamHeight();
}

int OvrvisionCameraSource::GetFrameChannels() const
{
	//return ovr_camera_->GetCamPixelsize();
	return 4;
}

float OvrvisionCameraSource::GetFocalLength() const
{
	return ovr_camera_->GetCamFocalPoint();
}


void OvrvisionCameraSource::SetProperties(const nlohmann::json &json_config)
{
	if (json_config.count("Exposure") != 0)
	{
		auto exposure = json_config["Exposure"].get<int>();
		ovr_camera_->SetCameraExposure(exposure);
	}

	if (json_config.count("ExposurePerSec") != 0)
	{
		auto fps = json_config["ExposurePerSec"].get<float>();
		ovr_camera_->SetCameraExposurePerSec(fps);
	}

	if (json_config.count("Gain") != 0)
	{
		auto gain = json_config["Gain"].get<int>();
		ovr_camera_->SetCameraGain(gain);
	}

	if (json_config.count("BLC") != 0)
	{
		auto blc = json_config["BLC"].get<int>();
		ovr_camera_->SetCameraBLC(blc);
	}

	if (json_config.count("AutoWhiteBalance") != 0)
	{
		auto whitebalance = json_config["AutoWhiteBalance"].get<bool>();
		ovr_camera_->SetCameraWhiteBalanceAuto(whitebalance);
	}

	if (json_config.count("WhiteBalanceR") != 0)
	{
		auto whitebalance = json_config["WhiteBalanceR"].get<int>();
		ovr_camera_->SetCameraWhiteBalanceR(whitebalance);
	}

	if (json_config.count("WhiteBalanceG") != 0)
	{
		auto whitebalance = json_config["WhiteBalanceG"].get<int>();
		ovr_camera_->SetCameraWhiteBalanceG(whitebalance);
	}

	if (json_config.count("WhiteBalanceB") != 0)
	{
		auto whitebalance = json_config["WhiteBalanceB"].get<int>();
		ovr_camera_->SetCameraWhiteBalanceB(whitebalance);
	}

	if (json_config.count("AutoContrast") != 0)
	{
		use_auto_contrast_ = json_config["AutoContrast"].get<bool>();
	}

	if (json_config.count("AutoContrastClipHistPercent") != 0)
	{
		auto_contrast_clip_percent_ = json_config["AutoContrastClipHistPercent"].get<float>();
	}

	if (json_config.count("AutoContrastAutoGain") != 0)
	{
		auto_contrast_auto_gain_ = json_config["AutoContrastAutoGain"].get<bool>();
	}

	if (json_config.count("AutoContrastMax") != 0)
	{
		auto_contrast_max_ = json_config["AutoContrastMax"].get<float>();
	}

	if (json_config.count("ExposurePerSec") != 0)
	{
		auto exposure_per_sec = json_config["ExposurePerSec"].get<float>();
		ovr_camera_->SetCameraExposurePerSec(exposure_per_sec);
	}
}

nlohmann::json OvrvisionCameraSource::GetProperties() const
{
	if (IsOpen())
	{
		return nlohmann::json{
			{ "HMDRightGap", { ovr_camera_->GetHMDRightGap(0), ovr_camera_->GetHMDRightGap(1), ovr_camera_->GetHMDRightGap(2) } },
			{ "FocalPoint", ovr_camera_->GetCamFocalPoint() },
			{ "Exposure", ovr_camera_->GetCameraExposure() },
			{ "Gain", ovr_camera_->GetCameraGain() },
			{ "BLC", ovr_camera_->GetCameraBLC() },
			{ "AutoWhiteBalance", ovr_camera_->GetCameraWhiteBalanceAuto() },
			{ "WhiteBalanceR", ovr_camera_->GetCameraWhiteBalanceR() },
			{ "WhiteBalanceG", ovr_camera_->GetCameraWhiteBalanceG() },
			{ "WhiteBalanceB", ovr_camera_->GetCameraWhiteBalanceB() },
			{ "AutoContrast", use_auto_contrast_ },
			{ "AutoContrastAutoGain", auto_contrast_auto_gain_ },
			{ "AutoContrastClipHistPercent", auto_contrast_clip_percent_ },
			{ "AutoContrastMax", auto_contrast_max_ },
		};
	}
	else
	{
		return nlohmann::json();
	}
}


// Adapted from: http://answers.opencv.org/question/75510/how-to-make-auto-adjustmentsbrightness-and-contrast-for-image-android-opencv-image-correction/
static void CalculateHistogram(float &alpha, float &beta, cv::Mat src, float clipHistPercent)
{
	int histSize = 256;
	double minGray = 0, maxGray = 0;

	//to calculate grayscale histogram
	cv::Mat gray;
	cv::cvtColor(src, gray, CV_BGRA2GRAY);
	if (clipHistPercent == 0)
	{
		// keep full available range
		cv::minMaxLoc(gray, &minGray, &maxGray);
	}
	else
	{
		cv::Mat hist; //the grayscale histogram

		float range[] = { 0, 256 };
		const float* histRange = { range };
		bool uniform = true;
		bool accumulate = false;
		calcHist(&gray, 1, 0, cv::Mat(), hist, 1, &histSize, &histRange, uniform, accumulate);

		// calculate cumulative distribution from the histogram
		std::vector<float> accumulator(histSize);
		accumulator[0] = hist.at<float>(0);
		for (int i = 1; i < histSize; i++)
		{
			accumulator[i] = accumulator[i - 1] + hist.at<float>(i);
		}

		// locate points that cuts at required value
		float max = accumulator.back();
		clipHistPercent *= (max / 100.0); //make percent as absolute
		clipHistPercent /= 2.0; // left and right wings
								// locate left cut
		minGray = 0;
		while (accumulator[minGray] < clipHistPercent)
			minGray++;

		// locate right cut
		maxGray = histSize - 1;
		while (accumulator[maxGray] >= (max - clipHistPercent))
			maxGray--;
	}

	// current range
	float inputRange = maxGray - minGray;

	alpha = (histSize - 1) / inputRange;   // alpha expands current range to histsize range
	beta = -minGray * alpha;             // beta shifts current range so that minGray will go to 0
}

/**
 * Adapted from: http://answers.opencv.org/question/75510/how-to-make-auto-adjustmentsbrightness-and-contrast-for-image-android-opencv-image-correction/
 *  \brief Automatic brightness and contrast optimization with optional histogram clipping
 *  \param [in]src Input image GRAY or BGR or BGRA
 *  \param [out]dst Destination image
 *  \param clipHistPercent cut wings of histogram at given percent tipical=>1, 0=>Disabled
 *  \note In case of BGRA image, we won't touch the transparency
 */
void OvrvisionCameraSource::BrightnessAndContrastAuto(cv::Mat &left, cv::Mat &right, float clip_hist_percent)
{
	CV_Assert(clip_hist_percent >= 0);

	float alpha_l, beta_l;
	CalculateHistogram(alpha_l, beta_l, left, clip_hist_percent);
	float alpha_r, beta_r;
	CalculateHistogram(alpha_r, beta_r, right, clip_hist_percent);

	const double AVG_WEIGHT = 0.95;
	float alpha = std::min(std::min(alpha_l, alpha_r), auto_contrast_max_);
	alpha = (avg_alpha * AVG_WEIGHT) + (alpha * (1 - AVG_WEIGHT));
	float beta = std::min(std::min(beta_l, beta_r), auto_contrast_max_);
	beta = (avg_beta * AVG_WEIGHT) + (beta * (1 - AVG_WEIGHT));

	if (auto_contrast_auto_gain_)
	{
		auto current_gain = ovr_camera_->GetCameraGain();
		if (alpha > 2.5 && current_gain < 47)
		{
			ovr_camera_->SetCameraGain(current_gain + 1);
		}
		else if (alpha < 1.5 && current_gain > 1)
		{
			ovr_camera_->SetCameraGain(current_gain - 1);
		}
		else
		{
			avg_alpha = alpha;
			avg_beta = beta;
		}
	}
	else
	{
		avg_alpha = alpha;
		avg_beta = beta;
	}

	// Apply brightness and contrast normalization
	// convertTo operates with saurate_cast
	left.convertTo(left, -1, avg_alpha, avg_beta);
	right.convertTo(right, -1, avg_alpha, avg_beta);
}
