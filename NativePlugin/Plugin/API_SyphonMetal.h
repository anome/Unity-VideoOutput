#include "UnityRenderAPI.h"
#include "PlatformBase.h"

#if SUPPORT_METAL

class API_SyphonMetal : public UnityRenderAPI
{
public:
    void processDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);
    void onRenderEvent(int eventID, void *data);
    void reset();
    bool isCompatible(IUnityInterfaces*);
};

#endif /* SUPPORT_METAL */
