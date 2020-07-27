#include "NdiSender.h"


NdiSender::NdiSender(const char* name)
{
    NDIlib_send_create_t send(name, nullptr, false);
    instance_ = NDIlib_send_create(&send);
}

NdiSender::~NdiSender()
{
    NDIlib_send_destroy(instance_);
}

void NdiSender::sendFrame(void* data, int width, int height, uint32_t fourCC)
{
    static NDIlib_video_frame_v2_t frame;
    
    frame.xres = width;
    frame.yres = height;
    frame.FourCC = static_cast<NDIlib_FourCC_type_e>(fourCC);
    frame.frame_rate_N = 60;
    frame.frame_rate_D = 1;
    frame.frame_format_type = NDIlib_frame_format_type_progressive;
    frame.p_data = static_cast<uint8_t*>(data);
    frame.line_stride_in_bytes = width * 2;
    
    NDIlib_send_send_video_async_v2(instance_, &frame);
}

void NdiSender::synchronize()
{
    NDIlib_send_send_video_async_v2(instance_, nullptr);
}

extern "C" NdiSender UNITY_INTERFACE_EXPORT *NDI_CreateSender(const char* name)
{
    return new NdiSender(name);
}

extern "C" void UNITY_INTERFACE_EXPORT NDI_DestroySender(NdiSender* sender)
{
    delete sender;
}

extern "C" void UNITY_INTERFACE_EXPORT NDI_SendFrame(NdiSender* sender, void* data, int width, int height, uint32_t fourCC)
{
    sender->sendFrame(data, width, height, fourCC);
}

extern "C" void UNITY_INTERFACE_EXPORT NDI_SyncSender(NdiSender* sender)
{
    sender->synchronize();
}
