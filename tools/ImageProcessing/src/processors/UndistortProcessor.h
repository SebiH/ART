#pragma once

#include "processors/Processor.h"
#include "frames/FrameSize.h"
#include <opencv2/opencv.hpp>

namespace ImageProcessing
{
	class UndistortProcessor : public Processor
	{
	private:
		cv::Mat intrinsic_left_;
		cv::Mat distcoeffs_left_;
		cv::Mat map1_l_, map2_l_; // radial undistorion maps

		cv::Mat intrinsic_right_;
		cv::Mat distcoeffs_right_;
		cv::Mat map1_r_, map2_r_; // radial undistorion maps

		cv::Size initialized_size;

	public:
		UndistortProcessor(const nlohmann::json &config);
		~UndistortProcessor();

		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData>& frame) override;

		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json &config) override;

	private:
		void Init(const cv::Size &framesize);
	};
}
