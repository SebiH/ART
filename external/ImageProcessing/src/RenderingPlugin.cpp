// Example low level rendering Unity plugin
#include "RenderingPlugin.h"
#include "Unity/IUnityGraphics.h"

// --------------------------------------------------------------------------
// Include headers for the graphics APIs we support

#if SUPPORT_D3D9
#include <d3d9.h>
#include "Unity/IUnityGraphicsD3D9.h"
#endif
#if SUPPORT_D3D11
#include <d3d11.h>
#include "Unity/IUnityGraphicsD3D11.h"
#endif
#if SUPPORT_D3D12
// TODO
//#include <d3d12.h>
//#include "Unity/IUnityGraphicsD3D12.h"
#endif


