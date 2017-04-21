#include "ArToolkitCalibrator.h"

#include <vector>
#include <opencv2/highgui.hpp>
#include <opencv2/calib3d.hpp>

#include "frames/FrameData.h"
#include "frames/FrameSize.h"
#include "outputs/OpenCvOutput.h"

using namespace ImageProcessing;

ArToolkitCalibrator::ArToolkitCalibrator()
{
}

void ArToolkitCalibrator::Calibrate(const std::shared_ptr<CameraSourceInterface> &camera, const std::string &filename)
{
	if (!camera->IsOpen())
	{
		camera->Open();
	}

	auto window_name = "ArToolkitCalibration";
	auto frame_size = FrameSize(camera->GetFrameWidth(), camera->GetFrameHeight(), camera->GetFrameChannels());
	auto mat_size = cv::Size(frame_size.width, frame_size.height);
	auto buffer_left = std::shared_ptr<unsigned char>(new unsigned char[frame_size.BufferSize()], std::default_delete<unsigned char[]>());
	auto buffer_right = std::shared_ptr<unsigned char>(new unsigned char[frame_size.BufferSize()], std::default_delete<unsigned char[]>());
	OpenCvOutput output(window_name);

	cv::Size pattern_size(corners_num_x, corners_num_y);
	std::vector<std::vector<cv::Point2f>> calibrated_corners_left;
	std::vector<std::vector<cv::Point2f>> calibrated_corners_right;
	int frame_counter = 0;

	// capture corners for calibration
	while (calibrated_corners_left.size() < calib_image_count)
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
		cv::putText(img_left, std::to_string(calibrated_corners_left.size()) + std::string("/") + std::to_string(calib_image_count), cv::Point(50, 50), cv::FONT_HERSHEY_COMPLEX, 1.0, cv::Scalar(0, 0, 0, 1));
		auto fd = std::make_shared<FrameData>(frame_counter++, buffer_left, buffer_right, frame_size);
		output.RegisterResult(fd);
		output.WriteResult();

		int pressedKey = cv::waitKey(10);
		if (pressedKey == ' ' && found_corners_left && found_corners_right)
		{
			cv::cornerSubPix(img_left_gray, corners_left, cv::Size(5, 5), cv::Size(-1, -1), cv::TermCriteria(CV_TERMCRIT_ITER, 100, 0.1));
			calibrated_corners_left.push_back(corners_left);
			cv::cornerSubPix(img_right_gray, corners_right, cv::Size(5, 5), cv::Size(-1, -1), cv::TermCriteria(CV_TERMCRIT_ITER, 100, 0.1));
			calibrated_corners_right.push_back(corners_right);
		}
	}

	// perform calibration
	SingleCameraCalibration(filename + std::string("_left.dat"), calibrated_corners_left, mat_size);
	SingleCameraCalibration(filename + std::string("_right.dat"), calibrated_corners_right, mat_size);

	// clean up
	cv::destroyWindow(window_name);
	camera->Close();
}


double ArToolkitCalibrator::SingleCameraCalibration(const std::string &filename, const std::vector<std::vector<cv::Point2f>> &image_points, const cv::Size &image_size)
{
	std::vector<std::vector<cv::Point3f>> object_points(1);
	for (int y = 0; y < corners_num_y; y++)
	{
		for (int x = 0; x < corners_num_x; x++)
		{
			object_points[0].push_back(cv::Point3f(float(x * pattern_width), float(y * pattern_width), 0));
		}
	}
	object_points.resize(image_points.size(), object_points[0]);

	cv::Mat camera_matrix = cv::Mat::eye(3, 3, CV_64F);
	cv::Mat dist_coeffs = cv::Mat::zeros(8, 1, CV_64F);
	std::vector<cv::Mat> rvecs;
	std::vector<cv::Mat> tvecs;

	auto rms = cv::calibrateCamera(object_points, image_points, image_size, camera_matrix, dist_coeffs, rvecs, tvecs, CV_CALIB_FIX_K4 | CV_CALIB_FIX_K5);
	bool ok = cv::checkRange(camera_matrix) && cv::checkRange(dist_coeffs);

	if (!ok)
	{
		throw std::exception("Invalid calibration");
	}
	
	std::vector<float> reproj_errs;
	return computeReprojectionErrors(object_points, image_points, rvecs, tvecs, camera_matrix, dist_coeffs, reproj_errs);
}


