#import "SyphonServerBase.h"
#import <Metal/MTLPixelFormat.h>
#import <Metal/MTLTexture.h>
#import <Metal/MTLCommandBuffer.h>

NS_ASSUME_NONNULL_BEGIN

@interface SyphonMetalServer : SyphonServerBase

- (id)initWithName:(NSString*)name device:(id<MTLDevice>)device options:(NSDictionary *)options;

// API Method 1
- (void)drawFrame:(void(^)(id<MTLTexture> texture,id<MTLCommandBuffer> commandBuffer))frameHandler size:(NSSize)size commandBuffer:(id<MTLCommandBuffer>)commandBuffer;

// API Method 2
- (void)publishFrameTexture:(id<MTLTexture>)textureToPublish imageRegion:(NSRect)region flip:(BOOL)flip;
- (void)publishFrameTexture:(id<MTLTexture>)textureToPublish imageRegion:(NSRect)region;
- (void)publishFrameTexture:(id<MTLTexture>)textureToPublish flip:(BOOL)flip;
- (void)publishFrameTexture:(id<MTLTexture>)textureToPublish;

- (id<MTLTexture>)newFrameImage;
- (void)stop;

@end

NS_ASSUME_NONNULL_END
