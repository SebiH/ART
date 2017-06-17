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
		ID3D11Device *g_D3D11Device_;
		ID3D11Texture2D *d3dtex_;
		ID3D11DeviceContext *ctx_;
		//D3D11_TEXTURE2D_DESC desc_;
		//D3D11_MAPPED_SUBRESOURCE mapped_data_;
		bool is_desc_initialized_ = false;

        ID3D11Texture2D *pTexture = NULL;

	public:

		UnityTextureOutput(Eye eye, void *texture_ptr);
		virtual ~UnityTextureOutput();

	protected:
		virtual void Write(const FrameData *frame) noexcept override;
	};
}
