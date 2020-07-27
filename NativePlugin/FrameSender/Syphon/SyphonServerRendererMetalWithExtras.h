#import <Foundation/Foundation.h>
#import <Metal/MTLPixelFormat.h>
#import <Metal/Metal.h>

// Actually there is zero difference with the real one but this file is non-accessible by default from the Syphon Framework

@protocol MTLDevice;
@protocol MTLTexture;
@protocol MTLCommandQueue;
@protocol MTLCommandBuffer;


@interface SyphonServerRendererMetalWithExtras : NSObject

- (instancetype) initWithDevice:(id<MTLDevice>)device colorPixelFormat:(MTLPixelFormat)colorPixelFormat msaaSampleCount:(NSInteger)sampleCount;
- (void)renderFromTexture:(id<MTLTexture>)offScreenTexture inTexture:(id<MTLTexture>)texture region:(NSRect)region onCommandBuffer:(id<MTLCommandBuffer>)commandBuffer flip:(BOOL)flip;

+ (NSInteger)safeMsaaSampleCountForDevice:(id<MTLDevice>)device unsafeSampleCount:(NSInteger)unsafeSampleCount verbose:(BOOL)verbose;

@end
