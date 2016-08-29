#pragma once

#include <string>
#include "outputs/Output.h"

namespace ImageProcessing
{
	class OpenCvOutput : public Output
	{
	private:
		std::string windowname_;

	public:
		OpenCvOutput(std::string windowname);
		~OpenCvOutput();

		void RegisterResult(const std::shared_ptr<const FrameData> &result) override;

	protected:
		virtual void Write(const FrameData *frame) noexcept override;
	};
}
