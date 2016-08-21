#include "ARToolkitModule.h"

#include <opencv2/imgproc.hpp>
#include <AR/param.h>			// arParamDisp()

#include "../util/UnityUtils.h"

using namespace ImageProcessing;

ARToolkitModule::ARToolkitModule()
{
	markerMatrix = new double[16]; // 4x4 matrix
}

ARToolkitModule::~ARToolkitModule()
{
	cleanup();
	delete[] markerMatrix;
}


static void arglCameraViewRH(const ARdouble para[3][4], ARdouble m_modelview[16], const ARdouble scale)
{
	m_modelview[0 + 0 * 4] = para[0][0]; // R1C1
	m_modelview[0 + 1 * 4] = para[0][1]; // R1C2
	m_modelview[0 + 2 * 4] = para[0][2];
	m_modelview[0 + 3 * 4] = para[0][3];
	m_modelview[1 + 0 * 4] = -para[1][0]; // R2
	m_modelview[1 + 1 * 4] = -para[1][1];
	m_modelview[1 + 2 * 4] = -para[1][2];
	m_modelview[1 + 3 * 4] = -para[1][3];
	m_modelview[2 + 0 * 4] = -para[2][0]; // R3
	m_modelview[2 + 1 * 4] = -para[2][1];
	m_modelview[2 + 2 * 4] = -para[2][2];
	m_modelview[2 + 3 * 4] = -para[2][3];
	m_modelview[3 + 0 * 4] = 0.0;
	m_modelview[3 + 1 * 4] = 0.0;
	m_modelview[3 + 2 * 4] = 0.0;
	m_modelview[3 + 3 * 4] = 1.0;
	if (scale != 0.0)
	{
		m_modelview[12] *= scale;
		m_modelview[13] *= scale;
		m_modelview[14] *= scale;
	}
}

