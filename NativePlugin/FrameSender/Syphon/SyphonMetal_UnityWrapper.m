#import "SyphonMetal_UnityWrapper.h"
#import "SyphonMetalServerWithExtras.h"
#import <Metal/Metal.h>
#import <Metal/MTLDevice.h>
#import <Metal/MTLTexture.h>
#import <Metal/MTLCommandBuffer.h>
#include "UnityLogger.h"

@implementation SyphonMetal_UnityWrapper
{
    SyphonMetalServerWithExtras *syphonServer;
    NSString *serverName;
    id <MTLDevice> device;
    BOOL srgb;
    id <MTLCommandQueue> commandQueue;
    OSSpinLock threadLocker;
}

- (id)initWithServerName:(NSString*)theServerName textureSize:(NSSize)size device:(id <MTLDevice>)theDevice srgb:(BOOL)theSrgb;
{
    if( self = [super init] )
    {
        device = theDevice;
        srgb = theSrgb;
        commandQueue = [device newCommandQueue];
        syphonServer = nil;
        serverName = theServerName;
        [self setupTextureToSend:size];
    }
    return self;
}

- (void)setupTextureToSend:(NSSize)size
{
    MTLPixelFormat colorPixelFormat = srgb ? MTLPixelFormatBGRA8Unorm_sRGB : MTLPixelFormatBGRA8Unorm;
    MTLTextureDescriptor *desc = [MTLTextureDescriptor texture2DDescriptorWithPixelFormat:colorPixelFormat
                                                                                    width:size.width
                                                                                   height:size.height
                                                                                mipmapped:NO];
    _textureToSend = [device newTextureWithDescriptor:desc];
}

- (void)lazyInitialise
{
    if( syphonServer == nil )
    {
        NSDictionary *options = @{@"SyphonServerOptionIsPrivate":@"NO"};
        syphonServer = [[SyphonMetalServerWithExtras alloc] initWithName:serverName device:device options:options srgb:srgb];
    }
}

- (void)shutdown
{
    OSSpinLockLock(&threadLocker);
    UnityNSLog(@"MetalSyphonServer shutdown");
    [syphonServer stop];
    syphonServer = nil;
    OSSpinLockUnlock(&threadLocker);
}

- (void)publishNewFrame
{
    [self lazyInitialise];
    [syphonServer publishFrameTexture:_textureToSend];
}

@end