/*
 *	Below is directly adapted from OpenCV's calibration.cpp sample
 */
double ArToolkitCalibrator::computeReprojectionErrors(const std::vector<std::vector<cv::Point3f>> &objectPoints,
	const std::vector<std::vector<cv::Point2f>> &imagePoints,
	const std::vector<cv::Mat> &rvecs,
	const std::vector<cv::Mat> &tvecs,
	const cv::Mat &cameraMatrix,
	const cv::Mat &distCoeffs,
	std::vector<float> &perViewErrors)
{
	std::vector<cv::Point2f> imagePoints2;
	int i, totalPoints = 0;
	double totalErr = 0, err;
	perViewErrors.resize(objectPoints.size());

	for (i = 0; i < (int)objectPoints.size(); i++)
	{
		projectPoints(cv::Mat(objectPoints[i]), rvecs[i], tvecs[i], cameraMatrix, distCoeffs, imagePoints2);
		err = norm(cv::Mat(imagePoints[i]), cv::Mat(imagePoints2), cv::NORM_L2);
		int n = (int)objectPoints[i].size();
		perViewErrors[i] = (float)std::sqrt(err*err / n);
		totalErr += err*err;
		totalPoints += n;
	}

	return std::sqrt(totalErr / totalPoints);
}


/*
 *	Below is directly adapted from ArToolkit's calib_camera.cpp sample
 */


/*
*  calib_camera.cpp
*  ARToolKit5
*
*  Camera calibration utility.
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
*  Author(s): Hirokazu Kato, Philip Lamb
*
*/

