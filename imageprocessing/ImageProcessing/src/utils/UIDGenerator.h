#pragma once

#include <atomic>

namespace ImageProcessing
{
	typedef int UID;

	class UIDGenerator
	{
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
		UIDGenerator() : _idCounter(0) { }
		virtual ~UIDGenerator() { }


	private:
		std::atomic<UID> _idCounter;
		
		const UID GetUID()
		{
			auto uid = ++_idCounter;
			return uid;
		}
	};
}
