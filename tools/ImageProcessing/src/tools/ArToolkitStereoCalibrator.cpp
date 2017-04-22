#include "ArToolkitStereoCalibrator.h"

#include <vector>
#include <opencv2/highgui.hpp>
#include <opencv2/calib3d.hpp>

#include "frames/FrameData.h"
#include "frames/FrameSize.h"
#include "outputs/OpenCvOutput.h"


using namespace ImageProcessing;

ArToolkitStereoCalibrator::ArToolkitStereoCalibrator()
{
}

/*
 *	Below is adapted from calib_stereo.cpp sample from ArToolkit
 */

/*
 *  calib_stereo.cpp
 *  ARToolKit5
 *
 *  Camera stereo parameters calibration utility.
 *
 *  Run with "--help" parameter to see usage.
 *
 *  This file is part of ARToolKit.
 *
 *  ARToolKit is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ARToolKit is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with ARToolKit.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  As a special exception, the copyright holders of this library give you
 *  permission to link this library with independent modules to produce an
 *  executable, regardless of the license terms of these independent modules, and to
 *  copy and distribute the resulting executable under terms of your choice,
 *  provided that you also meet, for each linked independent module, the terms and
 *  conditions of the license of that module. An independent module is a module
 *  which is neither derived from nor based on this library. If you modify this
 *  library, you may extend this exception to your version of the library, but you
 *  are not obligated to do so. If you do not wish to do so, delete this exception
 *  statement from your version.
 *
 *  Copyright 2015 Daqri, LLC.
 *  Copyright 2002-2015 ARToolworks, Inc.
 *
 *  Author(s): Hirokazu Kato, Philip Lamb.
 *
 */


