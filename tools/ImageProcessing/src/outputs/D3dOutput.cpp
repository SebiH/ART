#include "outputs/D3dOutput.h"

#include <d3d11.h>
#include "utils/Logger.h"

#pragma comment(lib, "d3d11.lib")

using namespace ImageProcessing;


//
// Warning: Must have "-force-d3d11-no-singlethreaded" parameter when starting Unity!
//


D3dOutput::D3dOutput()
{
    InitializeCriticalSection(&lock_);
    hwnd_ = ::CreateWindowA("STATIC", "D3dOutput", WS_VISIBLE, 0, 0, 100, 100, NULL, NULL, NULL, NULL);

    if (hwnd_ == NULL)
    {
        exit(0);
    }

    DXGI_SWAP_CHAIN_DESC scd;

    ZeroMemory(&scd, sizeof(DXGI_SWAP_CHAIN_DESC));
    scd.BufferCount = 1;
    scd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    scd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    scd.OutputWindow = hwnd_;
    scd.SampleDesc.Count = 4;
    scd.Windowed = TRUE;

    D3D11CreateDeviceAndSwapChain(NULL, D3D_DRIVER_TYPE_HARDWARE, NULL, D3D11_CREATE_DEVICE_DEBUG, NULL, NULL, D3D11_SDK_VERSION, &scd, &swapchain, &g_D3D11Device_, NULL, &devcon);

    HRESULT result = g_D3D11Device_->CreateDeferredContext(0, &deferred_ctx_);
    if (result != S_OK)
    {
        deferred_ctx_ = NULL;
    }




}


D3dOutput::~D3dOutput()
{
    return;

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
}


void D3dOutput::RegisterResult(const std::shared_ptr<const FrameData> &frame)
{
    auto buffer = frame->buffer_right.get();


    if (!is_initialized_)
    {
        //D3D11_BUFFER_DESC vertexBufferDesc = { 0 };
        //vertexBufferDesc.ByteWidth = frame->size.BufferSize();
        //vertexBufferDesc.Usage = D3D11_USAGE_DYNAMIC;
        //vertexBufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
        //vertexBufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
        //vertexBufferDesc.MiscFlags = 0;
        //vertexBufferDesc.StructureByteStride = 0;

        //D3D11_SUBRESOURCE_DATA vertexBufferData;
        //vertexBufferData.pSysMem = buffer;
        //vertexBufferData.SysMemPitch = 0;
        //vertexBufferData.SysMemSlicePitch = 0;


        //HRESULT hr = g_D3D11Device_->CreateBuffer(
        //    &vertexBufferDesc,
        //    &vertexBufferData,
        //    &vertexBuffer2
        //);



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
            desc.Usage = D3D11_USAGE_DEFAULT;
            desc.CPUAccessFlags = 0;
            desc.BindFlags = D3D11_BIND_RENDER_TARGET;
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
        HRESULT map_result = deferred_ctx_->Map(front_buffer_, 0, D3D11_MAP::D3D11_MAP_WRITE_DISCARD, 0, &mapped);

        if (map_result == S_OK)
        {
            memcpy(mapped.pData, buffer, frame->size.BufferSize());

            EnterCriticalSection(&lock_);
            deferred_ctx_->Unmap(front_buffer_, 0);
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

        {

            ID3D11DeviceContext* ctx = NULL;
            g_D3D11Device_->GetImmediateContext(&ctx);

            if (cmd_list_ != NULL)
            {
                ctx->ExecuteCommandList(cmd_list_, true);
                cmd_list_->Release();
                cmd_list_ = NULL;
            }

            ctx->CopyResource(back_buffer_, front_buffer_);
            ctx->Release();
        }
    }

}

void D3dOutput::WriteResult()
{
    Write(NULL);
}


void D3dOutput::Write(const FrameData *frame) noexcept
{
    return;
    EnterCriticalSection(&lock_);
    if (deferred_ctx_ == NULL)
    {
        HRESULT result = g_D3D11Device_->CreateDeferredContext(0, &deferred_ctx_);
        if (result != S_OK)
        {
            DebugLog("Invalid call! Start Unity with cmd line parameters!");
            deferred_ctx_ = NULL;
        }
    }


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
