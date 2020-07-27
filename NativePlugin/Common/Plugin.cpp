#include "UnityLoggerCPP.h"
#include "RegisteredRenderApis.h"

namespace
{
    UnityGfxRenderer apiType = kUnityGfxRendererNull;
    IUnityInterfaces* unityInterfaces;
    IUnityGraphics* unityGraphics;
    bool hasLoggedRenderingEngine = false;
    int pluginState = STATE_VOID;
}

#pragma mark - Unity API handler

extern "C" void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);
extern "C" void UNITY_INTERFACE_API OnRenderEvent(int eventID, void* data);
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* interfaces);

// Plugin load event
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* interfaces)
{
    UnityLog("INFO: Init plugin load");
    unityInterfaces = interfaces;
    unityGraphics = unityInterfaces->Get<IUnityGraphics>();
    unityGraphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
    // Run OnGraphicsDeviceEvent(initialize) manually on plugin load.
    // because it might already has been fired in Unity
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

// Plugin unload event
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    UnityLog("INFO: Init plugin unload");
    unityGraphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

#pragma mark Rendering callbacks

// Graphics device event handler
extern "C" void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    // Create graphics API implementation upon initialization
    if( eventType == kUnityGfxDeviceEventInitialize )
    {
        UnityLog("INFO: Graphics Device Event init");
        apiType = unityGraphics->GetRenderer();
    }
    
    // Cleanup graphics API implementation upon shutdown
    if( eventType == kUnityGfxDeviceEventShutdown )
    {
        UnityLog("INFO: Graphics Device Event shutdown");
        apiType = kUnityGfxRendererNull;
    }
    
    if( eventType == kUnityGfxDeviceEventAfterReset)
    {
        UnityLog("INFO: Graphics Device Event after reset");
    }
    
    if ( eventType == kUnityGfxDeviceEventBeforeReset)
    {
        UnityLog("INFO: Graphics Device Event before reset");
    }
    
    RegisteredRenderApis::getApi()->processDeviceEvent(eventType, unityInterfaces);
}

extern "C" void UNITY_INTERFACE_API OnRenderEvent(int eventID, void* data)
{
    if( RegisteredRenderApis::getApi()->isCompatible(unityInterfaces) )
    {
        RegisteredRenderApis::getApi()->onRenderEvent(eventID, data);
    }
}

// Render event callback referer
extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
    return OnRenderEvent;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API setPluginState(int newState)
{
    RegisteredRenderApis::getApi()->reset();
    RegisteredRenderApis::appState = newState;
    // Fake init to trigger the changes
    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}
