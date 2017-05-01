#include "UndistortProcessor.h"

#include <CL/cl.h>
#include <CL/cl_gl.h>
#include <CL/cl_gl_ext.h>
#include <CL/cl_ext.h>
#include <ovrvision/ovrvision_pro.h>

using namespace ImageProcessing;
using json = nlohmann::json;

UndistortProcessor::UndistortProcessor(const json & config)
{
	//cv::FileStorage fs_intrinsic_l(config["intrinsic_left"], cv::FileStorage::READ);
	//cv::FileStorage fs_distcoeffs_l(config["distcoeffs_left"], cv::FileStorage::READ);
	//fs_intrinsic_l["standard_left_intrinsic"] >> intrinsic_left_;
	//fs_distcoeffs_l["standard_left_distcoeffs"] >> distcoeffs_left_;

	//cv::FileStorage fs_intrinsic_r(config["intrinsic_right"], cv::FileStorage::READ);
	//cv::FileStorage fs_distcoeffs_r(config["distcoeffs_right"], cv::FileStorage::READ);
	//fs_intrinsic_r["standard_right_intrinsic"] >> intrinsic_right_;
	//fs_distcoeffs_r["standard_right_distcoeffs"] >> distcoeffs_right_;

	size_t origin[3] = { 0, 0, 0 };
	size_t region[3] = { _width, _height, 1 };

	if (_settings.ReadEEPROM())
	{
		// Left camera
		_settings.GetUndistortionMatrix(OVR::Cameye::OV_CAMEYE_LEFT, *_mapX[0], *_mapY[0], _width, _height);
		auto _errorCode = clEnqueueWriteImage(cmd_queue_, _mx[0], CL_TRUE, origin, region, _width * sizeof(float), 0, _mapX[0]->ptr(0), 0, NULL, NULL);
		_errorCode = clEnqueueWriteImage(cmd_queue_, _my[0], CL_TRUE, origin, region, _width * sizeof(float), 0, _mapY[0]->ptr(0), 0, NULL, NULL);
		//SAMPLE_CHECK_ERRORS(_errorCode);

		// Right camera
		_settings.GetUndistortionMatrix(OVR::Cameye::OV_CAMEYE_RIGHT, *_mapX[1], *_mapY[1], _width, _height);
		_errorCode = clEnqueueWriteImage(cmd_queue_, _mx[1], CL_TRUE, origin, region, _width * sizeof(float), 0, _mapX[1]->ptr(0), 0, NULL, NULL);
		_errorCode = clEnqueueWriteImage(cmd_queue_, _my[1], CL_TRUE, origin, region, _width * sizeof(float), 0, _mapY[1]->ptr(0), 0, NULL, NULL);
		//SAMPLE_CHECK_ERRORS(_errorCode);

		_remapAvailable = true;
		return true;
	}
}

UndistortProcessor::~UndistortProcessor()
{
}


void UndistortProcessor::DemosaicRemap(const ushort* src, cl_mem left, cl_mem right, cl_event *event_l, cl_event *event_r)
{
	size_t origin[3] = { 0, 0, 0 };
	size_t region[3] = { width_, height_, 1 };
	size_t demosaicSize[] = { width_ / 2, height_ / 2 };
	cl_event writeEvent, execute;

	auto _errorCode = clEnqueueWriteImage(cmd_queue_, src_, CL_TRUE, origin, region, width_ * sizeof(ushort), 0, src, 0, NULL, &writeEvent);
	//SAMPLE_CHECK_ERRORS(_errorCode);

	//__kernel void demosaic(
	//	__read_only image2d_t src,	// CL_UNSIGNED_INT16
	//	__write_only image2d_t left,	// CL_UNSIGNED_INT8 x 4
	//	__write_only image2d_t right)	// CL_UNSIGNED_INT8 x 4
	//cl_kernel _demosaic = clCreateKernel(_program, "demosaic", &_errorCode);
	//SAMPLE_CHECK_ERRORS(_errorCode);

	clSetKernelArg(clk_demosaic_, 0, sizeof(cl_mem), &src_);
	clSetKernelArg(clk_demosaic_, 1, sizeof(cl_mem), &_L);
	clSetKernelArg(clk_demosaic_, 2, sizeof(cl_mem), &_R);
	_errorCode = clEnqueueNDRangeKernel(cmd_queue_, clk_demosaic_, 2, NULL, demosaicSize, 0, 1, &writeEvent, &execute);
	//SAMPLE_CHECK_ERRORS(_errorCode);

	if (_remapAvailable)
	{
		size_t remapSize[] = { width_, height_ };

		//__kernel void remap(
		//	__read_only image2d_t src,		// CL_UNSIGNED_INT8 x 4
		//	__read_only image2d_t mapX,		// CL_FLOAT
		//	__read_only image2d_t mapY,		// CL_FLOAT
		//	__write_only image2d_t	dst)	// CL_UNSIGNED_INT8 x 4
		//cl_kernel _remap = clCreateKernel(_program, "remap", &_errorCode);
		//SAMPLE_CHECK_ERRORS(_errorCode);

		clSetKernelArg(clk_remap_, 0, sizeof(cl_mem), &_L);
		clSetKernelArg(clk_remap_, 1, sizeof(cl_mem), &_mx[0]);
		clSetKernelArg(clk_remap_, 2, sizeof(cl_mem), &_my[0]);
		clSetKernelArg(clk_remap_, 3, sizeof(cl_mem), &left);
		_errorCode = clEnqueueNDRangeKernel(cmd_queue_, clk_remap_, 2, NULL, remapSize, 0, 1, &execute, event_l);
		//SAMPLE_CHECK_ERRORS(_errorCode);
		clSetKernelArg(clk_remap_, 0, sizeof(cl_mem), &_R);
		clSetKernelArg(clk_remap_, 1, sizeof(cl_mem), &_mx[1]);
		clSetKernelArg(clk_remap_, 2, sizeof(cl_mem), &_my[1]);
		clSetKernelArg(clk_remap_, 3, sizeof(cl_mem), &right);
		_errorCode = clEnqueueNDRangeKernel(cmd_queue_, clk_remap_, 2, NULL, remapSize, 0, 1, &execute, event_r);
		//SAMPLE_CHECK_ERRORS(_errorCode);
	}
	// Release temporaries
	clReleaseEvent(writeEvent);
	clReleaseEvent(execute);
}




