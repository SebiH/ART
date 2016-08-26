#pragma once

#include <d3d11.h>
#include "outputs/Output.h"

namespace ImageProcessing
{
	class UnityTextureOutput : public Output
	{
	public:
		enum Eye {
			LEFT,
			RIGHT
		};


	private:
		Eye eye_;
		void *texture_ptr_;

	public:

		UnityTextureOutput(void *texture_ptr, Eye eye);
		virtual ~UnityTextureOutput();

	protected:
		virtual void Write(const FrameData &frame) noexcept override;
	};
}
