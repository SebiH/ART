#pragma once

#define ENABLE_PERFORMANCE_DEBUGGING 0

#if ENABLE_PERFORMANCE_DEBUGGING 

#include <chrono>
#include "utils/Logger.h"

#define PERF_MEASURE(TIMEPOINT_NAME) \
	std::chrono::time_point<std::chrono::high_resolution_clock> __##TIMEPOINT_NAME = std::chrono::high_resolution_clock::now();

#define PERF_OUTPUT(MESSAGE,TIMEPOINT1,TIMEPOINT2) \
	DebugLog(std::string(MESSAGE) + std::to_string(std::chrono::duration_cast<std::chrono::microseconds>((__##TIMEPOINT2) - (__##TIMEPOINT1)).count())); \


#else

#define PERF_MEASURE(TIMEPOINT_NAME)
#define PERF_OUTPUT(MESSAGE, TIMEPOINT1, TIMEPOINT2)

#endif
