#pragma once

#include <d3d11.h>
#include "outputs/Output.h"

namespace ImageProcessing
{
	class UnityTextureOutput : public Output
	{
	public:
		enum Eye {
			LEFT = 0,
			RIGHT = 1
		};


	private:
		Eye eye_;
		void *texture_ptr_;
        ID3D11Texture2D* d3dtex_;
        ID3D11Device *g_D3D11Device_;

	public:

		UnityTextureOutput(Eye eye, void *texture_ptr);
		virtual ~UnityTextureOutput();

	protected:
		virtual void Write(const FrameData *frame) noexcept override;
	};
}
