#pragma once


#include "PlatformBase.h"
#if SUPPORT_D3D11
#include "UnityRenderAPI.h"
#include "IUnityGraphics.h"
#include <assert.h>
#include <mutex>
#include "SpoutData.h"
#include "SpoutSenderForUnity.h"

class API_SpoutD3D11 : public UnityRenderAPI
{
public:
	void processDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);
	void onRenderEvent(int eventID, void *data);
	void reset();
	bool isCompatible(IUnityInterfaces*);
	API_SpoutD3D11();
	~API_SpoutD3D11() { }
};


#endif /* SUPPORT_D3D11 */
