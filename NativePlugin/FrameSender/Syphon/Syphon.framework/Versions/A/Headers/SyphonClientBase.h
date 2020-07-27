//
//  SyphonIOSurfaceClient.h
//  Syphon
//
//  Created by Tom Butterworth on 26/09/2019.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface SyphonClientBase : NSObject
/*!
 @param description

 */
- (instancetype)initWithServerDescription:(NSDictionary<NSString *, id> *)description options:(nullable NSDictionary<NSString *, id> *)options newFrameHandler:(nullable void (^)(id client))handler;

/*!
 Returns a dictionary with a description of the server the client is attached to. See SyphonServerDirectory for the keys this dictionary contains
*/
@property (readonly) NSDictionary *serverDescription;

/*!
 A client is valid if it has a working connection to a server. Once this returns NO, the SyphonClient will not yield any further frames.
 */
@property (readonly) BOOL isValid;

/*!
 Stops the client from receiving any further frames from the server. Use of this method is optional and releasing all references to the client has the same effect.

 This method may perform work in the OpenGL context. As with any other OpenGL calls, you must ensure no other threads use those contexts during calls to this method.
 */
- (void)stop;

/*!
 Returns YES if the server has output a new frame since the last time newFrameImage was called for this client, NO otherwise.
*/
@property (readonly) BOOL hasNewFrame;
@end

@interface SyphonClientBase (SyphonSubclassing)
/*!
 Subclasses use this method to acquire an IOSurface representing the current output from the server. Subclasses may consider the returned value valid until
 the next call to -invalidateFrame.

 @returns An IOSurface representing the live output from the server. YOU ARE RESPONSIBLE FOR RELEASING THIS OBJECT using CFRelease() when you
 are finished with it.
 */
- (IOSurfaceRef)newSurface;

/*!
 Subclasses override this method to invalidate their output when the server's surface backing changes. Do not call this method directly.
 */
- (void)invalidateFrame;
@end
NS_ASSUME_NONNULL_END
