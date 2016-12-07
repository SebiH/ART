#include "input/OptitrackInput.h"

using namespace Optitrack;

template<class O>
OptitrackInput<O>::OptitrackInput(const O &output)
	: output_(std::move(output))
{
	O.Broadcast("x");
}

template<class O>
Optitrack::OptitrackInput<O>::~OptitrackInput()
{
	Stop();
}

template<class O>
void Optitrack::OptitrackInput<O>::Start()
{
}

template<class O>
void Optitrack::OptitrackInput<O>::Stop()
{
}
