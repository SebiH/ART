#pragma once

#include <d3d11.h>
#include "outputs/Output.h"

namespace ImageProcessing
{
    class D3dOutput : public Output
    {
    private:
        HWND hwnd_;
        IDXGISwapChain *swapchain;
        ID3D11DeviceContext *devcon;

        ID3D11Device *g_D3D11Device_;
        ID3D11Texture2D *d3dtex_;

        bool is_initialized_ = false;
        CRITICAL_SECTION lock_;
        ID3D11Texture2D* front_buffer_ = NULL;
        ID3D11Texture2D* back_buffer_ = NULL;

        ID3D11DeviceContext* deferred_ctx_ = NULL;
        ID3D11CommandList* cmd_list_ = NULL;
    public:

        D3dOutput();
        virtual ~D3dOutput();

        void RegisterResult(const std::shared_ptr<const FrameData> &result) override;
        void WriteResult() override;

    protected:
        virtual void Write(const FrameData *frame) noexcept override;
    };
}
