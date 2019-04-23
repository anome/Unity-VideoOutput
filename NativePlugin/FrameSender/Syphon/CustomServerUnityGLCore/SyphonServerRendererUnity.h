//
//  SyphonServerDrawingCoreProfile.m
//  Draws frame texture in the core profile mode
//
//  Originally created by Eduardo Roman on 1/26/15.
//  Modified by Keijiro Takahashi
//  Modified by mto-anomes

#import <Foundation/Foundation.h>
#import <OpenGL/gltypes.h>


@interface SyphonServerRendererUnity : NSObject
- (void)drawFrameTexture:(GLuint)texID surfaceSize:(NSSize)surfaceSize discardAlpha:(BOOL)discardAlpha;
@end
