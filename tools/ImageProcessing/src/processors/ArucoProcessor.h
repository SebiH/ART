#pragma once

#include <map>
#include <aruco/aruco.h>
#include "processors/Processor.h"
#include "utils/Quaternion.h"

namespace ImageProcessing
{
	class ArucoProcessor : public Processor
	{
	private:
		aruco::MarkerDetector detector_;
		aruco::CameraParameters camera_params_;

		bool use_tracker_ = false;
		// each marker has its own tracker
		std::map<int,aruco::MarkerPoseTracker> pose_trackers_;
		float pt_min_error_ratio_ = 4.0f;

		FrameSize initialized_size_;
		float marker_size_m_;

		// TODO: deprecated, use GetParams on detector!
		float threshold_ = 130.0f;


	public:
		ArucoProcessor(const nlohmann::json &marker_config);
		~ArucoProcessor();

		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData>& frame) override;

		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json &config) override;

	private:
		void Init(const FrameSize &size, const float focalpoint);
	};
}
