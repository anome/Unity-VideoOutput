#pragma once

#include "SpoutData.h"

// Spout Sender handler class for Unity
// Not thread safe. The owner should care about it.
class SpoutSenderForUnity final
{

public:
	// Object attributes
	const std::string publicName;
	int textureWidth, textureHeight;

	// D3D11 objects
	ID3D11Resource* d3d11Resource;
	ID3D11ShaderResourceView* d3d11ResourceView;

	// Constructor
	SpoutSenderForUnity(const string& name, int width = -1, int height = -1)
		: publicName(name), textureWidth(width), textureHeight(height),
		d3d11Resource(nullptr), d3d11ResourceView(nullptr)
	{
		DEBUG_LOG("Sender created (%s)", publicName.c_str());
	}

	// Destructor
	~SpoutSenderForUnity()
	{
		releaseInternals();
		DEBUG_LOG("Sender disposed (%s)", publicName.c_str());
	}

	// Prohibit use of default constructor and copy operators
	SpoutSenderForUnity() = delete;
	SpoutSenderForUnity(SpoutSenderForUnity&) = delete;
	SpoutSenderForUnity& operator = (const SpoutSenderForUnity&) = delete;

	// Check if it's active.
	bool isActive() const
	{
		return d3d11Resource;
	}

	// Try activating the object. Returns false when failed.
	bool activate()
	{
		assert(d3d11Resource == nullptr && d3d11ResourceView == nullptr);
		return setupSender();
	}

	// Deactivate the object and release its internal resources.
	void deactivate()
	{
		releaseInternals();
	}

private:

	// Release internal objects.
	void releaseInternals()
	{
		auto& spoutData = SpoutData::get();

		// Senders should unregister their own name on destruction.
		if (d3d11Resource)
		{
			spoutData.spoutSenderFactory->ReleaseSenderName(publicName.c_str());
		}

		// Release D3D11 objects.
		if (d3d11Resource)
		{
			d3d11Resource->Release();
			d3d11Resource = nullptr;
		}

		if (d3d11ResourceView)
		{
			d3d11ResourceView->Release();
			d3d11ResourceView = nullptr;
		}
	}

	// Set up as a sender.
	bool setupSender()
	{
		auto& spoutData = SpoutData::get();

		// Avoid name duplication.
		{
			unsigned int width, height; HANDLE handle; DWORD format; // unused
			if (spoutData.spoutSenderFactory->CheckSender(publicName.c_str(), width, height, handle, format))
			{
				return false;
			}
		}

		// Currently we only support RGBA32.
		const auto format = DXGI_FORMAT_R8G8B8A8_UNORM;

		// Create a shared texture.
		ID3D11Texture2D* texture;
		HANDLE handle;
		auto success = spoutData.spoutFactory->CreateSharedDX11Texture(spoutData.D3D11Device, textureWidth, textureHeight, format, &texture, handle);

		if (!success)
		{
			DEBUG_LOG("CreateSharedDX11Texture failed (%s)", publicName.c_str());
			return false;
		}

		d3d11Resource = texture;

		// Create a resource view for the shared texture.
		auto res_d3d = spoutData.D3D11Device->CreateShaderResourceView(d3d11Resource, nullptr, &d3d11ResourceView);

		if (FAILED(res_d3d))
		{
			d3d11Resource->Release();
			d3d11Resource = nullptr;
			DEBUG_LOG("CreateShaderResourceView failed (%s:%x)", publicName.c_str(), res_d3d);
			return false;
		}

		// Create a Spout sender object for the shared texture.
		success = spoutData.spoutSenderFactory->CreateSender(publicName.c_str(), textureWidth, textureHeight, handle, format);

		if (!success)
		{
			d3d11ResourceView->Release();
			d3d11ResourceView = nullptr;
			d3d11Resource->Release();
			d3d11Resource = nullptr;
			DEBUG_LOG("CreateSender failed (%s)", publicName.c_str());
			return false;
		}

		DEBUG_LOG("Sender activated (%s)", publicName.c_str());
		return true;
	}
};