void ArToolkitStereoCalibrator::Calibrate(const std::shared_ptr<CameraSourceInterface>& camera, const std::string & filename)
{
	if (!camera->IsOpen())
	{
		camera->Open();
	}

	auto window_name = "ArToolkitCalibration";
	OpenCvOutput output(window_name);

	auto frame_size = FrameSize(camera->GetFrameWidth(), camera->GetFrameHeight(), camera->GetFrameChannels());
	auto mat_size = cv::Size(frame_size.width, frame_size.height);
	auto buffer_left = std::shared_ptr<unsigned char>(new unsigned char[frame_size.BufferSize()], std::default_delete<unsigned char[]>());
	auto buffer_right = std::shared_ptr<unsigned char>(new unsigned char[frame_size.BufferSize()], std::default_delete<unsigned char[]>());

	// TODO: hardcoded..
	//auto pixFormat = AR_PIXEL_FORMAT_BGRA;


	ARParam wparam;
	if (arParamLoad(calibration_file_left.c_str(), 1, &wparam) < 0)
	{
		throw std::exception("Unable to load left calibration file");
	}
	ARParam paramL;
	arParamChangeSize(&wparam, frame_size.width, frame_size.height, &paramL);
	arParamDisp(&paramL);

	if (arParamLoad(calibration_file_right.c_str(), 1, &wparam) < 0)
	{
		throw std::exception("Unable to load right calibration file");
	}
	ARParam paramR;
	arParamChangeSize(&wparam, frame_size.width, frame_size.height, &paramR);
	arParamDisp(&paramR);

	// artoolkit initialisation??
	arMalloc(cornersL, CvPoint2D32f, corners_num_x * corners_num_y);
	arMalloc(cornersR, CvPoint2D32f, corners_num_x * corners_num_y);
	arMalloc(worldCoord, ICP3DCoordT, corners_num_x * corners_num_y);
	for (int i = 0; i < corners_num_x; i++)
	{
		for (int j = 0; j < corners_num_y; j++)
		{
			worldCoord[i*corners_num_y + j].x = pattern_width * i;
			worldCoord[i*corners_num_y + j].y = pattern_width * j;
			worldCoord[i*corners_num_y + j].z = 0.0;
		}
	}

	arMalloc(calibData, ICPCalibDataT, calib_image_count);
	for (int i = 0; i < calib_image_count; i++)
	{
		arMalloc(calibData[i].screenCoordL, ICP2DCoordT, corners_num_x*corners_num_y);
		arMalloc(calibData[i].screenCoordR, ICP2DCoordT, corners_num_x*corners_num_y);
		calibData[i].worldCoordL = worldCoord;
		calibData[i].worldCoordR = worldCoord;
		calibData[i].numL = corners_num_x*corners_num_y;
		calibData[i].numR = corners_num_x*corners_num_y;
	}

	cv::Size pattern_size(corners_num_x, corners_num_y);
	int frame_counter = 0;
	int capturedImageNum = 0;

	// capture corners for calibration
	while (capturedImageNum < calib_image_count) //??
	{
		camera->PrepareNextFrame();
		camera->GrabFrame(buffer_left.get(), buffer_right.get());

		cv::Mat img_left(mat_size, frame_size.CvType(), buffer_left.get());
		cv::Mat img_left_gray(mat_size, CV_8UC1);
		cv::cvtColor(img_left, img_left_gray, frame_size.CvToGray());

		cv::Mat img_right(mat_size, frame_size.CvType(), buffer_right.get());
		cv::Mat img_right_gray(mat_size, CV_8UC1);
		cv::cvtColor(img_right, img_right_gray, frame_size.CvToGray());

		// find chessboard left
		std::vector<cv::Point2f> corners_left;
		auto found_corners_left = cv::findChessboardCorners(img_left_gray, pattern_size, corners_left, CV_CALIB_CB_ADAPTIVE_THRESH | CV_CALIB_CB_FILTER_QUADS);
		cv::drawChessboardCorners(img_left, pattern_size, corners_left, found_corners_left);

		// find chessboard right
		std::vector<cv::Point2f> corners_right;
		bool found_corners_right = false;

		if (found_corners_left)
		{
			found_corners_right = cv::findChessboardCorners(img_right_gray, pattern_size, corners_right, CV_CALIB_CB_ADAPTIVE_THRESH | CV_CALIB_CB_FILTER_QUADS);
			cv::drawChessboardCorners(img_right, pattern_size, corners_right, found_corners_right);
		}

		// show preview
		cv::putText(img_left, std::to_string(capturedImageNum) + std::string("/") + std::to_string(calib_image_count), cv::Point(50, 50), cv::FONT_HERSHEY_COMPLEX, 1.0, cv::Scalar(0, 0, 0, 1));
		auto fd = std::make_shared<FrameData>(frame_counter++, buffer_left, buffer_right, frame_size);
		output.RegisterResult(fd);
		output.WriteResult();

		int pressedKey = cv::waitKey(10);
		if (pressedKey == ' ' && found_corners_left && found_corners_right)
		{
			cv::cornerSubPix(img_left_gray, corners_left, cv::Size(5, 5), cv::Size(-1, -1), cv::TermCriteria(CV_TERMCRIT_ITER, 100, 0.1));
			for (int i = 0; i < corners_num_x*corners_num_y; i++) {
				arParamObserv2Ideal(paramL.dist_factor, (double)cornersL[i].x, (double)cornersL[i].y,
					&calibData[capturedImageNum].screenCoordL[i].x, &calibData[capturedImageNum].screenCoordL[i].y, paramL.dist_function_version);
			}

			cv::cornerSubPix(img_right_gray, corners_right, cv::Size(5, 5), cv::Size(-1, -1), cv::TermCriteria(CV_TERMCRIT_ITER, 100, 0.1));
			for (int i = 0; i < corners_num_x*corners_num_y; i++) {
				arParamObserv2Ideal(paramR.dist_factor, (double)cornersR[i].x, (double)cornersR[i].y,
					&calibData[capturedImageNum].screenCoordR[i].x, &calibData[capturedImageNum].screenCoordR[i].y, paramR.dist_function_version);
			}

			capturedImageNum++;
		}
	}


	// ArToolkit Calibration
	//COVHI10400, COVHI10352
	ICPDataT    icpData;
	ICPHandleT *icpHandleL = NULL;
	ICPHandleT *icpHandleR = NULL;
	ARdouble    initTransL2R[3][4], matL[3][4], matR[3][4], invMatL[3][4];
	ARdouble    initMatXw2Xc[3][4];
	ARdouble    transL2R[3][4];
	ARdouble    err;

	if ((icpHandleL = icpCreateHandle(paramL.mat)) == NULL) {
		ARLOG("Error!! icpCreateHandle\n");
		goto done;
	}
	icpSetBreakLoopErrorThresh(icpHandleL, 0.00001);

	if ((icpHandleR = icpCreateHandle(paramR.mat)) == NULL) {
		ARLOG("Error!! icpCreateHandle\n");
		goto done;
	}
	icpSetBreakLoopErrorThresh(icpHandleR, 0.00001);

	for (int i = 0; i < calib_image_count; i++) {
		if (icpGetInitXw2Xc_from_PlanarData(paramL.mat, calibData[i].screenCoordL, calibData[i].worldCoordL, calibData[i].numL,
			calibData[i].initMatXw2Xcl) < 0) {
			ARLOG("Error!! icpGetInitXw2Xc_from_PlanarData\n");
			goto done;
		}
		icpData.screenCoord = calibData[i].screenCoordL;
		icpData.worldCoord = calibData[i].worldCoordL;
		icpData.num = calibData[i].numL;
	}

	if (icpGetInitXw2Xc_from_PlanarData(paramL.mat, calibData[0].screenCoordL, calibData[0].worldCoordL, calibData[0].numL,
		initMatXw2Xc) < 0) {
		ARLOG("Error!! icpGetInitXw2Xc_from_PlanarData\n");
		goto done;
	}
	icpData.screenCoord = calibData[0].screenCoordL;
	icpData.worldCoord = calibData[0].worldCoordL;
	icpData.num = calibData[0].numL;
	if (icpPoint(icpHandleL, &icpData, initMatXw2Xc, matL, &err) < 0) {
		ARLOG("Error!! icpPoint\n");
		goto done;
	}
	if (icpGetInitXw2Xc_from_PlanarData(paramR.mat, calibData[0].screenCoordR, calibData[0].worldCoordR, calibData[0].numR,
		matR) < 0) {
		ARLOG("Error!! icpGetInitXw2Xc_from_PlanarData\n");
		goto done;
	}
	icpData.screenCoord = calibData[0].screenCoordR;
	icpData.worldCoord = calibData[0].worldCoordR;
	icpData.num = calibData[0].numR;
	if (icpPoint(icpHandleR, &icpData, initMatXw2Xc, matR, &err) < 0) {
		ARLOG("Error!! icpPoint\n");
		goto done;
	}
	arUtilMatInv(matL, invMatL);
	arUtilMatMul(matR, invMatL, initTransL2R);

	if (icpCalibStereo(calibData, calib_image_count, paramL.mat, paramR.mat, initTransL2R, transL2R, &err) < 0) {
		ARLOG("Calibration error!!\n");
		goto done;
	}
	ARLOG("Estimated transformation matrix from Left to Right.\n");
	arParamDispExt(transL2R);

	SaveParam(transL2R, filename);

done:
	free(icpHandleL);
	free(icpHandleR);

	// clean up
	cv::destroyWindow(window_name);
	camera->Close();
}

