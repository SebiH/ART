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
	class ArToolkitProcessor : public Processor
	{
	private:
		struct MarkerFilter {
			int id = -1;
			ARFilterTransMatInfo *trans = nullptr;
			int missed_frames = 0;
		};

		std::vector<MarkerFilter> filters_l_;
		std::vector<MarkerFilter> filters_r_;

	private:
		FrameSize initialized_size_;
		bool is_first_initialization_ = true;

		std::string calib_path_left_;
		std::string calib_path_right_;

		ARParamLT *c_param_lt_l_ = nullptr;
		ARParamLT *c_param_lt_r_ = nullptr;
		ARHandle *ar_handle_l_ = nullptr;
		ARHandle *ar_handle_r_ = nullptr;

		AR3DHandle *ar_3d_handle_l_ = nullptr;
		AR3DHandle *ar_3d_handle_r_ = nullptr;

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
			"min_confidence": 0.5,
			"marker_size": 0.5,
			"use_filters": true,
			"max_missed_frames": 10
		}

		 */
		ArToolkitProcessor(std::string config);
		virtual ~ArToolkitProcessor();
		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData> &frame) override;

		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json &config) override;

	private:
		void Initialize(const int sizeX, const int sizeY, const int depth);
		bool SetupCamera(const std::string filename, const int sizeX, const int sizeY, ARParamLT **cparamLT_p);
		void Cleanup();

		nlohmann::json ProcessMarkerInfo(ARMarkerInfo &info, const MarkerFilter &filter);
		void DrawMarker(const ARMarkerInfo &info, const FrameSize &size, unsigned char *buffer);
	};
}
