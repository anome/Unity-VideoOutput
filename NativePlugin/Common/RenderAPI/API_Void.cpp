#include "API_Void.h"

void API_Void::processDeviceEvent(UnityGfxDeviceEventType eventType, IUnityInterfaces* interfaces)
{
}

void API_Void::onRenderEvent(int eventID, void* data)
{
}

void API_Void::reset()
{
}

bool API_Void::isCompatible(IUnityInterfaces *interface)
{
    return true;
}
