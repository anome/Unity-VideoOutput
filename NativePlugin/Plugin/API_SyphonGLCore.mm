#include "API_SyphonGLCore.h"
#import "API_SyphonCommon.h"
#if SUPPORT_OPENGL_CORE
#import "SyphonGLCore_UnityWrapper.h"
#import "UnityLogger.h"

SyphonGLCore_UnityWrapper *syphonServer = [SyphonGLCore_UnityWrapper new];

void API_SyphonGLCore::processDeviceEvent(UnityGfxDeviceEventType eventType, IUnityInterfaces* interfaces)
{
   if(interfaces->Get<IUnityGraphics>()->GetRenderer() != kUnityGfxRendererOpenGLCore)
    {
        UnityNSLog(@"ERR: bad renderer. Expect bad behaviour after this");
        return;
    }
    if( eventType == kUnityGfxDeviceEventShutdown )
    {
        [syphonServer shutdown];
    }
}

void API_SyphonGLCore::onRenderEvent(int eventID, void *data)
{
    if( eventID == EVENTID_PUBLISH_FRAME )
    {
        [syphonServer publishFrame];
    }
    else if( eventID == EVENTID_RELEASE_SERVER )
    {
        [syphonServer shutdown];
    }
}

void API_SyphonGLCore::reset()
{
    [syphonServer shutdown];
    syphonServer = [SyphonGLCore_UnityWrapper new];
}

bool API_SyphonGLCore::isCompatible(IUnityInterfaces *interfaces)
{
    return (interfaces->Get<IUnityGraphics>()->GetRenderer() == kUnityGfxRendererOpenGLCore);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SyphonGLCoreShutdownServer()
{
    [syphonServer shutdown];
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SyphonGLCoreUpdateTextureId(int textureId)
{
    [syphonServer updateTextureId:textureId];
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SyphonGLCoreCreateServer(const char* serverName, int textureId, int width, int height, bool linearToSRGB)
{
    [syphonServer shutdown];
    [syphonServer setupNewServerWithName:[NSString stringWithUTF8String:serverName] texture:textureId width:width height:height linearToSRGB:linearToSRGB ? YES : NO];
}

#endif // if SUPPORT_OPENGL_CORE
