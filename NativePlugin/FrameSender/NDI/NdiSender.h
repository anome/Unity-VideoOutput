#include "IUnityInterface.h"
#pragma once

/// NDI imports
#ifdef _WIN32
#include <SDKDDKVer.h>
#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#endif

#include <stdlib.h> // Add support of NULL for NDI headers
#include <Processing.NDI.Lib.h>

#ifdef __cplusplus

class NdiSender
{
public:
    NdiSender(const char* name);
    ~NdiSender();
    void sendFrame(void* data, int width, int height, uint32_t fourCC);
    void synchronize();
    
private:
    
    NDIlib_send_instance_t instance_;
};

#endif
