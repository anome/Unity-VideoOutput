#import "SyphonMetal_UnityWrapper.h"
#import <Syphon/SyphonMetalServer.h>
#import <Metal/Metal.h>
#import <Metal/MTLDevice.h>
#import <Metal/MTLTexture.h>
#import <Metal/MTLCommandBuffer.h>
#include "UnityLogger.h"

@implementation SyphonMetal_UnityWrapper
{
    SyphonMetalServer *syphonServer;
    NSString *serverName;
    id <MTLDevice> device;
    MTLPixelFormat colorPixelFormat;
    id <MTLCommandQueue> commandQueue;
    OSSpinLock threadLocker;
}

- (id)initWithServerName:(NSString*)theServerName textureSize:(NSSize)size pixelFormat:(MTLPixelFormat)format device:(id <MTLDevice>)theDevice
{
    if( self = [super init] )
    {
        device = theDevice;
        colorPixelFormat = format;
        commandQueue = [device newCommandQueue];
        syphonServer = nil;
        serverName = theServerName;
        [self setupTextureToSend:size];
    }
    return self;
}

- (id)initWithTextureSize:(NSSize)size pixelFormat:(MTLPixelFormat)format device:(id <MTLDevice>)theDevice
{
    return [self initWithServerName:@"UnityMetalSyphon" textureSize:size pixelFormat:format device:theDevice];
}

- (void)setupTextureToSend:(NSSize)size
{
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
        syphonServer = [[SyphonMetalServer alloc] initWithName:serverName metalDevice:device pixelFormat:colorPixelFormat options:options];
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
