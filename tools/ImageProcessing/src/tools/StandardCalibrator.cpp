#include "StandardCalibrator.h"

#include <opencv2/highgui.hpp>
#include <opencv2/calib3d.hpp>

#include "frames/FrameData.h"
#include "frames/FrameSize.h"
#include "outputs/OpenCvOutput.h"

using namespace ImageProcessing;

StandardCalibrator::StandardCalibrator()
{
}

void StandardCalibrator::Calibrate(const std::shared_ptr<CameraSourceInterface>& camera, const std::string & filename)
{
	//if (!camera->IsOpen())
	//{
	//	camera->Open();
	//}

	auto window_name = "ArToolkitCalibration";

    auto initial_frame = cv::imread("img/0.jpg");
    auto mat_size = initial_frame.size();
    auto frame_size = FrameSize(mat_size.width, mat_size.height, 3);
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
        auto frame = cv::imread("img/" + std::to_string(frame_counter) + ".jpg");
        memcpy(buffer_left.get(), frame.data, frame_size.BufferSize());
        memcpy(buffer_right.get(), frame.data, frame_size.BufferSize());
		//camera->PrepareNextFrame();
		//camera->GrabFrame(buffer_left.get(), buffer_right.get());

        cv::Mat img_left = frame;
		//cv::Mat img_left(mat_size, frame_size.CvType(), buffer_left.get());
		cv::Mat img_left_gray(mat_size, CV_8UC1);
		cv::cvtColor(img_left, img_left_gray, frame_size.CvToGray());

		//cv::Mat img_right(mat_size, frame_size.CvType(), buffer_right.get());
        cv::Mat img_right = frame;
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
		cv::putText(img_left, std::to_string(calibrated_corners_left.size()) + std::string("/") + std::to_string(calib_image_count), cv::Point(50, 50), cv::FONT_HERSHEY_COMPLEX, 1.0, cv::Scalar(255, 0, 0, 255));
		auto fd = std::make_shared<FrameData>(frame_counter++, buffer_left, buffer_right, frame_size);
		output.RegisterResult(fd);
		output.WriteResult();

		int pressedKey = cv::waitKey(10);
		if (found_corners_left && found_corners_right)
		{
			cv::cornerSubPix(img_left_gray, corners_left, cv::Size(5, 5), cv::Size(-1, -1), cv::TermCriteria(CV_TERMCRIT_ITER, 100, 0.1));
			calibrated_corners_left.push_back(corners_left);
			cv::cornerSubPix(img_right_gray, corners_right, cv::Size(5, 5), cv::Size(-1, -1), cv::TermCriteria(CV_TERMCRIT_ITER, 100, 0.1));
			calibrated_corners_right.push_back(corners_right);
		}

        if (frame_counter >= calib_image_count)
        {
            frame_counter = 0;
        }
	}

	// perform calibration
	//SingleCameraCalibration(filename + std::string("_left.dat"), calibrated_corners_left, mat_size);
	//SingleCameraCalibration(filename + std::string("_right.dat"), calibrated_corners_right, mat_size);
	StereoCameraCalibration(filename, calibrated_corners_left, calibrated_corners_right, mat_size);
	cv::destroyWindow(window_name);

	//cv::Mat map1_l;
	//cv::Mat map2_l;
	//cv::initUndistortRectifyMap(camera_matrix_l_, dist_coeffs_l_, cv::Mat(), cv::Mat(), mat_size, CV_32FC1, map1_l, map2_l);

	//while (true)
	//{
	//	camera->PrepareNextFrame();
	//	camera->GrabFrame(buffer_left.get(), buffer_right.get());


	//	cv::Mat img_left(mat_size, frame_size.CvType(), buffer_left.get());
	//	cv::Mat img_left_undistorted;
	//	//cv::undistort(img_left, img_left_undistorted, camera_matrix_l_, dist_coeffs_l_);
	//	cv::remap(img_left, img_left_undistorted, map1_l, map2_l, cv::INTER_LINEAR);
	//	imshow("left", img_left);
	//	imshow("left_undistorted", img_left_undistorted);

	//	cv::Mat img_right(mat_size, frame_size.CvType(), buffer_right.get());
	//	cv::Mat img_right_undistorted;
	//cv::undistort(img_right, img_right_undistorted, camera_matrix_r_, dist_coeffs_r_);
	//	imshow("right", img_right);
	//	imshow("right_undistorted", img_right_undistorted);

	//	int key = cv::waitKey(1);
	//	if (key == 'x')
	//	{
	//		exit(0);
	//	}
	//}

	// clean up
	//camera->Close();
}

void StandardCalibrator::SingleCameraCalibration(const std::string &filename, const std::vector<std::vector<cv::Point2f>> &image_points, const cv::Size &image_size)
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
	//auto rms = cv::fisheye::calibrate(object_points, image_points, image_size, camera_matrix_, dist_coeffs_, rvecs, tvecs, 0);
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

void StandardCalibrator::StereoCameraCalibration(const std::string & filename, const std::vector<std::vector<cv::Point2f>>& image_points_l, const std::vector<std::vector<cv::Point2f>>& image_points_r, const cv::Size & image_size)
{
	std::vector<std::vector<cv::Point3f>> object_points;
	for (int k = 0; k < image_points_l.size(); k++)
	{
		std::vector<cv::Point3f> obj;
		for (int y = 0; y < corners_num_y; y++)
		{
			for (int x = 0; x < corners_num_x; x++)
			{
				obj.push_back(cv::Point3f(x * pattern_width, y * pattern_width, 0));
			}
		}
		object_points.push_back(obj);
	}

	std::vector<cv::Mat> rvecs;
	std::vector<cv::Mat> tvecs;

	camera_matrix_l_ = cv::Mat::zeros(3, 3, CV_32FC1);
	camera_matrix_l_.at<float>(0, 0) = 1;
	camera_matrix_l_.at<float>(1, 1) = 1;
	cv::calibrateCamera(object_points, image_points_l, image_size, camera_matrix_l_, dist_coeffs_l_, rvecs, tvecs);

	camera_matrix_r_ = cv::Mat::zeros(3, 3, CV_32FC1);
	camera_matrix_r_.at<float>(0, 0) = 1;
	camera_matrix_r_.at<float>(1, 1) = 1;
	cv::calibrateCamera(object_points, image_points_r, image_size, camera_matrix_r_, dist_coeffs_r_, rvecs, tvecs);

	//cv::Mat essential_matrix;
	//cv::Mat fundamental_matrix;
	//cv::stereoCalibrate(object_points, image_points_l, image_points_r, camera_matrix_l_, dist_coeffs_l_, camera_matrix_r_, dist_coeffs_r_, image_size, rvecs, tvecs, essential_matrix, fundamental_matrix);

	cv::FileStorage fs_i_l(filename + "_standard_left_intrinsic.yaml", cv::FileStorage::WRITE);
	cv::FileStorage fs_d_l(filename + "_standard_left_distcoeffs.yaml", cv::FileStorage::WRITE);
	fs_i_l << "standard_left_intrinsic" << camera_matrix_l_;
	fs_d_l << "standard_left_distcoeffs" << dist_coeffs_l_;

	cv::FileStorage fs_i_r(filename + "_standard_right_intrinsic.yaml", cv::FileStorage::WRITE);
	cv::FileStorage fs_d_r(filename + "_standard_right_distcoeffs.yaml", cv::FileStorage::WRITE);
	fs_i_r << "standard_right_intrinsic" << camera_matrix_r_;
	fs_d_r << "standard_right_distcoeffs" << dist_coeffs_r_;
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

