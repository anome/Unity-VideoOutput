#ifndef UnityRenderAPI_h
#define UnityRenderAPI_h
#include "IUnityGraphics.h"

class UnityRenderAPI
{
public:
    virtual void processDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces) = 0;
    virtual void onRenderEvent(int eventID, void *data) = 0;
    virtual void reset() = 0;
    virtual bool isCompatible(IUnityInterfaces*) = 0;
};


#endif /* UnityRenderAPI_h */
