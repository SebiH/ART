#pragma once

#include <atomic>
#include <condition_variable>
#include <memory>
#include <mutex>
#include <vector>

#include "cameras/ICameraSource.h"
#include "cameras/FrameMetaData.h"
#include "utils/Event.h"

namespace ImageProcessing
{
	class ActiveCamera
	{
		// singleton
	private:
		static ActiveCamera* s_instance;

	public:
		static ActiveCamera * Instance()
		{
			if (!s_instance)
			{
				s_instance = new ActiveCamera();
			}
			return s_instance;
		}

	private:
		std::mutex _camSourceMutex;
		std::mutex _frameDataMutex;
		std::mutex _frameIdMutex;

		std::condition_variable _frameNotifier;
		std::shared_ptr<ICameraSource> _cameraSource;

		std::atomic<int> _frameCounter = -1;
		std::unique_ptr<unsigned char[]> _frameDataLeft, _frameDataRight;
		FrameMetaData _currentFrameMetaData;

	public:
		Event<const FrameMetaData&> OnImageMetaDataChanged;

	private:
		ActiveCamera();
		~ActiveCamera();

	private:
		void ResizeBuffers(const FrameMetaData &newData);
		void FetchFrame();

	public:
		void SetSource(const std::shared_ptr<ICameraSource> &cam);
		std::shared_ptr<ICameraSource> GetSource();

		void WaitForNewFrame(int currentFrameId);
		void WriteFrame(unsigned char *leftBuffer, unsigned char *rightBuffer);
		FrameMetaData GetCurrentFrameData();
	};
}
