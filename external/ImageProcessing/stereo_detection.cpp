// waiting for CUDA vs2015 toolchain support...





///*
//*  Adapted from "stereo_match.cpp" opencv sample
//*/
//
//#include <iostream>
//#include <string>
//#include <sstream>
//#include <iomanip>
//#include <stdexcept>
//#include <memory>
//#include <opencv2/core/utility.hpp>
//#include <opencv2/cudastereo.hpp>
//#include <opencv2/highgui.hpp>
//#include <opencv2/imgproc.hpp>
//
//using namespace cv;
//using namespace std;
//#include <ovrvision_pro.h>
//
//using namespace cv;
//
//int64 work_begin;
//double work_fps;
//
//void workBegin() { work_begin = getTickCount(); }
//void workEnd()
//{
//	int64 d = getTickCount() - work_begin;
//	double f = getTickFrequency();
//	work_fps = f / d;
//}
//
//extern "C" __declspec(dllexport) void StereoDetection()
//{
//	const char* intrinsic_filename = "intrinsics.yml";
//	const char* extrinsic_filename = "extrinsics.yml";
//
//
//
//	OVR::Camprop camProp = OVR::OV_CAMVR_FULL;
//	auto ovrCamera = std::unique_ptr<OVR::OvrvisionPro>(new OVR::OvrvisionPro());
//
//	// TODO: error on failure?
//	auto openSuccess = ovrCamera->Open(0, camProp);
//
//	if (!openSuccess)
//	{
//		std::cout << "Could not open OVR cameras" << std::endl;
//		return;
//	}
//
//	// default settings
//	ovrCamera->SetCameraExposure(12960);
//	ovrCamera->SetCameraGain(47);
//	ovrCamera->SetCameraSyncMode(false);
//
//	auto camWidth = ovrCamera->GetCamWidth();
//	auto camHeight = ovrCamera->GetCamHeight();
//
//
//
//
//
//
//	while (true)
//	{
//		ovrCamera->PreStoreCamData(OVR::OV_CAMQT_DMS);
//
//		unsigned char *leftImgData = ovrCamera->GetCamImageBGRA(OVR::OV_CAMEYE_LEFT);
//		unsigned char *rightImgData = ovrCamera->GetCamImageBGRA(OVR::OV_CAMEYE_RIGHT);
//
//		Mat left_src(Size(camWidth, camHeight), CV_8UC4, leftImgData);
//		Mat right_src(Size(camWidth, camHeight), CV_8UC4, rightImgData);
//
//
//
//
//		Mat left, right;
//		cuda::GpuMat d_left, d_right;
//		bool running;
//
//		Ptr<cuda::StereoBM> bm;
//		Ptr<cuda::StereoBeliefPropagation> bp;
//		Ptr<cuda::StereoConstantSpaceBP> csbp;
//
//
//		// Load images
//		cvtColor(left_src, left, COLOR_BGR2GRAY);
//		cvtColor(right_src, right, COLOR_BGR2GRAY);
//		d_left.upload(left);
//		d_right.upload(right);
//
//		imshow("left", left);
//		imshow("right", right);
//
//		// Set common parameters
//		bm = cuda::createStereoBM(p.ndisp);
//		bp = cuda::createStereoBeliefPropagation(p.ndisp);
//		csbp = cv::cuda::createStereoConstantSpaceBP(p.ndisp);
//
//		// Prepare disparity map of specified type
//		Mat disp(left.size(), CV_8U);
//		cuda::GpuMat d_disp(left.size(), CV_8U);
//
//		cout << endl;
//		printParams();
//
//		running = true;
//		while (running)
//		{
//			workBegin();
//			switch (p.method)
//			{
//			case Params::BM:
//				if (d_left.channels() > 1 || d_right.channels() > 1)
//				{
//					cout << "BM doesn't support color images\n";
//					cvtColor(left_src, left, COLOR_BGR2GRAY);
//					cvtColor(right_src, right, COLOR_BGR2GRAY);
//					cout << "image_channels: " << left.channels() << endl;
//					d_left.upload(left);
//					d_right.upload(right);
//					imshow("left", left);
//					imshow("right", right);
//				}
//				bm->compute(d_left, d_right, d_disp);
//				break;
//			case Params::BP: bp->compute(d_left, d_right, d_disp); break;
//			case Params::CSBP: csbp->compute(d_left, d_right, d_disp); break;
//			}
//			workEnd();
//
//			// Show results
//			d_disp.download(disp);
//			putText(disp, text(), Point(5, 25), FONT_HERSHEY_SIMPLEX, 1.0, Scalar::all(255));
//			imshow("disparity", disp);
//
//			handleKey((char)waitKey(3));
//		}
//	}
//}
