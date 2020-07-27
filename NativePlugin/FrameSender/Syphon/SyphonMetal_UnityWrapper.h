#import <Foundation/Foundation.h>
#import <Metal/MTLPixelFormat.h>

@protocol MTLDevice;
@protocol MTLTexture;

@interface SyphonMetal_UnityWrapper : NSObject

@property (readonly) id <MTLTexture> textureToSend;

- (id)initWithServerName:(NSString*)serverName textureSize:(NSSize)size device:(id <MTLDevice>)device srgb:(BOOL)srgb;
- (void)publishNewFrame;
- (void)shutdown;

@end
