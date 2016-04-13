#include "UnityDX11TextureWriter.h"

#include <algorithm>
#include <d3d11.h>
#include <utility>

using namespace ImageProcessing;

UnityDX11TextureWriter::UnityDX11TextureWriter(std::vector<unsigned char *> &texturePtrs)
	: _texturePtrs(std::move(texturePtrs))
{
}

UnityDX11TextureWriter::~UnityDX11TextureWriter()
{
}

void UnityDX11TextureWriter::writeTexture(const std::vector<ProcessingOutput> &processedImages)
{
	auto minSize = std::min<size_t>(processedImages.size(), _texturePtrs.size());

	for (int i = 0; i < minSize; i++)
	{
		auto texturePtr = _texturePtrs[i];
		auto processedImage = processedImages[i].data.get();

		ID3D11Texture2D* d3dtex = (ID3D11Texture2D*)texturePtr;
		ID3D11Device *g_D3D11Device;
		d3dtex->GetDevice(&g_D3D11Device);

		ID3D11DeviceContext* ctx = NULL;
		g_D3D11Device->GetImmediateContext(&ctx);

		D3D11_TEXTURE2D_DESC desc;
		d3dtex->GetDesc(&desc);

		// TODO: store metadata to avoid unnecessary updates in case frame hasn't changed?
		ctx->UpdateSubresource(d3dtex, 0, NULL, processedImage, desc.Width * 4, 0);

		ctx->Release();
	}
}
