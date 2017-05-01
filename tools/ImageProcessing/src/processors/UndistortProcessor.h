#pragma once

#include "processors/Processor.h"
#include "frames/FrameSize.h"
#include <opencv2/opencv.hpp>
#include <CL/cl.h>

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

		// OpenCL

		unsigned int width_;
		unsigned int height_;

		cl_command_queue cmd_queue_;

		cl_mem	src_;
		cl_mem	_l, _r;			// demosaic and remapped image
		cl_mem	_L, _R;			// work image
		cl_mem	_mx[2], _my[2]; // map for remap in GPU

		cv::Mat		*_mapX[2], *_mapY[2];	// camera parameter

		cl_kernel clk_resize_;
		cl_kernel clk_remap_;
		cl_kernel clk_demosaic_;

	public:
		UndistortProcessor(const nlohmann::json &config);
		~UndistortProcessor();

		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData>& frame) override;

		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json &config) override;

	private:
		void Init(const cv::Size &framesize);
		void DemosaicRemap(const ushort* src, cl_mem left, cl_mem right, cl_event *event_l, cl_event *event_r);
		void DemosaicRemap(const ushort* src, cl_event *event_l, cl_event *event_r);
	};
}
