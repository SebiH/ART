#pragma once

#include <map>
#include <aruco/aruco.h>
#include "processors/Processor.h"
#include "utils/Quaternion.h"

namespace ImageProcessing
{
	class ArucoMapProcessor : public Processor
	{
	private:

		aruco::MarkerDetector detector_;
		aruco::CameraParameters camera_params_;

		std::map<int, aruco::MarkerMapPoseTracker> pose_trackers_;
		float pt_min_error_ratio_ = 4.0f;

		FrameSize initialized_size_;
		float marker_size_m_;

		// TODO: deprecated, use GetParams on detector!
		float threshold_ = 130.0f;


	public:
		ArucoMapProcessor(const nlohmann::json &marker_config);
		~ArucoMapProcessor();

		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData>& frame) override;

		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json &config) override;

	private:
		void Init(const FrameSize &size, const float focalpoint);
	};
}
