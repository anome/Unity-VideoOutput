#pragma once

#include <cstdio>
#include <cassert>
#include <d3d11.h>
#include <SpoutDirectX.h>
#include <SpoutSenderNames.h>

// Debug logging macro
#if defined(_DEBUG)
#define DEBUG_LOG(fmt, ...) std::printf("KlakSpout: "#fmt"\n", __VA_ARGS__)
#else
#define DEBUG_LOG(fmt, ...)
#endif


	// Singleton class used for storing global variables
class SpoutData final
{
public:

	ID3D11Device* D3D11Device;
	std::unique_ptr<spoutDirectX> spoutFactory;
	std::unique_ptr<spoutSenderNames> spoutSenderFactory;

	static SpoutData& get()
	{
		static SpoutData instance;
		return instance;
	}

	bool isReady() const
	{
		return D3D11Device;
	}
};
