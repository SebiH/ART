#include "api/unity_plugin.h"
#include <Unity/IUnityGraphics.h>

#if SUPPORT_D3D9
#include <d3d9.h>
#include <Unity/IUnityGraphicsD3D9.h>
#endif
#if SUPPORT_D3D11
#include <d3d11.h>
#include <Unity/IUnityGraphicsD3D11.h>
#endif
#if SUPPORT_D3D12
// TODO
//#include <d3d12.h>
//#include <Unity/IUnityGraphicsD3D12.h>
#endif

#include "pipelines/PipelineManager.h"


// ------------------------------------------
// Rendering Events
// ------------------------------------------

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	auto pipelines = ImageProcessing::PipelineManager::Instance()->GetPipelines();
	for (auto pipeline : *pipelines)
	{
		pipeline->FlushOutputs();
	}
}


// Usage in C#: GL.IssuePluginEvent(GetRenderEventFunc(), 1);
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}





// ------------------------------------------
// Unity Events
// ------------------------------------------

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;
static UnityGfxRenderer s_RendererType = kUnityGfxRendererNull;

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	switch (eventType)
	{
	case kUnityGfxDeviceEventInitialize:
	{
		s_RendererType = s_Graphics->GetRenderer();
		//InitializeImageProcessing();
		break;
	}
	case kUnityGfxDeviceEventShutdown:
	{
		s_RendererType = kUnityGfxRendererNull;
		//ShutdownImageProcessing();
		break;
	}
	case kUnityGfxDeviceEventBeforeReset:
	{
		//TODO: user Direct3D 9 code
		break;
	}
	case kUnityGfxDeviceEventAfterReset:
	{
		//TODO: user Direct3D 9 code
		break;
	}
	};
}

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}