std::vector<ProcessingOutput> ARToolkitModule::processImage(unsigned char *rawDataLeft, unsigned char *rawDataRight, const ImageInfo &info)
{
	ARdouble transR[3][4];
	ARPose poseR;


	if (!isInitialized)
	{
		initialize(info.width, info.height);
		isInitialized = true;
	}

	// Detect the markers in the video frame.
	if (arDetectMarker(gARHandleL, rawDataLeft) < 0 || arDetectMarker(gARHandleR, rawDataRight) < 0)
	{
		DebugLog("Error detecting markers");
		throw std::exception("Error detecting markers");
	}

	// Get detected markers
	auto markerInfoL = arGetMarker(gARHandleL);
	auto markerInfoR = arGetMarker(gARHandleR);
	auto markerNumL = arGetMarkerNum(gARHandleL);
	auto markerNumR = arGetMarkerNum(gARHandleR);

	int kL, kR;


	ProcessingOutput outputLeft;
	ProcessingOutput outputRight;
	bool isLeftInit = false;
	bool isRightInit = false;

	for (int i = 0; i < markersSquareCount; i++)
	{
		markersSquare[i].validPrev = markersSquare[i].valid;
		markersSquare[i].valid = FALSE;

		// Check through the marker_info array for highest confidence
		// visible marker matching our preferred pattern.
		kL = kR = -1;
		if (markersSquare[i].patt_type == AR_PATTERN_TYPE_TEMPLATE) {
			for (int j = 0; j < markerNumL; j++) {
				if (markersSquare[i].patt_id == markerInfoL[j].idPatt) {
					if (kL == -1) {
						if (markerInfoL[j].cfPatt >= markersSquare[i].matchingThreshold) kL = j; // First marker detected.
					}
					else if (markerInfoL[j].cfPatt > markerInfoL[kL].cfPatt) kL = j; // Higher confidence marker detected.
				}
			}
			if (kL != -1) {
				markerInfoL[kL].id = markerInfoL[kL].idPatt;
				markerInfoL[kL].cf = markerInfoL[kL].cfPatt;
				markerInfoL[kL].dir = markerInfoL[kL].dirPatt;
			}
			for (int j = 0; j < markerNumR; j++) {
				if (markersSquare[i].patt_id == markerInfoR[j].idPatt) {
					if (kR == -1) {
						if (markerInfoR[j].cfPatt >= markersSquare[i].matchingThreshold) kR = j; // First marker detected.
					}
					else if (markerInfoR[j].cfPatt > markerInfoR[kR].cfPatt) kR = j; // Higher confidence marker detected.
				}
			}
			if (kR != -1) {
				markerInfoR[kR].id = markerInfoR[kR].idPatt;
				markerInfoR[kR].cf = markerInfoR[kR].cfPatt;
				markerInfoR[kR].dir = markerInfoR[kR].dirPatt;
			}
		}
		else {
			for (int j = 0; j < markerNumL; j++) {
				if (markersSquare[i].patt_id == markerInfoL[j].idMatrix) {
					if (kL == -1) {
						if (markerInfoL[j].cfMatrix >= markersSquare[i].matchingThreshold) kL = j; // First marker detected.
					}
					else if (markerInfoL[j].cfMatrix > markerInfoL[kL].cfMatrix) kL = j; // Higher confidence marker detected.
				}
			}
			if (kL != -1) {
				markerInfoL[kL].id = markerInfoL[kL].idMatrix;
				markerInfoL[kL].cf = markerInfoL[kL].cfMatrix;
				markerInfoL[kL].dir = markerInfoL[kL].dirMatrix;
			}
			for (int j = 0; j < markerNumR; j++) {
				if (markersSquare[i].patt_id == markerInfoR[j].idMatrix) {
					if (kR == -1) {
						if (markerInfoR[j].cfMatrix >= markersSquare[i].matchingThreshold) kR = j; // First marker detected.
					}
					else if (markerInfoR[j].cfMatrix > markerInfoR[kR].cfMatrix) kR = j; // Higher confidence marker detected.
				}
			}
			if (kR != -1) {
				markerInfoR[kR].id = markerInfoR[kR].idMatrix;
				markerInfoR[kR].cf = markerInfoR[kR].cfMatrix;
				markerInfoR[kR].dir = markerInfoR[kR].dirMatrix;
			}
		}

		if (kL != -1 || kR != -1) {

			if (kL != -1 && kR != -1) {
				auto err = arGetStereoMatchingErrorSquare(gAR3DStereoHandle, &markerInfoL[kL], &markerInfoR[kR]);
				//ARLOG("stereo err = %f\n", err);
				if (err > 16.0) {
					//ARLOG("Stereo matching error: %d %d.\n", markerInfoL[kL].area, markerInfoR[kR].area);
					if (markerInfoL[kL].area > markerInfoR[kR].area) kR = -1;
					else                                              kL = -1;
				}
			}

			auto err = arGetTransMatSquareStereo(gAR3DStereoHandle, (kL == -1 ? NULL : &markerInfoL[kL]), (kR == -1 ? NULL : &markerInfoR[kR]), markersSquare[i].marker_width, markersSquare[i].trans);

			if (err < 10.0) markersSquare[i].valid = TRUE;

			//if (kL == -1)      ARLOG("[%2d] right:      err = %f\n", i, err);
			//else if (kR == -1) ARLOG("[%2d] left:       err = %f\n", i, err);
			//else               ARLOG("[%2d] left+right: err = %f\n", i, err);

		}

		if (markersSquare[i].valid)
		{
			// Filter the pose estimate.
			if (markersSquare[i].ftmi)
			{
				if (arFilterTransMat(markersSquare[i].ftmi, markersSquare[i].trans, !markersSquare[i].validPrev) < 0)
				{
					DebugLog("Could not filter transform");
					//ARLOGe("arFilterTransMat error with marker %d.\n", i);
				}
			}

			if (!markersSquare[i].validPrev)
			{
				// Marker has become visible, tell any dependent objects.
				//for (j = 0; j < viewCount; j++) {
				//	VirtualEnvironment2HandleARMarkerAppeared(views[j].ve2, i);
				//}
			}

			// We have a new pose, so set that.
			arglCameraViewRH((const ARdouble(*)[4])markersSquare[i].trans, markersSquare[i].pose.T, 1.0f /*VIEW_SCALEFACTOR*/);
			arUtilMatMul((const ARdouble(*)[4])transL2R, (const ARdouble(*)[4])markersSquare[i].trans, transR);
			arglCameraViewRH((const ARdouble(*)[4])transR, poseR.T, 1.0f /*VIEW_SCALEFACTOR*/);


			newMarkerMatrix = true;
			for (int j = 0; j < 16; j++)
			{
				markerMatrix[j] = markersSquare[i].pose.T[j];
			}

			//// Tell any dependent objects about the update.
			//for (j = 0; j < viewCount; j++) {
			//	VirtualEnvironment2HandleARMarkerWasUpdated(views[j].ve2, i, (views[j].viewEye == VIEW_RIGHTEYE ? poseR : markersSquare[i].pose));
			//}

		}
		else
		{

			if (markersSquare[i].validPrev) {
				// Marker has ceased to be visible, tell any dependent objects.
				//for (j = 0; j < viewCount; j++) {
				//	VirtualEnvironment2HandleARMarkerDisappeared(views[j].ve2, i);
				//}
			}
		}
	}



	if (markerNumL > 0)
	{
		isLeftInit = true;
		cv::Mat imgLeft = cv::Mat(cv::Size(info.width, info.height), info.type, rawDataLeft);
		cv::circle(imgLeft, cv::Point(markerInfoL[0].pos[0], markerInfoL[0].pos[1]), 5, cv::Scalar(0, 0, 255, 1.0), 3);

		for (int j = 0; j < 4; j++)
		{
			auto cornerPos = cv::Point(markerInfoL[0].vertex[j][0], markerInfoL[0].vertex[j][1]);
			cv::circle(imgLeft, cornerPos, 3, cv::Scalar(255, 0, 0, 1.0), 3);
		}

		for (int i = 0; i < markersSquareCount; i++)
		{
			std::string text;

			if (markersSquare[i].valid)
			{
				text = std::string("Marker ") + std::to_string(i) + std::string(" VALID");
			}
			else
			{
				text = std::string("Marker ") + std::to_string(i) + std::string(" invalid");
			}

			cv::putText(imgLeft, text, cv::Point(0, 100*(i+1)), cv::FONT_HERSHEY_PLAIN, 3, cv::Scalar(0, 0, 255, 1.0), 2);
		}

		// copy data to separate arrays, since underlying data will be destroyed once cv::Mat is out of scope
		// TODO: verify?
		auto memSize = imgLeft.size().width * imgLeft.size().height * imgLeft.channels();

		outputLeft.type = ProcessingOutput::Type::left;
		outputLeft.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
		outputLeft.img = imgLeft;
		memcpy(outputLeft.data.get(), imgLeft.data, memSize);
	}


	if (markerNumR > 0)
	{
		isRightInit = true;
		cv::Mat imgRight = cv::Mat(cv::Size(info.width, info.height), info.type, rawDataRight);
		cv::circle(imgRight, cv::Point(markerInfoR[0].pos[0], markerInfoR[0].pos[1]), 5, cv::Scalar(0, 0, 255, 1.0), 3);

		for (int j = 0; j < 4; j++)
		{
			auto cornerPos = cv::Point(markerInfoR[0].vertex[j][0], markerInfoR[0].vertex[j][1]);
			cv::circle(imgRight, cornerPos, 3, cv::Scalar(255, 0, 0, 1.0), 3);
		}


		// copy data to separate arrays, since underlying data will be destroyed once cv::Mat is out of scope
		// TODO: verify?
		auto memSize = imgRight.size().width * imgRight.size().height * imgRight.channels();


		outputRight.type = ProcessingOutput::Type::right;
		outputRight.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
		outputRight.img = imgRight;
		memcpy(outputRight.data.get(), imgRight.data, memSize);
	}






	if (!isLeftInit)
	{
		cv::Mat imgLeft = cv::Mat(cv::Size(info.width, info.height), info.type, rawDataLeft);

		for (int i = 0; i < markersSquareCount; i++)
		{
			std::string text;

			if (markersSquare[i].valid)
			{
				text = std::string("Marker ") + std::to_string(i) + std::string(" VALID");
			}
			else
			{
				text = std::string("Marker ") + std::to_string(i) + std::string(" invalid");
			}

			cv::putText(imgLeft, text, cv::Point(0, 100*(i+1)), cv::FONT_HERSHEY_PLAIN, 3, cv::Scalar(0, 0, 255, 1.0), 2);
		}


		auto memSize = imgLeft.size().width * imgLeft.size().height * imgLeft.channels();

		outputLeft.type = ProcessingOutput::Type::left;
		outputLeft.data = std::unique_ptr<unsigned char[]>(new unsigned char[memSize]);
		outputLeft.img = imgLeft;
		memcpy(outputLeft.data.get(), imgLeft.data, memSize);
	}

	if (!isRightInit)
	{
		outputRight.type = ProcessingOutput::Type::right;
		outputRight.data = std::unique_ptr<unsigned char[]>(new unsigned char[info.bufferSize]);
		memcpy(outputRight.data.get(), rawDataRight, info.bufferSize);
		outputRight.img = cv::Mat(cv::Size(info.width, info.height), info.type, outputRight.data.get());
	}

	std::vector<ProcessingOutput> output;
	output.push_back(std::move(outputLeft));
	output.push_back(std::move(outputRight));
	return output;
}

