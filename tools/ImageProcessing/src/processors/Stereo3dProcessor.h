#pragma once

#include <opencv2/cudastereo.hpp>
#include "processors/Processor.h"

namespace ImageProcessing
{
	class Stereo3dProcessor : public Processor
	{
	private:
		cv::Ptr<cv::cuda::StereoBM> bm;
		cv::Ptr<cv::cuda::StereoBeliefPropagation> bp;
		cv::Ptr<cv::cuda::StereoConstantSpaceBP> csbp;

		int method = 0;
		int blockSize = 15;
		int nDisparity = 64;

		void HandleKey();


	public:
		Stereo3dProcessor();
		virtual ~Stereo3dProcessor();

		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData>& frame) override;
		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json & config) override;
	};
}
