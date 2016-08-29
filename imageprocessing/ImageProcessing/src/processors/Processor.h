#pragma once

#include <memory>
#include <string>

#include "frames/FrameData.h"
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
		
		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData> &frame) = 0;

		// Workaround to accomodate different properties while trying to maintain simple Unity API..
		// GetDoubleProperty, GetIntProperty, etc.?
		//TODO: maybe look into LogDebugging blah?
		//TODO: config via json?
		//virtual std::string GetProperty(const std::string) = 0;
		//virtual void SetProperty(const std::string) = 0;
	};
}
