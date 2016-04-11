#pragma once

namespace ImageProcessing
{
	class IProcessingModule
	{
	public:
		virtual ~IProcessingModule() {}
		virtual void ProcessImage() = 0;
	};

}
