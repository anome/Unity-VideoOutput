#ifndef RegisteredRenderApis_h
#define RegisteredRenderApis_h

#include "PlatformBase.h"
#ifdef UNITY_OSX
#include "API_SyphonMetal.h"
#include "API_SyphonGLCore.h"
#endif
#ifdef UNITY_WIN
#include "API_SpoutD3D11.h"
#endif
#include "API_Void.h"
#include <map>
#define STATE_SEND_SYPHON_GLCORE 0
#define STATE_SEND_SYPHON_METAL 1
#define STATE_SEND_NDI 2
#define STATE_SEND_SPOUT 3
#define STATE_VOID 100

namespace RegisteredRenderApis {
    
    int appState = 100;
    
    std::map<int,UnityRenderAPI*> REGISTERED_RENDER_APIS = {
#ifdef UNITY_OSX
		{STATE_SEND_SYPHON_GLCORE, new API_SyphonGLCore()},
		{STATE_SEND_SYPHON_METAL, new API_SyphonMetal()},
        {STATE_SEND_SPOUT, new API_Void()},
#endif
#ifdef UNITY_WIN
		{STATE_SEND_SYPHON_GLCORE, new API_Void()},
		{STATE_SEND_SYPHON_METAL, new API_Void()},
        {STATE_SEND_SPOUT, new API_SpoutD3D11()},
#endif	
		{STATE_SEND_NDI, new API_Void()},
		{STATE_VOID, new API_Void()}
    };
    
    UnityRenderAPI* getApi()
    {
        return REGISTERED_RENDER_APIS.at(appState);
    }
    
}

#endif /* RegisteredRenderApis_h */
