#pragma once

#include <atomic>
#include "utils/UID.h"

namespace ImageProcessing
{
	class UIDGenerator
	{
		// Singleton
	public:
		static UIDGenerator * Instance()
		{
			static UIDGenerator *instance = new UIDGenerator();
			return instance;
		}

	public:
		UIDGenerator() : id_counter_(0) { }
		virtual ~UIDGenerator() { }


	private:
		std::atomic<UID> id_counter_;
		
	public:
		const UID GetUID()
		{
			auto uid = ++id_counter_;
			return uid;
		}
	};
}
