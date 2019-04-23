#import <Foundation/Foundation.h>
#import <Metal/MTLPixelFormat.h>

@protocol MTLDevice;
@protocol MTLTexture;

@interface SyphonMetal_UnityWrapper : NSObject

@property (readonly) id <MTLTexture> textureToSend;

- (id)initWithServerName:(NSString*)serverName textureSize:(NSSize)size pixelFormat:(MTLPixelFormat)format device:(id <MTLDevice>)device;
- (id)initWithTextureSize:(NSSize)size pixelFormat:(MTLPixelFormat)format device:(id <MTLDevice>)device;
- (void)publishNewFrame;
- (void)shutdown;

@end
