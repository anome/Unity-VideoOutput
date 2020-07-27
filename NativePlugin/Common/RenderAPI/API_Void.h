#include "UnityRenderAPI.h"

class API_Void : public UnityRenderAPI
{
public:
    void processDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);
    void onRenderEvent(int eventID, void* data);
    void reset();
    bool isCompatible(IUnityInterfaces*);
};
