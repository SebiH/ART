#include "Stereo3dProcessor.h"

#include <opencv2/imgproc.hpp>
#include <opencv2/highgui.hpp>

using namespace ImageProcessing;
using json = nlohmann::json;

Stereo3dProcessor::Stereo3dProcessor()
{
	int ndisp = 16;

	// Set common parameters
	bm = cv::cuda::createStereoBM(ndisp);
	bp = cv::cuda::createStereoBeliefPropagation(ndisp);
	csbp = cv::cuda::createStereoConstantSpaceBP(ndisp);
}

Stereo3dProcessor::~Stereo3dProcessor()
{

}

std::shared_ptr<const FrameData> Stereo3dProcessor::Process(const std::shared_ptr<const FrameData> &frame)
{
	cv::Size framesize(frame->size.width, frame->size.height);
	auto left_src = cv::Mat(framesize, CV_MAKETYPE(CV_8U, frame->size.depth), frame->buffer_left.get());
	auto right_src = cv::Mat(framesize, CV_MAKETYPE(CV_8U, frame->size.depth), frame->buffer_right.get());

	cv::Mat left, right;
	cv::cuda::GpuMat d_left, d_right;

	cv::cvtColor(left_src, left, cv::COLOR_BGR2GRAY);
	cv::cvtColor(right_src, right, cv::COLOR_BGR2GRAY);
	d_left.upload(left);
	d_right.upload(right);

	// Prepare disparity map of specified type
	cv::Mat disp(left.size(), CV_8U);
	cv::cuda::GpuMat d_disp(left.size(), CV_8U);

	//bool running = true;
	//while (running)
	//{
		//workBegin();
		switch (method)
		{
		case 0:
			//if (d_left.channels() > 1 || d_right.channels() > 1)
			//{
			//	cout << "BM doesn't support color images\n";
			//	cvtColor(left_src, left, COLOR_BGR2GRAY);
			//	cvtColor(right_src, right, COLOR_BGR2GRAY);
			//	cout << "image_channels: " << left.channels() << endl;
			//	d_left.upload(left);
			//	d_right.upload(right);
			//	imshow("left", left);
			//	imshow("right", right);
			//}
			bm->compute(d_left, d_right, d_disp);
			break;
		case 1:
			bp->compute(d_left, d_right, d_disp);
			break;
		case 2:
			csbp->compute(d_left, d_right, d_disp);
			break;
		}
		//workEnd();

		// Show results
		d_disp.download(disp);
		//putText(disp, text(), Point(5, 25), FONT_HERSHEY_SIMPLEX, 1.0, Scalar::all(255));
		cv::imshow("disparity", disp);

		//handleKey((char)waitKey(3));
		HandleKey();
		//cv::waitKey(3);
	//}

	//return std::shared_ptr<const FrameData>();
	return frame;
}
#include <opencv2/core/utility.hpp>

void Stereo3dProcessor::HandleKey()
{
	switch (cv::waitKey(3))
	{
	case 'm': case 'M':
		method = (method + 1) % 3;
		break;
	case 's': case 'S':
		switch (bm->getPreFilterType())
		{
		case 0:
			bm->setPreFilterType(cv::StereoBM::PREFILTER_XSOBEL);
			break;
		case cv::StereoBM::PREFILTER_XSOBEL:
			bm->setPreFilterType(0);
			break;
		}
		break;
	case '1':
		nDisparity = nDisparity == 1 ? 8 : nDisparity + 8;
		bm->setNumDisparities(nDisparity);
		bp->setNumDisparities(nDisparity);
		csbp->setNumDisparities(nDisparity);
		break;
	case 'q': case 'Q':
		nDisparity = nDisparity - 8;
		if (nDisparity < 8) { nDisparity = 8; }
		bm->setNumDisparities(nDisparity);
		bp->setNumDisparities(nDisparity);
		csbp->setNumDisparities(nDisparity);
		break;
	case '2':
		bm->setBlockSize(cv::min(bm->getBlockSize() + 2, 51));
		break;
	case 'w': case 'W':
		bm->setBlockSize(cv::max(bm->getBlockSize() - 2, 3));
		break;
	case '3':
		bp->setNumIters(bp->getNumIters() + 1);
		csbp->setNumIters(csbp->getNumIters() + 1);
		break;
	case 'e': case 'E':
		bp->setNumIters(cv::max(bp->getNumIters() - 1, 1));
		csbp->setNumIters(cv::max(csbp->getNumIters() - 1, 1));
		break;
	case '4':
		bp->setNumLevels(bp->getNumLevels() + 1);
		csbp->setNumLevels(csbp->getNumLevels() + 1);
		break;
	case 'r': case 'R':
		bp->setNumLevels(cv::max(bp->getNumLevels() - 1, 1));
		csbp->setNumLevels(cv::max(csbp->getNumLevels() - 1, 1));
		break;
	}
}

json Stereo3dProcessor::GetProperties()
{
	return json();
}

void Stereo3dProcessor::SetProperties(const json &config)
{
}