bool ARToolkitModule::hasNewMarkerDetected()
{
	if (newMarkerMatrix)
	{
		newMarkerMatrix = false;
		return true;
	}

	return false;
}

double* ARToolkitModule::getNewMarkerMatrix() const
{
	return markerMatrix;
}


void ARToolkitModule::initialize(int sizeX, int sizeY)
{
	// Cameras
	if (!setupCamera("C:/code/resources/calib_ovrvision_left.dat", sizeX, sizeY, &gCparamLTL))
	{
		throw std::exception("Unable to setup left camera");
	}

	if (!setupCamera("C:/code/resources/calib_ovrvision_right.dat", sizeX, sizeY, &gCparamLTR))
	{
		throw std::exception("Unable to setup right camera");
	}



	// Init AR.
	gARPattHandle = arPattCreateHandle();
	if (!gARPattHandle) {
		DebugLog("Error creating pattern handle.");
		throw std::exception("Error - See log.");
	}

	gARHandleL = arCreateHandle(gCparamLTL);
	gARHandleR = arCreateHandle(gCparamLTR);
	if (!gARHandleL || !gARHandleR) {
		DebugLog("Error creating AR handle.");
		throw std::exception("Error - See log.");
	}
	arPattAttach(gARHandleL, gARPattHandle);
	arPattAttach(gARHandleR, gARPattHandle);

	if (arSetPixelFormat(gARHandleL, AR_PIXEL_FORMAT_BGRA) < 0 || arSetPixelFormat(gARHandleR, AR_PIXEL_FORMAT_BGRA) < 0) {
		DebugLog("Error setting pixel format.");
		throw std::exception("Error - See log.");
	}

	gAR3DHandleL = ar3DCreateHandle(&gCparamLTL->param);
	gAR3DHandleR = ar3DCreateHandle(&gCparamLTR->param);
	if (!gAR3DHandleL || !gAR3DHandleR) {
		DebugLog("Error creating 3D handle.");
		throw std::exception("Error - See log.");
	}

	if (arParamLoadExt("C:/code/resources/calib_ovrvision_stereo.dat", transL2R) < 0) {
		DebugLog("Error: arParamLoadExt.");
		throw std::exception("Error - See log.");
	}
	arUtilMatInv((const ARdouble(*)[4])transL2R, transR2L);
	arParamDispExt(transL2R);
	gAR3DStereoHandle = ar3DStereoCreateHandle(&(gCparamLTL->param), &(gCparamLTR->param), AR_TRANS_MAT_IDENTITY, transL2R);
	if (!gAR3DStereoHandle) {
		DebugLog("Error: ar3DCreateHandle.");
		throw std::exception("Error - See log.");
	}

	//
	// Markers setup.
	//

	// Load marker(s).
	newMarkers("C:/code/resources/markers.dat", gARPattHandle, &markersSquare, &markersSquareCount, &gARPattDetectionMode);

	//
	// Other ARToolKit setup.
	//

	arSetMarkerExtractionMode(gARHandleL, AR_USE_TRACKING_HISTORY_V2);
	arSetMarkerExtractionMode(gARHandleR, AR_USE_TRACKING_HISTORY_V2);
	//arSetMarkerExtractionMode(gARHandleL, AR_NOUSE_TRACKING_HISTORY);
	//arSetMarkerExtractionMode(gARHandleR, AR_NOUSE_TRACKING_HISTORY);
	//arSetLabelingThreshMode(gARHandleL, AR_LABELING_THRESH_MODE_MANUAL); // Uncomment to force manual thresholding.
	//arSetLabelingThreshMode(gARHandleR, AR_LABELING_THRESH_MODE_MANUAL); // Uncomment to force manual thresholding.

	// Set the pattern detection mode (template (pictorial) vs. matrix (barcode) based on
	// the marker types as defined in the marker config. file.
	arSetPatternDetectionMode(gARHandleL, gARPattDetectionMode); // Default = AR_TEMPLATE_MATCHING_COLOR
	arSetPatternDetectionMode(gARHandleR, gARPattDetectionMode); // Default = AR_TEMPLATE_MATCHING_COLOR

	// Other application-wide marker options. Once set, these apply to all markers in use in the application.
	// If you are using standard ARToolKit picture (template) markers, leave commented to use the defaults.
	// If you are usign a different marker design (see http://www.artoolworks.com/support/app/marker.php )
	// then uncomment and edit as instructed by the marker design application.
	//arSetLabelingMode(gARHandleL, AR_LABELING_BLACK_REGION); // Default = AR_LABELING_BLACK_REGION
	//arSetLabelingMode(gARHandleR, AR_LABELING_BLACK_REGION); // Default = AR_LABELING_BLACK_REGION
	//arSetBorderSize(gARHandleL, 0.25f); // Default = 0.25f
	//arSetBorderSize(gARHandleR, 0.25f); // Default = 0.25f
	//arSetMatrixCodeType(gARHandleL, AR_MATRIX_CODE_3x3); // Default = AR_MATRIX_CODE_3x3
	//arSetMatrixCodeType(gARHandleR, AR_MATRIX_CODE_3x3); // Default = AR_MATRIX_CODE_3x3
}