void ArToolkitCalibrator::ArToolkitCalibration(const std::string &filename, const int xsize, const int ysize, const std::vector<cv::Point2f> &cornerSet)
{
	ARParam         param;
	CvMat          *objectPoints;
	CvMat          *imagePoints;
	CvMat          *pointCounts;
	CvMat          *intrinsics;
	CvMat          *distortionCoeff;
	CvMat          *rotationVectors;
	CvMat          *translationVectors;
	CvMat          *rotationVector;
	CvMat          *rotationMatrix;
	float           intr[3][4];
	float           dist[4];
	ARdouble        trans[3][4];
	ARdouble        cx, cy, cz, hx, hy, h, sx, sy, ox, oy, err;
	int             i, j, k, l;

	objectPoints = cvCreateMat(calib_image_count * corners_num_x * corners_num_y, 3, CV_32FC1);
	imagePoints = cvCreateMat(calib_image_count * corners_num_x * corners_num_y, 2, CV_32FC1);
	pointCounts = cvCreateMat(calib_image_count, 1, CV_32SC1);
	intrinsics = cvCreateMat(3, 3, CV_32FC1);
	distortionCoeff = cvCreateMat(1, 4, CV_32FC1);
	//rotationVectors = cvCreateMat(calib_image_count, 3, CV_32FC1);
	//translationVectors = cvCreateMat(calib_image_count, 3, CV_32FC1);
	rotationVectors = cvCreateMat(calib_image_count, 1, CV_32FC3);
	translationVectors = cvCreateMat(calib_image_count, 1, CV_32FC3);
	rotationVector = cvCreateMat(1, 3, CV_32FC1);
	rotationMatrix = cvCreateMat(3, 3, CV_32FC1);

	l = 0;
	for (k = 0; k < calib_image_count; k++) {
		for (i = 0; i < corners_num_x; i++) {
			for (j = 0; j < corners_num_y; j++) {
				((float*)(objectPoints->data.ptr + objectPoints->step*l))[0] = pattern_width*i;
				((float*)(objectPoints->data.ptr + objectPoints->step*l))[1] = pattern_width*j;
				((float*)(objectPoints->data.ptr + objectPoints->step*l))[2] = 0.0f;

				((float*)(imagePoints->data.ptr + imagePoints->step*l))[0] = cornerSet[l].x;
				((float*)(imagePoints->data.ptr + imagePoints->step*l))[1] = cornerSet[l].y;

				l++;
			}
		}
		((int*)(pointCounts->data.ptr))[k] = corners_num_x*corners_num_y;
	}

	cvCalibrateCamera2(objectPoints, imagePoints, pointCounts, cvSize(xsize, ysize), intrinsics, distortionCoeff, rotationVectors, translationVectors, 0);

	for (j = 0; j < 3; j++) {
		for (i = 0; i < 3; i++) {
			intr[j][i] = ((float*)(intrinsics->data.ptr + intrinsics->step*j))[i];
		}
		intr[j][3] = 0.0f;
	}
	for (i = 0; i < 4; i++) {
		dist[i] = ((float*)(distortionCoeff->data.ptr))[i];
	}
	ConvParam(intr, dist, xsize, ysize, &param); //COVHI10434 ignored.
	arParamDisp(&param);

	l = 0;
	for (k = 0; k < calib_image_count; k++) {
		for (i = 0; i < 3; i++) {
			((float*)(rotationVector->data.ptr))[i] = ((float*)(rotationVectors->data.ptr + rotationVectors->step*k))[i];
		}
		cvRodrigues2(rotationVector, rotationMatrix);
		for (j = 0; j < 3; j++) {
			for (i = 0; i < 3; i++) {
				trans[j][i] = ((float*)(rotationMatrix->data.ptr + rotationMatrix->step*j))[i];
			}
			trans[j][3] = ((float*)(translationVectors->data.ptr + translationVectors->step*k))[j];
		}
		//arParamDispExt(trans);

		err = 0.0;
		for (i = 0; i < corners_num_x; i++) {
			for (j = 0; j < corners_num_y; j++) {
				cx = trans[0][0] * pattern_width*i + trans[0][1] * pattern_width*j + trans[0][3];
				cy = trans[1][0] * pattern_width*i + trans[1][1] * pattern_width*j + trans[1][3];
				cz = trans[2][0] * pattern_width*i + trans[2][1] * pattern_width*j + trans[2][3];
				hx = param.mat[0][0] * cx + param.mat[0][1] * cy + param.mat[0][2] * cz + param.mat[0][3];
				hy = param.mat[1][0] * cx + param.mat[1][1] * cy + param.mat[1][2] * cz + param.mat[1][3];
				h = param.mat[2][0] * cx + param.mat[2][1] * cy + param.mat[2][2] * cz + param.mat[2][3];
				if (h == 0.0) continue;
				sx = hx / h;
				sy = hy / h;
				arParamIdeal2Observ(param.dist_factor, sx, sy, &ox, &oy, param.dist_function_version);
				sx = ((float*)(imagePoints->data.ptr + imagePoints->step*l))[0];
				sy = ((float*)(imagePoints->data.ptr + imagePoints->step*l))[1];
				err += (ox - sx)*(ox - sx) + (oy - sy)*(oy - sy);
				l++;
			}
		}
		err = sqrt(err / (corners_num_x*corners_num_y));
		ARLOG("Err[%2d]: %f[pixel]\n", k + 1, err);
	}
	SaveParam(&param, filename);

	cvReleaseMat(&objectPoints);
	cvReleaseMat(&imagePoints);
	cvReleaseMat(&pointCounts);
	cvReleaseMat(&intrinsics);
	cvReleaseMat(&distortionCoeff);
	cvReleaseMat(&rotationVectors);
	cvReleaseMat(&translationVectors);
	cvReleaseMat(&rotationVector);
	cvReleaseMat(&rotationMatrix);
}


void ArToolkitCalibrator::ConvParam(float intr[3][4], float dist[4], int xsize, int ysize, ARParam *param)
{
	ARdouble   s;
	int      i, j;

	param->dist_function_version = 4;
	param->xsize = xsize;
	param->ysize = ysize;

	param->dist_factor[0] = (ARdouble)dist[0];     /* k1  */
	param->dist_factor[1] = (ARdouble)dist[1];     /* k2  */
	param->dist_factor[2] = (ARdouble)dist[2];     /* p1  */
	param->dist_factor[3] = (ARdouble)dist[3];     /* p2  */
	param->dist_factor[4] = (ARdouble)intr[0][0];  /* fx  */
	param->dist_factor[5] = (ARdouble)intr[1][1];  /* fy  */
	param->dist_factor[6] = (ARdouble)intr[0][2];  /* x0  */
	param->dist_factor[7] = (ARdouble)intr[1][2];  /* y0  */
	param->dist_factor[8] = (ARdouble)1.0;         /* s   */

	for (j = 0; j < 3; j++) {
		for (i = 0; i < 4; i++) {
			param->mat[j][i] = (ARdouble)intr[j][i];
		}
	}

	s = GetSizeFactor(param->dist_factor, xsize, ysize, param->dist_function_version);
	param->mat[0][0] /= s;
	param->mat[0][1] /= s;
	param->mat[1][0] /= s;
	param->mat[1][1] /= s;
	param->dist_factor[8] = s;
}


