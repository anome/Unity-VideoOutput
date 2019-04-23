#include "API_SpoutD3D11.h"
#include "PlatformBase.h"
#include "UnityLoggerCPP.h"
#if SUPPORT_D3D11
#include "IUnityGraphics.h"
#include "IUnityGraphicsD3D11.h"


// Local mutex object used to prevent race conditions between the main
// thread and the render thread. This should be locked at the following
// points:
// - OnRenderEvent (this is the only point called from the render thread)
// - Plugin function that calls SharedObject or Spout API functions.
std::mutex lock_;

API_SpoutD3D11::API_SpoutD3D11()
{
}


void API_SpoutD3D11::reset()
{

}

bool API_SpoutD3D11::isCompatible(IUnityInterfaces *interfaces)
{
	return (interfaces->Get<IUnityGraphics>()->GetRenderer() == kUnityGfxRendererD3D11);
}

void API_SpoutD3D11::processDeviceEvent(UnityGfxDeviceEventType eventType, IUnityInterfaces* interfaces)
{
	assert(interfaces);

	// Do nothing if it's not the D3D11 renderer.
	if (interfaces->Get<IUnityGraphics>()->GetRenderer() != kUnityGfxRendererD3D11)
	{
		return;
	}

	//UnityLog("OnGraphicsDeviceEvent (%d)", event_type);

	auto& spoutGlobals = SpoutData::get();

	if (eventType == kUnityGfxDeviceEventInitialize)
	{
		// Retrieve the D3D11 interface.
		spoutGlobals.D3D11Device = interfaces->Get<IUnityGraphicsD3D11>()->GetDevice();

		// Initialize the Spout global objects.
		spoutGlobals.spoutFactory = std::make_unique<spoutDirectX>();
		spoutGlobals.spoutSenderFactory = std::make_unique<spoutSenderNames>();

		// Apply the max sender registry value.
		DWORD max_senders;
		if (spoutGlobals.spoutFactory->ReadDwordFromRegistry(&max_senders, "Software\\Leading Edge\\Spout", "MaxSenders"))
		{
			spoutGlobals.spoutSenderFactory->SetMaxSenders(max_senders);
		}
	}
	else if (eventType == kUnityGfxDeviceEventShutdown)
	{
		// Invalidate the D3D11 interface.
		spoutGlobals.D3D11Device = nullptr;

		// Finalize the Spout globals.
		spoutGlobals.spoutFactory.reset();
		spoutGlobals.spoutSenderFactory.reset();
	}
}

// TODO: handle data info
void API_SpoutD3D11::onRenderEvent(int eventId, void *data)
{
	// Do nothing if the D3D11 interface is not available. This only
		// happens on Editor. It may leak some resoruces but we can't do
		// anything about them.
	if (!SpoutData::get().isReady())
	{
		return;
	}

	std::lock_guard<std::mutex> guard(lock_);

	auto* pobj = reinterpret_cast<SpoutSenderForUnity*>(data);

	if (eventId == 0) // Update event
	{
		if (!pobj->isActive()) pobj->activate();
	}
	else if (eventId == 1) // Dispose event
	{
		delete pobj;
	}
}



//
// Native plugin implementation
//

extern "C" void UNITY_INTERFACE_EXPORT * Spout_CreateSender(const char* name, int width, int height)
{
	if (!SpoutData::get().isReady())
	{
		return nullptr;
	}
	return new SpoutSenderForUnity(name != nullptr ? name : "", width, height);
}

extern "C" void UNITY_INTERFACE_EXPORT *  Spout_GetTexturePointer(void* ptr)
{
	return reinterpret_cast<const SpoutSenderForUnity*>(ptr)->d3d11ResourceView;
}

extern "C" int UNITY_INTERFACE_EXPORT  Spout_GetTextureWidth(void* ptr)
{
	return reinterpret_cast<const SpoutSenderForUnity*>(ptr)->textureWidth;
}

extern "C" int UNITY_INTERFACE_EXPORT  Spout_GetTextureHeight(void* ptr)
{
	return reinterpret_cast<const SpoutSenderForUnity*>(ptr)->textureHeight;
}


#endif // #if SUPPORT_D3D11
