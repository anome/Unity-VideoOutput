#import "SyphonGLCore_UnityWrapper.h"
#import "UnityLogger.h"
#import "SyphonServerForUnity.h"

@interface SyphonData : NSObject

@property (retain, nonatomic) SyphonServerForUnity *syphonServer;
@property (copy, nonatomic) NSString *serverName;
@property (assign) NSRect textureRect;
@property (assign) int textureId;
@property (assign) BOOL linearToSRGB;
@property (assign) BOOL discardAlpha;

@end

@implementation SyphonData

- (void)dealloc
{
    [_syphonServer release];
    [_serverName release];
    [super dealloc];
}

@end


@implementation SyphonGLCore_UnityWrapper
{
    OSSpinLock threadLocker;
    SyphonData *syphonData;
}

- (instancetype)init
{
    self = [super init];
    if( self )
    {
        threadLocker = OS_SPINLOCK_INIT;
        syphonData = nil;
    }
    return self;
}

- (void)setupNewServerWithName:(NSString*)serverName texture:(int)textureId width:(int)width height:(int)height linearToSRGB:(BOOL)linearToSRGB
{
    OSSpinLockLock(&threadLocker);
    
    // Lazy loading
    if( !syphonData )
    {
        syphonData = [SyphonData new];
    }
    
    // Status update
    syphonData.serverName = serverName;
    syphonData.textureId = textureId;
    syphonData.textureRect = NSMakeRect(0, 0, width, height);
    syphonData.linearToSRGB = linearToSRGB;
    syphonData.discardAlpha = NO;
    OSSpinLockUnlock(&threadLocker);
}

- (void)setupNewServerWithTexture:(int)textureId width:(int)width height:(int)height linearToSRGB:(BOOL)linearToSRGB
{
    [self setupNewServerWithName:@"UnityGLSyphon" texture:textureId width:width height:height linearToSRGB:linearToSRGB];
}

- (void)updateTextureId:(int)newTextureId
{
    if( syphonData )
    {
        OSSpinLockLock(&threadLocker);
        syphonData.textureId = newTextureId;
        OSSpinLockUnlock(&threadLocker);
    }
}

- (void)shutdown
{
    OSSpinLockLock(&threadLocker);
    if( syphonData )
    {
        [syphonData.syphonServer stop];
        [syphonData release];
        syphonData = nil;
    }
    UnityNSLog(@"CoreSyphonServer shutdown");
    OSSpinLockUnlock(&threadLocker);
}

- (void)publishFrame
{
    [self lazyInitialize];
    OSSpinLockLock(&threadLocker);
    if( syphonData.syphonServer.hasClients )
    {
        [syphonData.syphonServer publishFrameTexture:syphonData.textureId size:syphonData.textureRect.size];
    }
    OSSpinLockUnlock(&threadLocker);
}

- (void)lazyInitialize
{
    if( !syphonData.syphonServer )
    {
        syphonData.syphonServer = [[SyphonServerForUnity alloc] initWithName:syphonData.serverName];
        syphonData.syphonServer.linearToSRGB = syphonData.linearToSRGB;
        syphonData.syphonServer.discardsAlpha = syphonData.discardAlpha;
        UnityNSLog(@"CoreSyphonServer created a new server");
    }
}

@end
