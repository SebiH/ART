#pragma once

#include <memory>
#include "cameras\ICamera.h"

enum CameraType
{
	OVR,
	OpenCV,
	Dummy,
	LeapMotion
};

void SetCamera(CameraType type);
ImageProcessing::ICamera* GetCamera();
