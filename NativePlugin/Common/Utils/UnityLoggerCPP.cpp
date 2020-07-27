#include "UnityLoggerCPP.h"
#include "IUnityInterface.h"


#pragma mark - Unity C Bridge

typedef void(*LoggerCallback)(const char* message);

static LoggerCallback unityLoggerCallback = nullptr;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API subscribeAsLoggerListener(LoggerCallback unityLoggerCb)
{
    unityLoggerCallback = unityLoggerCb;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API unsubscribeAsLoggerListener()
{
    unityLoggerCallback = nullptr;
}

void sendLogToUnity(const char* value)
{
    if( unityLoggerCallback != nullptr )
    {
        unityLoggerCallback(value);
    }
}