void ArToolkitStereoCalibrator::CopyImage(ARUint8 * p1, ARUint8 * p2, int size, int pixFormat)
{
	int    i, j;

	if (pixFormat == AR_PIXEL_FORMAT_RGB || pixFormat == AR_PIXEL_FORMAT_BGR) {
		for (i = 0; i < size; i++) {
			j = *(p1 + 0) + *(p1 + 1) + *(p1 + 2);
			*(p2++) = j / 3;
			p1 += 3;
		}
	}
	if (pixFormat == AR_PIXEL_FORMAT_RGBA || pixFormat == AR_PIXEL_FORMAT_BGRA) {
		for (i = 0; i < size; i++) {
			j = *(p1 + 0) + *(p1 + 1) + *(p1 + 2);
			*(p2++) = j / 3;
			p1 += 4;
		}
	}
	if (pixFormat == AR_PIXEL_FORMAT_ABGR || pixFormat == AR_PIXEL_FORMAT_ARGB) {
		for (i = 0; i < size; i++) {
			j = *(p1 + 1) + *(p1 + 2) + *(p1 + 3);
			*(p2++) = j / 3;
			p1 += 4;
		}
	}
	if (pixFormat == AR_PIXEL_FORMAT_MONO || pixFormat == AR_PIXEL_FORMAT_420v || pixFormat == AR_PIXEL_FORMAT_420f) {
		for (i = 0; i < size; i++) {
			*(p2++) = *(p1++);
		}
	}
	if (pixFormat == AR_PIXEL_FORMAT_2vuy) {
		for (i = 0; i < size; i++) {
			*(p2++) = *(p1 + 1);
			p1 += 2;
		}
	}
	if (pixFormat == AR_PIXEL_FORMAT_yuvs) {
		for (i = 0; i < size; i++) {
			*(p2++) = *(p1);
			p1 += 2;
		}
	}
}

void ImageProcessing::ArToolkitStereoCalibrator::SaveParam(ARdouble transL2R[3][4], const std::string &filename)
{
	if (arParamSaveExt(filename.c_str(), transL2R) < 0) {
		ARLOG("Parameter write error!!\n");
	}
	else {
		ARLOG("Saved parameter file '%s'.\n", filename);
	}
}
