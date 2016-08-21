#pragma once

#include <memory>
#include "..\util\ModuleManager.h"

extern std::unique_ptr<ImageProcessing::ModuleManager> g_moduleManager;
void InitializeImageProcessing();
void ShutdownImageProcessing();
