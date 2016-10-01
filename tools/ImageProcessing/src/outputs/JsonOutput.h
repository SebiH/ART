#pragma once

#include "outputs/Output.h"

namespace ImageProcessing
{
	class JsonOutput : public Output
	{
	public:
		typedef void(__stdcall * JsonCallback) (const char *str);

	private:
		JsonCallback callback_;

	public:
		JsonOutput(JsonCallback &callback);
		~JsonOutput();

		void RegisterResult(const std::shared_ptr<const FrameData> &result) override;
		void WriteResult() override;

	protected:
		virtual void Write(const FrameData *frame) noexcept override;
	};
}
