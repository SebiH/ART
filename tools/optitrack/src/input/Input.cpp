#include "input/Input.h"

#include <algorithm>

using namespace Optitrack;

Input::Input()
{
	static int id_counter = 0;
	Id = id_counter++;
}

void Input::AddOutput(const std::shared_ptr<Output> &out)
{
	outputs_.push_back(out);
}

void Input::RemoveOutput(const int output_id)
{
	outputs_.erase(std::remove_if(outputs_.begin(), outputs_.end(), [output_id](const std::shared_ptr<Output> &other_output) {
		return output_id == other_output->Id;
	}), outputs_.end());
}

void Input::Broadcast(OptitrackMessage &msg)
{
	auto msg_raw = msg.Print();

	for (auto &output : outputs_)
	{
		output->Broadcast(msg_raw);
	}
}
