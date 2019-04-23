#import <Foundation/Foundation.h>
#import <Metal/MTLPixelFormat.h>

@protocol MTLDevice;

@interface SyphonMetalClient : NSObject
{
@private
    id _connectionManager;
    NSUInteger _lastFrameID;
    void (^_handler)(id);
    int32_t _status;
    int32_t _lock;
    id<MTLTexture> _frame;
    int32_t _frameValid;
    NSDictionary *_serverDescription;
    id<MTLDevice> device;
    MTLPixelFormat colorPixelFormat;
}

@property (readonly) BOOL isValid;
@property (readonly) NSDictionary *serverDescription;
@property (readonly) BOOL hasNewFrame;

/*!
 Returns a new client instance for the described server. You should check the isValid property after initialization to ensure a connection was made to the server.
 @param description Typically acquired from the shared SyphonServerDirectory, or one of Syphon's notifications.
 @param device The Metal Device used in your app
 #param pixelFormat the color pixel format of your view
 @param options Currently ignored. May be nil.
 @param handler A block which is invoked when a new frame becomes available. handler may be nil. This block may be invoked on a thread other than that on which the client was created.
 @returns A newly initialized SyphonClient object, or nil if a client could not be created.
 */

- (id)initWithServerDescription:(NSDictionary *)description device:(id<MTLDevice>)device colorPixelFormat:(MTLPixelFormat)colorPixelFormat options:(NSDictionary *)options
                newFrameHandler:(void (^)(SyphonMetalClient *client))handler;
- (id<MTLTexture>)newFrameImage;
- (void)stop;

@end
