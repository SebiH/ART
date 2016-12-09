#include "output/Output.h"

using namespace Optitrack;

Output::Output()
{
	static int id_counter = 0;
	Id = id_counter++;
}
