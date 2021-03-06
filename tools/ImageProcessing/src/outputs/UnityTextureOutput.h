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
		ID3D11Device *g_D3D11Device_;
		ID3D11Texture2D *d3dtex_;

        bool is_initialized_ = false;
        CRITICAL_SECTION lock_;
        ID3D11Texture2D* staging_tx_ = NULL;

        ID3D11DeviceContext* deferred_ctx_ = NULL;
        ID3D11CommandList* cmd_list_ = NULL;
	public:

		UnityTextureOutput(Eye eye, void *texture_ptr);
		virtual ~UnityTextureOutput();

        void RegisterResult(const std::shared_ptr<const FrameData> &result) override;
        void WriteResult() override;

	protected:
		virtual void Write(const FrameData *frame) noexcept override;
	};
}
