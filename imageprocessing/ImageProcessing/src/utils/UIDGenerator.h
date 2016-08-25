#pragma once

#include <atomic>
#include "utils/UID.h"

namespace ImageProcessing
{
	class UIDGenerator
	{
		// Singleton
	private:
		static UIDGenerator *s_instance_;

	public:
		static UIDGenerator * Instance()
		{
			if (!s_instance_)
			{
				s_instance_ = new UIDGenerator();
			}

			return s_instance_;
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
