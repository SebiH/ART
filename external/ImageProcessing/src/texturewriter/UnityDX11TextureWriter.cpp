#include "UnityDX11TextureWriter.h"

#include <algorithm>
#include <d3d11.h>
#include <utility>

using namespace ImageProcessing;

UnityDX11TextureWriter::UnityDX11TextureWriter(unsigned char * texturePtr, ProcessingOutput::Type type)
	: _texturePtr(texturePtr),
	  _type(type)
{
}

UnityDX11TextureWriter::~UnityDX11TextureWriter()
{
}

void UnityDX11TextureWriter::writeTexture(const std::vector<ProcessingOutput> &processedImages)
{
	for (auto const &processedImg : processedImages)
	{
		if (processedImg.type == _type)
		{
			ID3D11Texture2D* d3dtex = (ID3D11Texture2D*)_texturePtr;
			ID3D11Device *g_D3D11Device;
			d3dtex->GetDevice(&g_D3D11Device);

			ID3D11DeviceContext* ctx = NULL;
			g_D3D11Device->GetImmediateContext(&ctx);

			D3D11_TEXTURE2D_DESC desc;
			d3dtex->GetDesc(&desc);

			// TODO: store metadata to avoid unnecessary updates in case frame hasn't changed?
			ctx->UpdateSubresource(d3dtex, 0, NULL, processedImg.data.get(), desc.Width * processedImg.img.channels(), 0);

			ctx->Release();
			return;
		}
	}
}
