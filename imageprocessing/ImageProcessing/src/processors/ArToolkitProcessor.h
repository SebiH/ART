#pragma once

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
		struct Marker
		{
			bool initialized = false;

			double size;
			std::string pattern_path;
			std::string type;
			bool filter;

			int pattern_id;
			std::string name;
			double filter_cutoff_freq;
			double filter_sample_rate;

			ARFilterTransMatInfo *ftmi;
		};
		std::vector<Marker> markers_;

	private:
		FrameSize initialized_size_;
		bool is_first_initialization_ = true;

		std::string calib_path_left_;
		std::string calib_path_right_;

		ARPattHandle *ar_pattern_handle_ = nullptr;
		ARParamLT *c_param_lt_l_ = nullptr;
		ARParamLT *c_param_lt_r_ = nullptr;
		ARHandle *ar_handle_l_ = nullptr;
		ARHandle *ar_handle_r_ = nullptr;

		AR3DHandle *ar_3d_handle_l_ = nullptr;
		AR3DHandle *ar_3d_handle_r_ = nullptr;

		// get/settable properties
		double min_confidence_ = 0.5;

	public:
		/*
		 * Configuration example:

		{
			"config": {
				"calibration_left": "absolute/path/to/calib/file",
				"calibration_right": "absolute/path/to/calib/file"
			},
			"markers": [
				{
					"size": 0.026, // in m! 2.6cm => 0.026
					"name": "myMarker",
					"pattern_path": "absolute/path/to/file.patt",
					"type": "SINGLE", // SINGLE, MULTI (unsupported), NFT (unsupported)
					"filter": 5.0 // cuttoff frequency for pose estimation (?) (optional)
				},
				{ .. other markers .. }
			]
		}

		 */
		ArToolkitProcessor(std::string config);
		virtual ~ArToolkitProcessor();
		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData> &frame) override;

		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json config) override;

	private:
		void Initialize(const int sizeX, const int sizeY, const int depth);
		bool SetupCamera(const std::string filename, const int sizeX, const int sizeY, ARParamLT **cparamLT_p);
		void SetupMarker(nlohmann::json &json_marker);
		void Cleanup();

		nlohmann::json ProcessMarkerInfo(ARMarkerInfo &info);
		void DrawMarker(const ARMarkerInfo &info, const FrameSize &size, unsigned char *buffer);
		const Marker GetMarker(const ARMarkerInfo &info) const;
	};
}