void ArToolkitCalibrator::SaveParam(ARParam *param, const std::string &filename)
{
	char *name = NULL, *cwd = NULL;
	size_t len;
	int nameOK;

	//arMalloc(name, char, MAXPATHLEN);
	//arMalloc(cwd, char, MAXPATHLEN);
	//if (!_getcwd(cwd, MAXPATHLEN)) ARLOGe("Unable to read current working directory.\n");

	nameOK = 0;
	//ARLOG("Filename[%s]: ", SAVE_FILENAME);
	//if (fgets(name, MAXPATHLEN, stdin) != NULL) {

		// Trim whitespace from end of name.
		len = strlen(name);
		while (len > 0 && (name[len - 1] == '\r' || name[len - 1] == '\n' || name[len - 1] == '\t' || name[len - 1] == ' ')) {
			len--;
			name[len] = '\0';
		}

		if (len > 0) {
			nameOK = 1;
			if (arParamSave(name, 1, param) < 0) {
				ARLOG("Parameter write error!!\n");
			}
			else {
				ARLOG("Saved parameter file '%s/%s'.\n", cwd, name);
			}
		}
	//}

	// Try and save with a default name.
	if (!nameOK) {
		if (arParamSave(filename.c_str(), 1, param) < 0) {
			ARLOG("Parameter write error!!\n");
		}
		else {
			ARLOG("Saved parameter file '%s/%s'.\n", cwd, filename);
		}
	}

	free(name);
	free(cwd);
}


ARdouble ArToolkitCalibrator::GetSizeFactor(ARdouble dist_factor[], int xsize, int ysize, int dist_function_version)
{
	ARdouble  ox, oy, ix, iy;
	ARdouble  olen, ilen;
	ARdouble  sf, sf1;

	sf = 100.0;

	ox = 0.0;
	oy = dist_factor[7];
	olen = dist_factor[6];
	arParamObserv2Ideal(dist_factor, ox, oy, &ix, &iy, dist_function_version);
	ilen = dist_factor[6] - ix;
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}

	ox = xsize;
	oy = dist_factor[7];
	olen = xsize - dist_factor[6];
	arParamObserv2Ideal(dist_factor, ox, oy, &ix, &iy, dist_function_version);
	ilen = ix - dist_factor[6];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}

	ox = dist_factor[6];
	oy = 0.0;
	olen = dist_factor[7];
	arParamObserv2Ideal(dist_factor, ox, oy, &ix, &iy, dist_function_version);
	ilen = dist_factor[7] - iy;
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}

	ox = dist_factor[6];
	oy = ysize;
	olen = ysize - dist_factor[7];
	arParamObserv2Ideal(dist_factor, ox, oy, &ix, &iy, dist_function_version);
	ilen = iy - dist_factor[7];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}


	ox = 0.0;
	oy = 0.0;
	arParamObserv2Ideal(dist_factor, ox, oy, &ix, &iy, dist_function_version);
	ilen = dist_factor[6] - ix;
	olen = dist_factor[6];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}
	ilen = dist_factor[7] - iy;
	olen = dist_factor[7];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}

	ox = xsize;
	oy = 0.0;
	arParamObserv2Ideal(dist_factor, ox, oy, &ix, &iy, dist_function_version);
	ilen = ix - dist_factor[6];
	olen = xsize - dist_factor[6];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}
	ilen = dist_factor[7] - iy;
	olen = dist_factor[7];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}

	ox = 0.0;
	oy = ysize;
	arParamObserv2Ideal(dist_factor, ox, oy, &ix, &iy, dist_function_version);
	ilen = dist_factor[6] - ix;
	olen = dist_factor[6];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}
	ilen = iy - dist_factor[7];
	olen = ysize - dist_factor[7];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}

	ox = xsize;
	oy = ysize;
	arParamObserv2Ideal(dist_factor, ox, oy, &ix, &iy, dist_function_version);
	ilen = ix - dist_factor[6];
	olen = xsize - dist_factor[6];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}
	ilen = iy - dist_factor[7];
	olen = ysize - dist_factor[7];
	//ARLOG("Olen = %f, Ilen = %f, s = %f\n", olen, ilen, ilen / olen);
	if (ilen > 0) {
		sf1 = ilen / olen;
		if (sf1 < sf) sf = sf1;
	}

	if (sf == 100.0) sf = 1.0;

	return sf;
}
