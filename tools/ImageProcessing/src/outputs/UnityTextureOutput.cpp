#include "outputs/UnityTextureOutput.h"

#include <d3d11.h>
#include "utils/Logger.h"


using namespace ImageProcessing;


//
// Warning: Must have "-force-d3d11-no-singlethreaded" parameter when starting Unity!
//


UnityTextureOutput::UnityTextureOutput(Eye eye, void *texture_ptr)
    : eye_(eye)
{
    d3dtex_ = (ID3D11Texture2D*)texture_ptr;
    d3dtex_->GetDevice(&g_D3D11Device_);
    InitializeCriticalSection(&lock_);

    if (deferred_ctx_ == NULL)
    {
        HRESULT result = g_D3D11Device_->CreateDeferredContext(0, &deferred_ctx_);
        if (result != S_OK)
        {
            DebugLog("Invalid call! Start Unity with cmd line parameters!");
            deferred_ctx_ = NULL;
        }
    }
}


UnityTextureOutput::~UnityTextureOutput()
{
    if (front_buffer_)
    {
        front_buffer_->Release();
        front_buffer_ = NULL;
    }

    if (back_buffer_)
    {
        back_buffer_->Release();
        back_buffer_ = NULL;
    }

    is_initialized_ = false;
    DeleteCriticalSection(&lock_);
}


void UnityTextureOutput::RegisterResult(const std::shared_ptr<const FrameData> &frame)
{
    auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();


    if (!is_initialized_)
    {
        D3D11_TEXTURE2D_DESC desc;
        memset(&desc, 0, sizeof(desc));
        desc.Width = frame->size.width;
        desc.Height = frame->size.height;
        desc.MipLevels = desc.ArraySize = 1;
        desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
        desc.SampleDesc.Count = 1;
        desc.Usage = D3D11_USAGE_DYNAMIC;
        desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
        desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;

        D3D11_SUBRESOURCE_DATA srInitData;
        auto buffer = (eye_ == Eye::LEFT) ? frame->buffer_left.get() : frame->buffer_right.get();
        srInitData.pSysMem = (void *)buffer;
        srInitData.SysMemPitch = frame->size.width * frame->size.depth;
        srInitData.SysMemSlicePitch = frame->size.width * frame->size.height * frame->size.depth;


        // buffer 1
        {
            HRESULT r = g_D3D11Device_->CreateTexture2D(&desc, &srInitData, &front_buffer_);

            if (r != S_OK)
            {
                DebugLog("Could not initialize front buffer");
                return;
            }
        }

        // buffer 2
        {
            HRESULT r = g_D3D11Device_->CreateTexture2D(&desc, &srInitData, &back_buffer_);

            if (r != S_OK)
            {
                DebugLog("Could not initialize back buffer");
                return;
            }
        }

        is_initialized_ = true;
    }
    else if (deferred_ctx_ != NULL)
    {

        //{
        //    ID3D11DeviceContext* ctx = NULL;
        //    g_D3D11Device_->GetImmediateContext(&ctx);
        //    D3D11_MAPPED_SUBRESOURCE mapped;
        //    ZeroMemory(&mapped, sizeof(mapped));
        //    HRESULT result = ctx->Map(back_buffer_, 0, D3D11_MAP_WRITE, 0, &mapped);

        //    if (result == S_OK && mapped.pData != (void *)0xcccccccccccccccc)
        //    {
        //        memcpy(mapped.pData, buffer, frame->size.BufferSize());
        //    }

        //    ctx->Unmap(back_buffer_, 0);
        //    ctx->Release();
        //}

        D3D11_MAPPED_SUBRESOURCE mapped;
        ZeroMemory(&mapped, sizeof(mapped));
        HRESULT map_result = deferred_ctx_->Map(front_buffer_, 0, D3D11_MAP_WRITE_DISCARD, 0, &mapped);

        if (map_result == S_OK)
        {
            memcpy(mapped.pData, buffer, frame->size.BufferSize());

            deferred_ctx_->Unmap(front_buffer_, 0);

            EnterCriticalSection(&lock_);
            deferred_ctx_->FinishCommandList(false, &cmd_list_);
            LeaveCriticalSection(&lock_);
        }

        //EnterCriticalSection(&lock_);
        //{
        //    auto tmp = back_buffer_;
        //    back_buffer_ = front_buffer_;
        //    front_buffer_ = back_buffer_;
        //}
        //LeaveCriticalSection(&lock_);

        //{
        //    ID3D11DeviceContext* ctx = NULL;
        //    g_D3D11Device_->GetImmediateContext(&ctx);

        //    ctx->CopyResource(d3dtex_, back_buffer_);
        //    ctx->Release();
        //}
    }

}

void UnityTextureOutput::WriteResult()
{
    Write(NULL);
}


void UnityTextureOutput::Write(const FrameData *frame) noexcept
{
    EnterCriticalSection(&lock_);


    if (is_initialized_)
    {

        ID3D11DeviceContext* ctx = NULL;
        g_D3D11Device_->GetImmediateContext(&ctx);

        if (cmd_list_ != NULL)
        {
            ctx->ExecuteCommandList(cmd_list_, true);
            cmd_list_->Release();
            cmd_list_ = NULL;
        }

        ctx->CopyResource(d3dtex_, front_buffer_);
        ctx->Release();
    }
    LeaveCriticalSection(&lock_);
}
