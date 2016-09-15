#pragma once

#include "processors/Processor.h"

namespace ImageProcessing
{
	class ArucoProcessor : public Processor
	{
	private:

	public:
		ArucoProcessor();
		~ArucoProcessor();

		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData>& frame) override;

		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json config) override;
	};
}
