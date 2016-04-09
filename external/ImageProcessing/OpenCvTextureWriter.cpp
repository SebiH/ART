#include "OpenCvTextureWriter.h"

#include <opencv2/highgui.hpp>

using namespace ImageProcessing;

OpenCvTextureWriter::OpenCvTextureWriter(std::string windowName)
	: _windowName(windowName)
{
	cv::namedWindow(_windowName);
}


OpenCvTextureWriter::~OpenCvTextureWriter()
{

}



void OpenCvTextureWriter::WriteTexture(unsigned char * rawData)
{
	// TODO: need to know size?!
}
