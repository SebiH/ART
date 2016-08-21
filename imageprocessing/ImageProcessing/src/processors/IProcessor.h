#pragma once

#include "ImageMetaData.h"
#include <string>

namespace ImageProcessing
{
	class IProcessor
	{
	public:
		virtual ~IProcessor() {}
		
		virtual void Initialize(const ImageProcessing::ImageMetaData) = 0;
		virtual void Process(const ImageProcessing::ImageMetaData) = 0;

		// Workaround to accomodate different properties while trying to maintain simple Unity API..
		// GetDoubleProperty, GetIntProperty, etc.?
		TODO: maybe look into LogDebugging blah?
		virtual double GetProperty(const std::string) = 0;
		virtual void SetProperty(const std::string, const double) = 0;
	};
}
