#ifndef UnityLoggerCPP_h
#define UnityLoggerCPP_h

#define UnityLog(...) sendLogToUnity(__VA_ARGS__)

void sendLogToUnity(const char* value);

#endif /* UnityLoggerCPP_h */
