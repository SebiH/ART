#include "FisheyeCalibrator.h"

#include <opencv2/highgui.hpp>
#include <opencv2/calib3d.hpp>

#include "frames/FrameData.h"
#include "frames/FrameSize.h"
#include "outputs/OpenCvOutput.h"

using namespace ImageProcessing;

FisheyeCalibrator::FisheyeCalibrator()
{
}

void FisheyeCalibrator::Calibrate(const std::shared_ptr<CameraSourceInterface>& camera, const std::string & filename)
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
	//SingleCameraCalibration(filename + std::string("_left.dat"), calibrated_corners_left, mat_size);
	//SingleCameraCalibration(filename + std::string("_right.dat"), calibrated_corners_right, mat_size);
	StereoCameraCalibration("", calibrated_corners_left, calibrated_corners_right, mat_size);
	cv::destroyWindow(window_name);

	while (true)
	{
		camera->PrepareNextFrame();
		camera->GrabFrame(buffer_left.get(), buffer_right.get());


		cv::Mat img_left(mat_size, frame_size.CvType(), buffer_left.get());
		cv::Mat img_left_undistorted;
		cv::fisheye::undistortImage(img_left, img_left_undistorted, camera_matrix_l_, dist_coeffs_l_);
		imshow("left", img_left_undistorted);
		imshow("left_unistorted", img_left_undistorted);

		cv::Mat img_right(mat_size, frame_size.CvType(), buffer_right.get());
		cv::Mat img_right_undistorted;
		cv::fisheye::undistortImage(img_right, img_right_undistorted, camera_matrix_r_, dist_coeffs_r_);
		imshow("right", img_right_undistorted);
		imshow("right_undistorted", img_right_undistorted);

		int key = cv::waitKey(50);
		if (key == 'x')
		{
			exit(0);
		}
	}

	// clean up
	camera->Close();
}

void FisheyeCalibrator::SingleCameraCalibration(const std::string &filename, const std::vector<std::vector<cv::Point2f>> &image_points, const cv::Size &image_size)
{
	// calculate opencv calibration
	std::vector<std::vector<cv::Point3f>> object_points(1);
	for (int k = 0; k < image_points.size(); k++)
	{
		std::vector<cv::Point3f> obj;
		for (int y = 0; y < corners_num_y; y++)
		{
			for (int x = 0; x < corners_num_x; x++)
			{
				obj.push_back(cv::Point3f(float(x * pattern_width), float(y * pattern_width), 0));
			}
		}
		object_points.push_back(obj);
	}
	//object_points.resize(image_points.size(), object_points[0]);

	camera_matrix_ = cv::Mat();
	dist_coeffs_ = cv::Mat();
	std::vector<cv::Mat> rvecs;
	std::vector<cv::Mat> tvecs;

	//auto rms = cv::calibrateCamera(object_points, image_points, image_size, camera_matrix, dist_coeffs, rvecs, tvecs, 0);
	auto rms = cv::fisheye::calibrate(object_points, image_points, image_size, camera_matrix_, dist_coeffs_, rvecs, tvecs, 0);
	bool ok = cv::checkRange(camera_matrix_) && cv::checkRange(dist_coeffs_);

	if (!ok)
	{
		throw std::exception("Invalid calibration");
	}


	// convert & save as ArToolkit calibration
	float intr[3][4];
	float dist[4];

	for (int j = 0; j < 3; j++)
	{
		for (int i = 0; i < 3; i++)
		{
			intr[j][i] = camera_matrix_.at<double>(j, i);
		}
		intr[j][3] = 0.0f;
	}
	for (int i = 0; i < 4; i++)
	{
		//dist[i] = ((float*)(dist_coeffs.data))[i];
		dist[i] = dist_coeffs_.at<double>(i);
	}

	// return calibration quality (totalAvgError)
	std::vector<float> reproj_errs;
}

// see: https://github.com/sourishg/fisheye-stereo-calibration/blob/master/calibrate.cpp
void FisheyeCalibrator::StereoCameraCalibration(const std::string & filename, const std::vector<std::vector<cv::Point2f>>& image_points_l, const std::vector<std::vector<cv::Point2f>>& image_points_r, const cv::Size & image_size)
{
	std::vector<std::vector<cv::Point3d>> object_points;
	for (int k = 0; k < image_points_l.size(); k++)
	{
		std::vector<cv::Point3d> obj;
		for (int y = 0; y < corners_num_y; y++)
		{
			for (int x = 0; x < corners_num_x; x++)
			{
				obj.push_back(cv::Point3d(x * pattern_width, y * pattern_width, 0));
			}
		}
		object_points.push_back(obj);
	}

	std::vector<std::vector<cv::Point2d>> imgp_left;
	std::vector<std::vector<cv::Point2d>> imgp_right;
	for (int i = 0; i < image_points_l.size(); i++)
	{
		std::vector<cv::Point2d> v1, v2;
		for (int j = 0; j < image_points_l[i].size(); j++)
		{
			v1.push_back(cv::Point2d((double)image_points_l[i][j].x, (double)image_points_l[i][j].y));
			v2.push_back(cv::Point2d((double)image_points_r[i][j].x, (double)image_points_r[i][j].y));
		}
		imgp_left.push_back(v1);
		imgp_right.push_back(v2);
	}

	int flag = 0;
	//flag |= cv::fisheye::CALIB_RECOMPUTE_EXTRINSIC;
	//flag |= cv::fisheye::CALIB_CHECK_COND;
	//flag |= cv::fisheye::CALIB_FIX_SKEW;

	cv::Matx33f R;
	cv::Vec3d T;

	cv::Mat rvecs;
	cv::Mat tvecs;

	auto rms_l = cv::fisheye::calibrate(object_points, imgp_left, image_size, camera_matrix_l_, dist_coeffs_l_, rvecs, tvecs, 0);
	auto rms_r = cv::fisheye::calibrate(object_points, imgp_right, image_size, camera_matrix_r_, dist_coeffs_r_, rvecs, tvecs, 0);
	//cv::fisheye::stereoCalibrate(object_points, imgp_left, imgp_right, camera_matrix_l_, dist_coeffs_l_, camera_matrix_r_, dist_coeffs_r_, image_size, R, T, flag, cv::TermCriteria(3, 12, 0));
}


/*
*	Below is directly adapted from OpenCV's calibration.cpp sample
*/
static double computeReprojectionErrors(const std::vector<std::vector<cv::Point3f>> &objectPoints,
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