bool ARToolkitModule::setupCamera(std::string filename, int sizeX, int sizeY, ARParamLT **cparamLT_p)
{
	ARParam cparam;

	if (arParamLoad(filename.c_str(), 1, &cparam) < 0)
	{
		DebugLog(std::string("Unable to load ") + filename);
		return false;
	}

	if (cparam.xsize != sizeX || cparam.ysize != sizeY) {
		arParamChangeSize(&cparam, sizeX, sizeY, &cparam);
	}

	if ((*cparamLT_p = arParamLTCreate(&cparam, AR_PARAM_LT_DEFAULT_OFFSET)) == NULL) {
		DebugLog("Unable to create ParamLT");
		return false;
	}

	return true;
}

void ARToolkitModule::cleanup()
{
	arPattDetach(gARHandleL);
	arPattDetach(gARHandleR);
	arPattDeleteHandle(gARPattHandle);
	ar3DStereoDeleteHandle(&gAR3DStereoHandle);
	ar3DDeleteHandle(&gAR3DHandleL);
	ar3DDeleteHandle(&gAR3DHandleR);
	arDeleteHandle(gARHandleL);
	arDeleteHandle(gARHandleR);
	arParamLTFree(&gCparamLTL);
	arParamLTFree(&gCparamLTR);
}
