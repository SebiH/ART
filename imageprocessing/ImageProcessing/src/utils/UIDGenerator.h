#pragma once

#include <mutex>

namespace ImageProcessing
{
	class UIDGenerator
	{
		typedef int UID;

		// Singleton
	private:
		static UIDGenerator *s_instance;

	public:
		static UIDGenerator * Instance()
		{
			if (!s_instance)
			{
				s_instance = new UIDGenerator();
			}

			return s_instance;
		}

	private:
		UIDGenerator() : _counterMutex(), _idCounter(0) { }
		virtual ~UIDGenerator() { }


	private:
		std::mutex _counterMutex;
		UID _idCounter;
		
		const UID GetUID()
		{
			std::unique_lock<std::mutex> lock(_counterMutex);
			++_idCounter;
			return _idCounter;
		}
	};
}
