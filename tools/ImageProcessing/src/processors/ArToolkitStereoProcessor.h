#pragma once

#include <AR/ar.h>
#include <json/json.hpp>
#include <memory>
#include <string>
#include "processors/Processor.h"
#include "frames/FrameSize.h"

namespace ImageProcessing
{
	class ArToolkitStereoProcessor : public Processor
	{
	private:
		std::unique_ptr<ARParamLT> param_lookup_table_;
		std::unique_ptr<ARPattHandle> ar_patt_handle_;
		std::unique_ptr<AR3DHandle> ar_3d_handle_;
		std::unique_ptr<ARHandle> ar_handle_;


	public:
		ArToolkitStereoProcessor(std::string config);
		virtual ~ArToolkitStereoProcessor();
		virtual std::shared_ptr<const FrameData> Process(const std::shared_ptr<const FrameData> &frame) override;

		virtual nlohmann::json GetProperties() override;
		virtual void SetProperties(const nlohmann::json &config) override;

	private:
		void Initialise(std::string parameter_filename, int width, int height, int depth);
	};
}
