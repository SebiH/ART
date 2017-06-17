#include "outputs/UnityTextureOutput.h"

#include <d3d11.h>

using namespace ImageProcessing;


UnityTextureOutput::UnityTextureOutput(Eye eye, void *texture_ptr)
	: texture_ptr_(texture_ptr),
	  eye_(eye)
{
	ID3D11Texture2D* d3dtex_ = (ID3D11Texture2D*)texture_ptr_;
	d3dtex_->GetDevice(&g_D3D11Device_);
}


UnityTextureOutput::~UnityTextureOutput()
{

}


void UnityTextureOutput::Write(const FrameData *frame) noexcept
{

	ID3D11DeviceContext* ctx = NULL;
	g_D3D11Device_->GetImmediateContext(&ctx);

	D3D11_TEXTURE2D_DESC desc;
	d3dtex_->GetDesc(&desc);

	auto linelength = frame->size.width * frame->size.depth;
	auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();

	ctx->UpdateSubresource(d3dtex_, 0, NULL, buffer, linelength, 0);
	ctx->Release();
}
