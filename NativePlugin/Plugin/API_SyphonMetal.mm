#include "API_SyphonMetal.h"
#import "API_SyphonCommon.h"
#import <Metal/Metal.h>
#include "Unity/IUnityGraphicsMetal.h"
#include "UnityLogger.h"
#import "SyphonMetal_UnityWrapper.h"


#if SUPPORT_METAL

@protocol MTLDevice;

id<MTLDevice> metalDevice;
IUnityGraphicsMetal* m_MetalGraphics;
SyphonMetal_UnityWrapper *server = nil;

void API_SyphonMetal::reset()
{
    metalDevice = nil;
    m_MetalGraphics = nil;
    [server shutdown];
    server = nil;
}

bool API_SyphonMetal::isCompatible(IUnityInterfaces *interfaces)
{
    return interfaces->Get<IUnityGraphics>()->GetRenderer() == kUnityGfxRendererMetal;
}

id<MTLDevice> GetMetalDevice(IUnityInterfaces *interfaces, IUnityGraphicsMetal *graphicsMetal)
{
    if( !graphicsMetal )
    {
        graphicsMetal = interfaces->Get<IUnityGraphicsMetal>();
    }
    if( graphicsMetal )
    {
        return graphicsMetal->MetalDevice();
    }
    else
    {
        return nil;
    }
}

void API_SyphonMetal::processDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces)
{
    if(interfaces->Get<IUnityGraphics>()->GetRenderer() != kUnityGfxRendererMetal)
    {
        UnityNSLog(@"ERR: bad renderer. Expect bad behaviour after this");
        return;
    }
	if( type == kUnityGfxDeviceEventInitialize )
	{
        m_MetalGraphics = interfaces->Get<IUnityGraphicsMetal>();
        metalDevice = GetMetalDevice(interfaces, m_MetalGraphics);
	}
	else if( type == kUnityGfxDeviceEventShutdown )
	{
        [server shutdown];
        server = nil;
	}
}

void API_SyphonMetal::onRenderEvent(int eventID, void *data)
{
    if( server == nil )
    {
        UnityNSLog(@"ERR: No metal syphon server");
        return;
    }
    if( eventID == EVENTID_PUBLISH_FRAME )
    {
         [server publishNewFrame];
    }
    else if( eventID == EVENTID_RELEASE_SERVER )
    {
        [server shutdown];
        server = nil;
    }
}

#pragma mark - Metal Unity Syphon

static MTLPixelFormat metalPixelFormat = MTLPixelFormatBGRA8Unorm;

extern "C" void UNITY_INTERFACE_EXPORT *SyphonMetalCreateServer(const char* serverName, int width, int height, int gammaColorSpace)
{
    MTLPixelFormat pixelFormat = gammaColorSpace ? MTLPixelFormatBGRA8Unorm : MTLPixelFormatBGRA8Unorm_sRGB;
    server = [[SyphonMetal_UnityWrapper alloc] initWithServerName:[NSString stringWithUTF8String:serverName] textureSize:NSMakeSize(width, height) pixelFormat:pixelFormat device:metalDevice];
    return server.textureToSend;
}

#endif // #if SUPPORT_METAL
