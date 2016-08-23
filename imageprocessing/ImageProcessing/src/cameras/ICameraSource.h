#pragma once

#include "cameras\FrameMetaData.h"

namespace ImageProcessing
{
	class ICameraSource
	{
	public:
		virtual ~ICameraSource() {}

		virtual void GrabFrame(unsigned char *leftBuffer, unsigned char *rightBuffer) = 0;
		virtual ImageProcessing::FrameMetaData GetFrameMetaData() const = 0;

		virtual void Open() = 0;
		virtual void Close() = 0;
		virtual bool IsOpen() const = 0;

		// Camera Properties
		virtual int GetFrameWidth() const = 0;
		virtual int GetFrameHeight() const = 0;
		virtual int GetFrameChannels() const = 0;
		virtual int GetCamExposure() const = 0;
		virtual void SetCamExposure(const int val) const = 0;
		virtual int GetCamGain() const = 0;
		virtual void SetCamGain(const int val) const = 0;
		virtual int GetCamBLC() const = 0;
		virtual void SetCamBLC(const int val) const = 0;
		virtual bool GetCamAutoWhiteBalance() const = 0;
		virtual void SetCamAutoWhiteBalance(const bool val) const = 0;
		virtual int GetCamWhiteBalanceR() const = 0;
		virtual void SetCamWhiteBalanceR(const int val) const = 0;
		virtual int GetCamWhiteBalanceG() const = 0;
		virtual void SetCamWhiteBalanceG(const int val) const = 0;
		virtual int GetCamWhiteBalanceB() const = 0;
		virtual void SetCamWhiteBalanceB(const int val) const = 0;
		virtual int GetCamFps() const = 0;
	};
}
