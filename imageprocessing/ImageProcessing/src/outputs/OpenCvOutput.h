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

	protected:
		virtual void Write(const FrameData *frame) noexcept override;
	};
}
