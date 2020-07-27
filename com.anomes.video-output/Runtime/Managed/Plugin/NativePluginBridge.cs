using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace UVO
{
    public class NativePluginBridge : ScriptableObject
    {

        public enum ApiStates
        {
            SendSyphonGLCore = 0,
            SendSyphonMetal = 1,
            SendNDI = 2,
            SendSpout = 3,
            Void = 100
        }

        [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "setPluginState")]
        public static extern IntPtr setApiState(int newState);

        [DllImport(PluginData.NATIVE_DLL_NAME)]
        internal static extern System.IntPtr GetRenderEventFunc();

        public class NDI
        {

            // PixelEncoding code definitions used in NDI
            public enum PixelEncoding : uint
            {
                UYVY = 0x59565955,
                UYVA = 0x41565955
            }

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "NDI_CreateSender")]
            public static extern IntPtr CreateSender(string name);

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "NDI_DestroySender")]
            public static extern void DestroySender(IntPtr sender);

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "NDI_SendFrame")]
            public static extern void SendFrame(IntPtr sender, IntPtr data, int width, int height, PixelEncoding fourCC);

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "NDI_SyncSender")]
            public static extern void SyncSender(IntPtr sender);
        }

        public class SYPHON
        {
            internal enum GraphicEvent
            {
                PublishFrame = 0x10000,
                StopServer = 0x20000
            }
            // Returns the texture pointer to fill
            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "SyphonMetalCreateServer")]
            internal static extern IntPtr CreateMetalServer(string serverName, int width, int height, int gammaColorSpace);

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "SyphonGLCoreCreateServer")]
            internal static extern void CreateGLCoreServer(string serverName, System.IntPtr textureId, int width, int height, bool linearToSrgb);

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "SyphonGLCoreShutdownServer")]
            internal static extern void ShutdownGLCoreServer();

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "SyphonGLCoreUpdateTextureId")]
            internal static extern void GLCoreUpdateTextureId(System.IntPtr textureId);

        }

        public class SPOUT
        {
            internal enum GraphicEvent
            {
                Update = 0,
                Dispose = 1
            }

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "Spout_CreateSender")]
            internal static extern System.IntPtr CreateSender(string name, int width, int height);

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "Spout_GetTexturePointer")]
            internal static extern System.IntPtr GetTexturePointer(System.IntPtr ptr);

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "Spout_GetTextureWidth")]
            internal static extern int GetTextureWidth(System.IntPtr ptr);

            [DllImport(PluginData.NATIVE_DLL_NAME, EntryPoint = "Spout_GetTextureHeight")]
            internal static extern int GetTextureHeight(System.IntPtr ptr);
        }
    }

}
