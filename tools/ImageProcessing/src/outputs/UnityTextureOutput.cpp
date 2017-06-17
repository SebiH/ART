#include "outputs/UnityTextureOutput.h"

#include <d3d11.h>
#include "debugging/MeasurePerformance.h"

using namespace ImageProcessing;


UnityTextureOutput::UnityTextureOutput(Eye eye, void *texture_ptr)
    : texture_ptr_(texture_ptr),
    eye_(eye)
{
    d3dtex_ = (ID3D11Texture2D*)texture_ptr_;
    d3dtex_->GetDevice(&g_D3D11Device_);
}


UnityTextureOutput::~UnityTextureOutput()
{
    if (is_desc_initialized_)
    {
    }
}


void UnityTextureOutput::Write(const FrameData *frame) noexcept
{
    auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();

    if (!is_desc_initialized_)
    {
        is_desc_initialized_ = true;

        D3D11_TEXTURE2D_DESC desc;
        memset(&desc, 0, sizeof(desc));
        desc.Width = frame->size.width;
        desc.Height = frame->size.height;
        desc.MipLevels = desc.ArraySize = 1;
        desc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
        desc.SampleDesc.Count = 1;
        desc.Usage = D3D11_USAGE_STAGING;
        desc.BindFlags = 0;
        desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;

        D3D11_SUBRESOURCE_DATA srInitData;
        srInitData.pSysMem = (void *)buffer;
        srInitData.SysMemPitch = frame->size.width * frame->size.depth;
        srInitData.SysMemSlicePitch = frame->size.width * frame->size.height * frame->size.depth;

        HRESULT r = g_D3D11Device_->CreateTexture2D(&desc, &srInitData, &pTexture);

        if (r != S_OK)
        {
            is_desc_initialized_ = false;
            return;
        }
    }

    //if (!is_desc_initialized_)
    //{
    //	//d3dtex_->GetDesc(&desc_);
    //	//desc_.Usage = D3D11_USAGE_DYNAMIC;
    //	//desc_.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    //	//desc_.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
    //	//desc_.Format = DXGI_FORMAT::DXGI_FORMAT_B8G8R8A8_TYPELESS;
    //}
    ////D3D11_MAPPED_SUBRESOURCE mapped_data_;
    ////ctx_->Map(d3dtex_, 0, D3D11_MAP_WRITE_DISCARD, 0, &mapped_data_);
    ////memcpy(mapped_data_.pData, buffer, frame->size.BufferSize());
    ////ctx_->Unmap(d3dtex_, 0);

    //// TODO: https://gamedev.stackexchange.com/questions/60668/how-to-use-updatesubresource-and-map-unmap ?

    //{
    //    g_D3D11Device_->GetImmediateContext(&ctx_);
    //    auto linelength = frame->size.width * frame->size.depth;
    //    ctx_->UpdateSubresource(d3dtex_, 0, NULL, buffer, linelength, 0);
    //    ctx_->Release();
    //}


    //*outBufferSize = desc.ByteWidth;
    {
        D3D11_TEXTURE2D_DESC desc;
        pTexture->GetDesc(&desc);

        ID3D11DeviceContext* ctx = NULL;
        g_D3D11Device_->GetImmediateContext(&ctx);
        D3D11_MAPPED_SUBRESOURCE mapped;
        ZeroMemory(&mapped, sizeof(mapped));
        auto result =  ctx->Map(pTexture, 0, D3D11_MAP_WRITE, 0, &mapped);

        PERF_MEASURE(map)
        if (mapped.pData)
        {
            memcpy(mapped.pData, buffer, frame->size.BufferSize());
        }

        ctx->Unmap(pTexture, 0);

        PERF_MEASURE(unmap)
        ctx->CopyResource(d3dtex_, pTexture);
        PERF_MEASURE(cpy)

        ctx->Release();

        PERF_OUTPUT("Map ", map, unmap)
        PERF_OUTPUT("Copy ", unmap, cpy);
    }

    //{
    //    D3D11_TEXTURE2D_DESC desc;
    //    d3dtex_->GetDesc(&desc);

    //    ID3D11DeviceContext* ctx = NULL;
    //    g_D3D11Device_->GetImmediateContext(&ctx);
    //    D3D11_MAPPED_SUBRESOURCE mapped;
    //    ZeroMemory(&mapped, sizeof(mapped));
    //    auto result =  ctx->Map(d3dtex_, 0, D3D11_MAP_WRITE, 0, &mapped);

    //    if (mapped.pData)
    //    {
    //        memcpy(mapped.pData, buffer, frame->size.BufferSize());
    //    }

    //    ctx->Unmap(d3dtex_, 0);
    //    ctx->Release();
    //    
    //    //ctx->CopyResource(d3dtex_, pTexture);
    //}
}
