#include "UndistortProcessor.h"

using namespace ImageProcessing;
using json = nlohmann::json;

UndistortProcessor::UndistortProcessor(const json & config)
{
	cv::FileStorage fs_intrinsic_l(config["intrinsic_left"], cv::FileStorage::READ);
	cv::FileStorage fs_distcoeffs_l(config["distcoeffs_left"], cv::FileStorage::READ);
	fs_intrinsic_l["standard_left_intrinsic"] >> intrinsic_left_;
	fs_distcoeffs_l["standard_left_distcoeffs"] >> distcoeffs_left_;

	cv::FileStorage fs_intrinsic_r(config["intrinsic_right"], cv::FileStorage::READ);
	cv::FileStorage fs_distcoeffs_r(config["distcoeffs_right"], cv::FileStorage::READ);
	fs_intrinsic_r["standard_right_intrinsic"] >> intrinsic_right_;
	fs_distcoeffs_r["standard_right_distcoeffs"] >> distcoeffs_right_;
}

UndistortProcessor::~UndistortProcessor()
{
}

std::shared_ptr<const FrameData> UndistortProcessor::Process(const std::shared_ptr<const FrameData>& frame)
{
	cv::Size mat_size(frame->size.width, frame->size.height);
	Init(mat_size);

	cv::Mat img_left(mat_size, frame->size.CvType(), frame->buffer_left.get());
	cv::remap(img_left, img_left, map1_l_, map2_l_, cv::INTER_LINEAR);

	cv::Mat img_right(mat_size, frame->size.CvType(), frame->buffer_right.get());
	cv::remap(img_right, img_right, map1_r_, map2_r_, cv::INTER_LINEAR);

	return frame;
}

json UndistortProcessor::GetProperties()
{
	return json();
}

void UndistortProcessor::SetProperties(const json & config)
{
}

void UndistortProcessor::Init(const cv::Size &framesize)
{
	if (framesize != initialized_size)
	{
		cv::initUndistortRectifyMap(intrinsic_left_, distcoeffs_left_, cv::Mat(), cv::Mat(), framesize, CV_32FC1, map1_l_, map2_l_);
		cv::initUndistortRectifyMap(intrinsic_right_, distcoeffs_right_, cv::Mat(), cv::Mat(), framesize, CV_32FC1, map1_r_, map2_r_);
		initialized_size = framesize;
	}
}
