#import "UnityLogger.h"
#import "UnityLoggerCPP.h"

#pragma mark - Objc impl.

@implementation UnityLogger

+ (void)log:(NSString*)text
{
    UnityLog([text UTF8String]);
}

@end
