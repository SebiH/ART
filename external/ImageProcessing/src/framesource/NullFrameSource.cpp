#include "NullFrameSource.h"

using namespace ImageProcessing;

const int DUMMY_CHANNELS = 4;

NullFrameSource::NullFrameSource(int width, int height)
	: dummyWidth(width),
	  dummyHeight(height),
	  dummyMemorySize(width * height * DUMMY_CHANNELS)
{
	dummyMemory = std::unique_ptr<unsigned char[]>(new unsigned char[dummyMemorySize]);
}


NullFrameSource::~NullFrameSource()
{
}


void NullFrameSource::close()
{
}


ImageInfo NullFrameSource::poll(long &frameId, unsigned char *bufferLeft, unsigned char *bufferRight)
{
	if (bufferLeft != nullptr)
	{
		memcpy(bufferLeft, dummyMemory.get(), dummyMemorySize);
	}

	if (bufferRight != nullptr)
	{
		memcpy(bufferRight, dummyMemory.get(), dummyMemorySize);
	}
}


std::size_t NullFrameSource::getImageBufferSize() const
{
	return dummyMemorySize;
}

int NullFrameSource::getFrameWidth() const
{
	return dummyWidth;
}

int NullFrameSource::getFrameHeight() const
{
	return dummyHeight;
}

int NullFrameSource::getFrameChannels() const
{
	return DUMMY_CHANNELS;
}

int NullFrameSource::getCamExposure() const
{
	return 0;
}


void NullFrameSource::setCamExposure(int val) const
{
}


int NullFrameSource::getCamGain() const
{
	return 0;
}


void NullFrameSource::setCamGain(int val) const
{
}

bool NullFrameSource::isOpen() const
{
	return true;
}


int NullFrameSource::getCamBLC() const
{
	return 0;
}

void NullFrameSource::setCamBLC(const int val) const
{
}

bool NullFrameSource::getCamAutoWhiteBalance() const
{
	return true;
}

void NullFrameSource::setCamAutoWhiteBalance(const bool val) const
{
}

int NullFrameSource::getCamWhiteBalanceR() const
{
	return 0;
}

void NullFrameSource::setCamWhiteBalanceR(const int val) const
{
}

int NullFrameSource::getCamWhiteBalanceG() const
{
	return 0;
}

void NullFrameSource::setCamWhiteBalanceG(const int val) const
{
}

int NullFrameSource::getCamWhiteBalanceB() const
{
	return 0;
}

void NullFrameSource::setCamWhiteBalanceB(const int val) const
{
}

int NullFrameSource::getCamFps() const
{
	return 1;
}
