#pragma once

#include <memory>
#include <string>
#include <json/json.hpp>

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

		virtual nlohmann::json GetProperties() = 0;
		virtual void SetProperties(const nlohmann::json config) = 0;
	};
}
