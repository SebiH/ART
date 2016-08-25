#pragma once

#include <string>
#include "utils/UID.h"
#include "utils/UIDGenerator.h"

namespace ImageProcessing
{
	class Processor
	{
	private:
		const UID id_;

	public:
		Processor() : id_(UIDGenerator::Instance()->GetUID()) { }
		virtual ~Processor() {}

		UID Id() const { return id_; }
		
		virtual void Initialize(const ImageProcessing::ImageMetaData) = 0;
		virtual void Process(const ImageProcessing::ImageMetaData) = 0;

		// Workaround to accomodate different properties while trying to maintain simple Unity API..
		// GetDoubleProperty, GetIntProperty, etc.?
		TODO: maybe look into LogDebugging blah?
		TODO: config via json?
		virtual double GetProperty(const std::string) = 0;
		virtual void SetProperty(const std::string, const double) = 0;
	};
}
