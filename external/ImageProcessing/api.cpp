#pragma once

#define DllExport   __declspec( dllexport )

#include <opencv2/opencv.hpp>
#include <ovrvision_pro.h>

using namespace cv;

extern "C" DllExport void Init()
{
	// TODO.
}


