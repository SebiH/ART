#include "outputs/UnityTextureOutput.h"

#include <d3d11.h>
#include "debugging/MeasurePerformance.h"

using namespace ImageProcessing;


UnityTextureOutput::UnityTextureOutput(Eye eye, void *texture_ptr)
    : eye_(eye)
{
    d3dtex_ = (ID3D11Texture2D*)texture_ptr;
    d3dtex_->GetDevice(&g_D3D11Device_);
}


UnityTextureOutput::~UnityTextureOutput()
{
    if (is_desc_initialized_)
    {
    }
}


void UnityTextureOutput::RegisterResult(const std::shared_ptr<const FrameData> &frame)
{
    Output::RegisterResult(frame);
    auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();

    if (!is_desc_initialized_)
    {
        AcquireSRWLockExclusive(lock_);
        {
            D3D11_TEXTURE2D_DESC desc;
            memset(&desc, 0, sizeof(desc));
            desc.Width = frame->size.width;
            desc.Height = frame->size.height;
            desc.MipLevels = desc.ArraySize = 1;
            desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
            desc.SampleDesc.Count = 1;
            desc.Usage = D3D11_USAGE_STAGING;
            desc.BindFlags = 0;
            desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;

            D3D11_SUBRESOURCE_DATA srInitData;
            auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();
            srInitData.pSysMem = (void *)buffer;
            srInitData.SysMemPitch = frame->size.width * frame->size.depth;
            srInitData.SysMemSlicePitch = frame->size.width * frame->size.height * frame->size.depth;

            HRESULT r = g_D3D11Device_->CreateTexture2D(&desc, &srInitData, &pTexture);

            if (r == S_OK)
            {
                is_desc_initialized_ = true;
            }
        }

        {
            D3D11_TEXTURE2D_DESC desc;
            memset(&desc, 0, sizeof(desc));
            desc.Width = frame->size.width;
            desc.Height = frame->size.height;
            desc.MipLevels = desc.ArraySize = 1;
            desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
            desc.SampleDesc.Count = 1;
            desc.Usage = D3D11_USAGE_STAGING;
            desc.BindFlags = 0;
            desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;

            D3D11_SUBRESOURCE_DATA srInitData;
            auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();
            srInitData.pSysMem = (void *)buffer;
            srInitData.SysMemPitch = frame->size.width * frame->size.depth;
            srInitData.SysMemSlicePitch = frame->size.width * frame->size.height * frame->size.depth;

            HRESULT r = g_D3D11Device_->CreateTexture2D(&desc, &srInitData, &pTexture2);

            if (r == S_OK)
            {
                is_desc_initialized_2 = true;
            }
        }

        ReleaseSRWLockExclusive(lock_);
    }

    else
    if (is_desc_initialized_2)
    {

        ID3D11DeviceContext* ctx = NULL;
        g_D3D11Device_->GetImmediateContext(&ctx);
        D3D11_TEXTURE2D_DESC desc;
        d3dtex_->GetDesc(&desc);
        D3D11_MAPPED_SUBRESOURCE mapped;
        ZeroMemory(&mapped, sizeof(mapped));
        auto result = ctx->Map(pTexture2, 0, D3D11_MAP_WRITE, 0, &mapped);

        if (mapped.pData)
        {
            memcpy(mapped.pData, buffer, frame->size.BufferSize());
        }

        ctx->Unmap(pTexture2, 0);
        ctx->Release();
    }

}


void UnityTextureOutput::Write(const FrameData *frame) noexcept
{
    //auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();

    //if (!is_desc_initialized_)
    //{
    //    D3D11_TEXTURE2D_DESC desc;
    //    memset(&desc, 0, sizeof(desc));
    //    desc.Width = frame->size.width;
    //    desc.Height = frame->size.height;
    //    desc.MipLevels = desc.ArraySize = 1;
    //    desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    //    desc.SampleDesc.Count = 1;
    //    desc.Usage = D3D11_USAGE_STAGING;
    //    desc.BindFlags = 0;
    //    desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;

    //    D3D11_SUBRESOURCE_DATA srInitData;
    //    auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();
    //    srInitData.pSysMem = (void *)buffer;
    //    srInitData.SysMemPitch = frame->size.width * frame->size.depth;
    //    srInitData.SysMemSlicePitch = frame->size.width * frame->size.height * frame->size.depth;

    //    HRESULT r = g_D3D11Device_->CreateTexture2D(&desc, &srInitData, &pTexture);

    //    if (r == S_OK)
    //    {
    //        is_desc_initialized_ = true;
    //    }
    //}





    if (is_desc_initialized_)
    {
        ID3D11DeviceContext* ctx = NULL;
        g_D3D11Device_->GetImmediateContext(&ctx);




        // TODO: critical section instead of SRWLock, DoubleBuffering to avoid locks??
        // Test if it even works in MultiThreading in the first place...
        //D3D11_MAPPED_SUBRESOURCE mapped;
        //ZeroMemory(&mapped, sizeof(mapped));
        //auto result = ctx->Map(pTexture, 0, D3D11_MAP_WRITE, 0, &mapped);

        //if (mapped.pData)
        //{
        //    auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();
        //    memcpy(mapped.pData, buffer, frame->size.BufferSize());
        //}

        //ctx->Unmap(pTexture, 0);
        ///


        PERF_MEASURE(start)
        ctx->CopyResource(d3dtex_, pTexture);
        PERF_MEASURE(end)
        PERF_OUTPUT("CopyResource", start, end)
        ctx->Release();
    }
}
