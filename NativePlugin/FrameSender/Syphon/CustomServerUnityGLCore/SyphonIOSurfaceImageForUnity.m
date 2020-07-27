//#import "SyphonIOSurfaceImageForUnity.h"
//#import <IOSurface/IOSurface.h>
//#import <OpenGL/gl3.h>
//#import <OpenGL/CGLIOSurface.h>
//
//
//@implementation SyphonIOSurfaceImageForUnity
//{
//@protected
//    NSSize _size;
//@private
//    IOSurfaceRef _surface;
//    GLuint _texture;
//}
//
//- (id)initWithSurface:(IOSurfaceRef)surfaceRef
//{
//    if ( surfaceRef && (self = [super init]) )
//    {
//        _surface = (IOSurfaceRef)CFRetain(surfaceRef);
//        
//        _size.width = IOSurfaceGetWidth(surfaceRef);
//        _size.height = IOSurfaceGetHeight(surfaceRef);
//        
//        glGenTextures(1, &_texture);
//        glBindTexture(GL_TEXTURE_RECTANGLE, _texture);
//        
//        CGLError err = CGLTexImageIOSurface2D(CGLGetCurrentContext(), GL_TEXTURE_RECTANGLE, GL_SRGB8_ALPHA8, _size.width, _size.height, GL_BGRA, GL_UNSIGNED_INT_8_8_8_8_REV, _surface, 0);
//        
//        if( err != kCGLNoError )
//        {
////            SYPHONLOG(@"Error creating IOSurface texture: %s & %x", CGLErrorString(err), glGetError());
//            [self release];
//            return nil;
//        }
//    }
//    return self;
//}
//
//- (void)dealloc
//{
//    if( _texture )
//    {
//        glDeleteTextures(1, &_texture);
//    }
//    if( _surface )
//    {
//        CFRelease(_surface);
//    }
//    [super dealloc];
//}
//
//- (GLuint)textureName
//{
//    return _texture;
//}
//
//- (NSSize)textureSize
//{
//    return _size;
//}
//
//
//@end