void UndistortProcessor::DemosaicRemap(const ushort* src, cl_event *event_l, cl_event *event_r)
{
	cl_event wait_l, wait_r;
	DemosaicRemap(src, _l, _r, &wait_l, &wait_r);

	// Resize
	int scale;
	size_t origin[3] = { 0, 0, 0 };

	//__kernel void resize( 
	//		__read_only image2d_t src,	// CL_UNSIGNED_INT8 x 4
	//		__write_only image2d_t dst,	// CL_UNSIGNED_INT8 x 4
	//		__read_only int scale)		// 2, 4, 8

	//clSetKernelArg(_resize, 0, sizeof(cl_mem), &_l);
	//clSetKernelArg(_resize, 1, sizeof(cl_mem), &_reducedL);
	//clSetKernelArg(_resize, 2, sizeof(int), &scale);
	//auto _errorCode = clEnqueueNDRangeKernel(cmd_queue_, _resize, 2, NULL, _scaledRegion, 0, 1, &wait_l, event_l);
	//SAMPLE_CHECK_ERRORS(_errorCode);
	//clSetKernelArg(_resize, 0, sizeof(cl_mem), &_r);
	//clSetKernelArg(_resize, 1, sizeof(cl_mem), &_reducedR);
	//clSetKernelArg(_resize, 2, sizeof(int), &scale);
	//_errorCode = clEnqueueNDRangeKernel(cmd_queue_, _resize, 2, NULL, _scaledRegion, 0, 1, &wait_r, event_r);
	//SAMPLE_CHECK_ERRORS(_errorCode);

	if (event_l == NULL || event_r == NULL)
	{
		clFinish(cmd_queue_);
	}
	// Release temporaries
	clReleaseEvent(wait_l);
	clReleaseEvent(wait_r);
}

std::shared_ptr<const FrameData> UndistortProcessor::Process(const std::shared_ptr<const FrameData>& frame)
{
	cv::Size mat_size(frame->size.width, frame->size.height);

	size_t origin[3] = { 0, 0, 0 };
	size_t region[3] = { frame->size.width, frame->size.height, 1 };
	cl_event execute_l, execute_r, event[2];


	DemosaicRemap(src, &execute_l, &execute_r);

	// Read result
	auto _errorCode = clEnqueueReadImage(cmd_queue_, _l, CL_TRUE, origin, region, width_ * sizeof(uchar) * 4, 0, frame->buffer_left.get(), 1, &execute_l, &event[0]);
	_errorCode = clEnqueueReadImage(cmd_queue_, _r, CL_TRUE, origin, region, width_ * sizeof(uchar) * 4, 0, frame->buffer_right.get(), 1, &execute_r, &event[1]);
	//SAMPLE_CHECK_ERRORS(_errorCode);
	clWaitForEvents(2, event);

	// Release temporaries
	clReleaseEvent(execute_l);
	clReleaseEvent(execute_r);
	clReleaseEvent(event[0]);
	clReleaseEvent(event[1]);

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
