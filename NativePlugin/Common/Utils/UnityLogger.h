#import <Foundation/Foundation.h>


#define UnityNSLog(...) [UnityLogger log:[NSString stringWithFormat:__VA_ARGS__]];

@interface UnityLogger : NSObject
+ (void)log:(NSString*)text;
@end
