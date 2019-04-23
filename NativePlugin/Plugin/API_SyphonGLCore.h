#include "PlatformBase.h"
#if SUPPORT_OPENGL_CORE
#include "UnityRenderAPI.h"

class API_SyphonGLCore : public UnityRenderAPI
{
public:    
    void processDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);
    void onRenderEvent(int eventID, void *data);
    void reset();
    bool isCompatible(IUnityInterfaces*);
};

#endif /* SUPPORT_OPENGL_CORE */
