#include "outputs/UnityTextureOutput.h"

#include <d3d11.h>

using namespace ImageProcessing;


UnityTextureOutput::UnityTextureOutput(void *texture_ptr, Eye eye)
	: texture_ptr_(texture_ptr),
	  eye_(eye)
{

}


UnityTextureOutput::~UnityTextureOutput()
{

}


void UnityTextureOutput::Write(const FrameData &frame) noexcept
{
	// TODO: move this bit into constructor, if possible?
	ID3D11Texture2D* d3dtex = (ID3D11Texture2D*)texture_ptr_;
	ID3D11Device *g_D3D11Device;
	d3dtex->GetDevice(&g_D3D11Device);

	ID3D11DeviceContext* ctx = NULL;
	g_D3D11Device->GetImmediateContext(&ctx);

	D3D11_TEXTURE2D_DESC desc;
	d3dtex->GetDesc(&desc);

	auto linelength = frame.size.width * frame.size.depth;
	auto buffer = (eye_ == Eye::LEFT) ? frame.buffer_left.get() : frame.buffer_right.get();

	// TODO: https://gamedev.stackexchange.com/questions/60668/how-to-use-updatesubresource-and-map-unmap ?
	ctx->UpdateSubresource(d3dtex, 0, NULL, buffer, linelength, 0);
	ctx->Release();
}
