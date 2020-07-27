#import "SyphonMetalServerWithExtras.h"
#import <Metal/MTLCommandQueue.h>
#import "SyphonServerRendererMetalWithExtras.h"
//#import "SyphonPrivate.h"

@implementation SyphonMetalServerWithExtras
{
    id<MTLTexture> _surfaceTexture;
    id<MTLDevice> _device;
    id<MTLCommandQueue> _commandQueue;
    SyphonServerRendererMetalWithExtras *_renderer;
    NSInteger _msaaSampleCount;
}

+ (NSInteger)integerValueForKey:(NSString *)key fromOptions:(NSDictionary *)options
{
    NSNumber *number = [options objectForKey:key];
    if ([number respondsToSelector:@selector(unsignedIntValue)])
    {
        return [number unsignedIntValue];
    }
    return 0;
}


#pragma mark - Lifecycle

- (id)initWithName:(NSString *)name device:(id<MTLDevice>)theDevice options:(NSDictionary *)options srgb:(BOOL)srgb;
{
    self = [super initWithName:name options:options];
    if( self )
    {
        _device = [theDevice retain];
        _commandQueue = [_device newCommandQueue];
        _surfaceTexture = nil;
//        NSInteger unsafeMsaaSampleCount = [[self class] integerValueForKey:SyphonServerOptionAntialiasSampleCount fromOptions:options];
        _msaaSampleCount = 1;//[SyphonServerRendererMetalWithExtras safeMsaaSampleCountForDevice:_device unsafeSampleCount:unsafeMsaaSampleCount verbose:YES];
#warning MTO: MSAA is disabled, it needs more testing
        MTLPixelFormat pixelFormat = srgb ? MTLPixelFormatBGRA8Unorm_sRGB : MTLPixelFormatBGRA8Unorm;
        _renderer = [[SyphonServerRendererMetalWithExtras alloc] initWithDevice:theDevice colorPixelFormat:pixelFormat msaaSampleCount:1];
    }
    return self;
}

- (void)lazySetupTextureForSize:(NSSize)size
{
    BOOL hasSizeChanged = !NSEqualSizes(CGSizeMake(_surfaceTexture.width, _surfaceTexture.height), size);
    if (hasSizeChanged)
    {
        [_surfaceTexture release];
        _surfaceTexture = nil;
    }
    if(_surfaceTexture == nil)
    {
        MTLTextureDescriptor *descriptor = [MTLTextureDescriptor texture2DDescriptorWithPixelFormat:MTLPixelFormatBGRA8Unorm
                                                                                              width:size.width
                                                                                             height:size.height
                                                                                          mipmapped:NO];
        descriptor.usage = MTLTextureUsageRenderTarget | MTLTextureUsageShaderRead;
        IOSurfaceRef surface = [super copySurfaceForWidth:size.width height:size.height options:nil];
        if (surface)
        {
            _surfaceTexture = [_device newTextureWithDescriptor:descriptor iosurface:surface plane:0];
            _surfaceTexture.label = @"Syphon Surface Texture";
            CFRelease(surface);
        }
    }
}

- (id<MTLTexture>)prepareToDrawFrameOfSize:(NSSize)size
{
    [self lazySetupTextureForSize:size];
    return _surfaceTexture;
}

- (void)stop
{
    [_surfaceTexture release];
    _surfaceTexture = nil;
    [_device release];
    _device = nil;
    [_commandQueue release];
    _commandQueue = nil;
    [_renderer release];
    _renderer = nil;
    [super stop];
}


#pragma mark - Public API

- (id<MTLTexture>)newFrameImage
{
    return [_surfaceTexture retain];
}

- (void)drawFrame:(void(^)(id<MTLTexture> frame,id<MTLCommandBuffer> commandBuffer))frameHandler size:(NSSize)size commandBuffer:(id<MTLCommandBuffer>)commandBuffer
{
    id<MTLTexture> texture = [self prepareToDrawFrameOfSize:size];
    if( texture != nil )
    {
        frameHandler(texture, commandBuffer);
        [commandBuffer addCompletedHandler:^(id<MTLCommandBuffer> _Nonnull commandBuffer) {
            [self publish];
        }];
    }
}

- (void)publishFrameTexture:(id<MTLTexture>)textureToPublish
{
    NSRect region = NSMakeRect(0, 0, textureToPublish.width, textureToPublish.height);
    [self publishFrameTexture:textureToPublish imageRegion:region flip:NO];
}

- (void)publishFrameTexture:(id<MTLTexture>)textureToPublish flip:(BOOL)flip
{
    NSRect region = NSMakeRect(0, 0, textureToPublish.width, textureToPublish.height);
    [self publishFrameTexture:textureToPublish imageRegion:region flip:flip];
}

- (void)publishFrameTexture:(id<MTLTexture>)textureToPublish imageRegion:(NSRect)region
{
    [self publishFrameTexture:textureToPublish imageRegion:region flip:NO];
}

- (void)publishFrameTexture:(id<MTLTexture>)textureToPublish imageRegion:(NSRect)region flip:(BOOL)flip
{
    [self lazySetupTextureForSize:region.size];
    id<MTLCommandBuffer> commandBuffer = [_commandQueue commandBuffer];
    commandBuffer.label = @"Syphon Server commandBuffer";
    
    // When possible, use faster blit
    if( !flip && _msaaSampleCount == 1 && textureToPublish.pixelFormat == _surfaceTexture.pixelFormat
       && textureToPublish.sampleCount == _surfaceTexture.sampleCount
       && !textureToPublish.framebufferOnly)
    {
        id<MTLBlitCommandEncoder> blitCommandEncoder = [commandBuffer blitCommandEncoder];
        blitCommandEncoder.label = @"Syphon Server Optimised Blit commandEncoder";
        [blitCommandEncoder copyFromTexture:textureToPublish
                                sourceSlice:0
                                sourceLevel:0
                               sourceOrigin:MTLOriginMake(region.origin.x, region.origin.y, 0)
                                 sourceSize:MTLSizeMake(region.size.width, region.size.height, 1)
                                  toTexture:_surfaceTexture
                           destinationSlice:0
                           destinationLevel:0
                          destinationOrigin:MTLOriginMake(0, 0, 0)];

        [blitCommandEncoder endEncoding];
    }
    // otherwise, re-draw the frame
    else
    {
        [_renderer renderFromTexture:textureToPublish inTexture:_surfaceTexture region:region onCommandBuffer:commandBuffer flip:flip];
    }
    
    [commandBuffer addCompletedHandler:^(id<MTLCommandBuffer> _Nonnull commandBuffer) {
        [self publish];
    }];
    [commandBuffer commit];
}



@end
