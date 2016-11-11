#pragma once

#include <aruco/aruco.h>
#include "processors/Processor.h"

namespace ImageProcessing
{
	class ArucoProcessor : public Processor
	{
	private:
		// Adapted from OVRVision
		struct Quaternion {
			union {
				float v[4];
				struct {
					float x;
					float y;
					float z;
					float w;
				};
			};
		};

		aruco::MarkerDetector detector_;
		aruco::CameraParameters camera_params_;

		bool use_tracker_ = false;
		aruco::MarkerPoseTracker pose_tracker_;
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

		// Adapted from OVRVision
		void RotMatToQuaternion(Quaternion *outQuat, const float *inMat);
		Quaternion MultiplyQuaternion(Quaternion *a, Quaternion *b);

	};
}
