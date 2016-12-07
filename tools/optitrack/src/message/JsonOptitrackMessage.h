#pragma once

// turn on RapidJSON std::string support
#define RAPIDJSON_HAS_STDSTRING 1

// turn on RapidJSON SSE4.2 support
#define RAPIDJSON_SSE42 1

#include <rapidjson/writer.h>
#include "message/OptitrackMessage.h"

namespace Optitrack
{
	class JsonOptitrackMessage : public OptitrackMessage
	{
	public:
		JsonOptitrackMessage();
		virtual ~JsonOptitrackMessage();
		void AddRigidbody(const Rigidbody &body);
		const char * Print();

	private:
		rapidjson::StringBuffer buffer_;
		rapidjson::Writer<rapidjson::StringBuffer> writer_;
		bool is_sealed_;
	};
}
