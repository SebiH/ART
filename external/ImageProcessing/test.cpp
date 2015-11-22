#pragma once

#define DllExport   __declspec( dllexport )

extern "C" float DllExport FooPluginFunction() { return 5.0F; }
