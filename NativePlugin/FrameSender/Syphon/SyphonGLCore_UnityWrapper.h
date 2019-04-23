#import <Foundation/Foundation.h>

@interface SyphonGLCore_UnityWrapper : NSObject

- (void)setupNewServerWithName:(NSString*)serverName texture:(int)textureId width:(int)width height:(int)height linearToSRGB:(BOOL)linearToSRGB;
- (void)setupNewServerWithTexture:(int)textureId width:(int)width height:(int)height linearToSRGB:(BOOL)linearToSRGB;
- (void)updateTextureId:(int)newTextureId;
- (void)shutdown;
- (void)publishFrame;

@end
