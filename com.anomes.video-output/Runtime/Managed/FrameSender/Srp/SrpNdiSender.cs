
#if UVO_ASMDEF_HAS_CORE_SRP

using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace UVO.SRP
{
    class NdiSender
    {
        const string ndiConversionShaderName = "UVO/SRP/NdiConversion";
        Material ndiConvertMaterial;
        bool alphaSupport = false;
        RenderTexture ndiCompliantTexture;
        int lastFrameCount = -1;
        const int queueSize = 4;
        IntPtr nativeSender = IntPtr.Zero;
        string outputName = "UnityVideoOutput";

        public struct NDIFrame
        {
            public int width, height;
            public bool alpha;
            public AsyncGPUReadbackRequest readback;
        }
        Queue<NDIFrame> _frameQueue = new Queue<NDIFrame>(queueSize);

        public void Setup(string outputName = "UnityVideoOutput")
        {
            this.outputName = outputName;
            NativePluginBridge.setApiState((int)NativePluginBridge.ApiStates.SendNDI);
        }

        public void SendTextureByCameraBridge(CommandBuffer cmd, RenderTargetIdentifier textureToSend, int width, int height)
        {
            // Sync event
            OnReadbackSync();
            // Process the readback queue before enqueuing.
            ProcessQueue();
            // Push the source image into the readback queue.
            QueueFrame(textureToSend, cmd, width, height);
        }

        public void Cleanup()
        {
            CoreUtils.Destroy(ndiConvertMaterial);
            if (ndiCompliantTexture != null)
            {
                RenderTexture.ReleaseTemporary(ndiCompliantTexture);
                ndiCompliantTexture = null;
            }
            NativePluginBridge.NDI.DestroySender(nativeSender);
            nativeSender = IntPtr.Zero;
        }


        private void OnReadbackFrame(NDIFrame frame)
        {
            // Lazy initialization of the plugin sender instance.
            if (nativeSender == IntPtr.Zero)
            {
                nativeSender = NativePluginBridge.NDI.CreateSender(outputName);
            }
            // Feed the frame data to the sender. It encodes/sends the
            // frame asynchronously.
            unsafe
            {
                NativePluginBridge.NDI.SendFrame(
                    nativeSender, (IntPtr)frame.readback.GetData<Byte>().GetUnsafeReadOnlyPtr(),
                   frame.width, frame.height, frame.alpha ? NativePluginBridge.NDI.PixelEncoding.UYVA : NativePluginBridge.NDI.PixelEncoding.UYVY
                );
            }
        }

        private void OnReadbackSync()
        {
            if (nativeSender != IntPtr.Zero)
            {
                NativePluginBridge.NDI.SyncSender(nativeSender);
            }
        }

        private void ProcessQueue()
        {
            while (_frameQueue.Count > 0)
            {
                var frame = _frameQueue.Peek();

                // Edit mode: Wait for readback completion every frame.
                if (!Application.isPlaying)
                {
                    frame.readback.WaitForCompletion();
                }

                // Skip error frames.
                if (frame.readback.hasError)
                {
                    //Debug.LogWarning(PluginData.PRODUCTION_PLUGIN_NAME + ": GPU readback error was detected.");
                    _frameQueue.Dequeue();
                    continue;
                }

                // Break when found a frame that hasn't been read back yet.
                if (!frame.readback.done)
                {
                    break;
                }

                // Feed the frame data to the sender. It encodes/sends the
                // frame asynchronously.
                OnReadbackFrame(frame);

                // Done. Remove the frame from the queue.
                _frameQueue.Dequeue();
            }

            // Edit mode: We're not sure when the readback buffer will be
            // disposed, so let's synchronize with the sender to prevent it
            // from accessing disposed memory area.
            // TODO: is this still useful?
            if (!Application.isPlaying)
            {
                OnReadbackSync();
            }
        }

        private void QueueFrame(RenderTargetIdentifier source, CommandBuffer cmd, int width, int height)
        {
            if (queueSize - 1 < _frameQueue.Count)
            {
                Debug.LogWarning($"{PluginData.PRODUCTION_PLUGIN_NAME}: Readback Request queue is full. drop frame");
                return;
            }

            // On Editor, this may be called multiple times in a single frame.
            // To avoid wasting memory (actually this can cause an out-of-memory
            // exception), check the frame count and reject duplicated requests.
            if (lastFrameCount == Time.frameCount)
            {
                return;
            }
            lastFrameCount = Time.frameCount;

            // Return the old render texture to the pool.
            if (ndiCompliantTexture != null)
            {
                RenderTexture.ReleaseTemporary(ndiCompliantTexture);
            }

            int trueWidth = width;
            int trueHeight = height;
            ndiCompliantTexture = RenderTexture.GetTemporary(
                trueWidth / 2, (alphaSupport ? 3 : 2) * trueHeight / 2, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear
            );

            // Lazy initialization of the conversion shader.
            if (ndiConvertMaterial == null)
            {
                ndiConvertMaterial = new Material(Shader.Find(ndiConversionShaderName));
                ndiConvertMaterial.hideFlags = HideFlags.DontSave;
            }

            var linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
            if (linear)
            {
                ndiConvertMaterial.SetInt("_isLinear", 1);
            }
            else
            {
                ndiConvertMaterial.SetInt("_isLinear", 0);
            }

            // Alpha support set to false by default
            cmd.Blit(source, ndiCompliantTexture, ndiConvertMaterial, 0);

            // Request readback.
            _frameQueue.Enqueue(new NDIFrame
            {
                width = trueWidth,
                height = trueHeight,
                alpha = alphaSupport,
                readback = AsyncGPUReadback.Request(ndiCompliantTexture)
            });
        }
    }
}

#endif