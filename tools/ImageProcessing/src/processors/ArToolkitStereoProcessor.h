#pragma once

#include <string>
#include <vector>
#include <AR/ar.h>
#include <AR/arFilterTransMat.h>
#include <json/json.hpp>
#include "processors/Processor.h"
#include "frames/FrameSize.h"

namespace ImageProcessing
{
	class ArToolkitStereoProcessor : public Processor
	{
	private:
		struct MarkerFilter {
			int id = -1;
			ARFilterTransMatInfo *ftmi = nullptr;
			int missed_frames = 0;
		};
		std::vector<MarkerFilter> filters_;

	private:
		// hardcoded config
		const int MAX_MARKER_ID = 512;
		const AR_MATRIX_CODE_TYPE MARKER_TYPE = AR_MATRIX_CODE_4x4_BCH_13_9_3;
		const double MARKER_BORDER_SIZE = 0.1;

	private:
		FrameSize initialized_size_;
		bool is_first_initialization_ = true;

		std::string calib_path_left_;
		std::string calib_path_right_;
		std::string calib_path_stereo_;

		ARParamLT *c_param_lt_l_ = nullptr;
		ARParamLT *c_param_lt_r_ = nullptr;
		ARHandle *ar_handle_l_ = nullptr;
		ARHandle *ar_handle_r_ = nullptr;

		AR3DHandle *ar_3d_handle_l_ = nullptr;
		AR3DHandle *ar_3d_handle_r_ = nullptr;

		AR3DStereoHandle *ar_3d_stereo_handle_ = nullptr;
		ARdouble trans_l2r_[3][4];


		// get/settable properties
		double min_confidence_ = 0.5;
		double marker_size_ = 0.5;
		bool use_filters_ = true;
		int max_missed_frames_ = 180;


	public:
		/*
		* Configuration example:

		{
			"calibration_left": "absolute/path/to/calib/file", // only in constructor
			"calibration_right": "absolute/path/to/calib/file", // only in constructor
			"calibration_stereo": "absolute/path/to/calib/file", // only in constructor
			"min_confidence": 0.5,
			"marker_size": 0.5
		}

		*/
		ArToolkitStereoProcessor(std::string config);
		virtual ~ArToolkitStereoProcessor();
		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData> &frame) override;

		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json &config) override;

	private:
		void Initialize(const int sizeX, const int sizeY, const int depth);
		bool SetupCamera(const std::string filename, const int sizeX, const int sizeY, ARParamLT **cparamLT_p);
		void Cleanup();

		nlohmann::json ProcessMarkerInfo(ARMarkerInfo &info_l, ARMarkerInfo &info_r);
		void DrawMarker(const ARMarkerInfo &info, const FrameSize &size, unsigned char *buffer);
	};
}
