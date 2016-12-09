#pragma once

#include <memory>
#include <vector>
#include "message/OptitrackMessage.h"
#include "output/Output.h"

namespace Optitrack
{
	class Input
	{
	public:
		Input();
		virtual ~Input() {}

		int Id;

		virtual void Start() = 0;
		virtual void Stop() = 0;

		void AddOutput(const std::shared_ptr<Output> &out);
		void RemoveOutput(const int output_id);

	protected:
		std::vector<std::shared_ptr<Output>> outputs_;
		void Broadcast(OptitrackMessage &msg);
	};
}
