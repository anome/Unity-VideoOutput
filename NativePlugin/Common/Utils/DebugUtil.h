
#import <Foundation/Foundation.h>

#define DEBUG_FILE @"DEBUG.txt"

void logToDebugFile(NSString *text)
{
    NSFileHandle *fileHandle = [NSFileHandle fileHandleForWritingAtPath:DEBUG_FILE];
    if( fileHandle )
    {
        
        [fileHandle seekToEndOfFile];
        [fileHandle writeData:[text dataUsingEncoding:NSUTF8StringEncoding]];
        [fileHandle closeFile];
    }
    else
    {
        
        [text writeToFile:DEBUG_FILE
               atomically:NO
                 encoding:NSStringEncodingConversionAllowLossy
                    error:nil];
    }
}
