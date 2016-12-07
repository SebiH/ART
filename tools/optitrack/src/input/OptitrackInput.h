#pragma once

#include "input/Input.h"
#include "output/Output.h"

namespace Optitrack
{
	template <class O>
	class OptitrackInput : Input
	{
	static_assert(std::is_base_of<Output, O>::value, "O must derive from Output");

	public:
		OptitrackInput(const O &output);
		virtual ~OptitrackInput();

		void Start();
		void Stop();

	private:
		O output_;
		bool is_running_;
	};
}
