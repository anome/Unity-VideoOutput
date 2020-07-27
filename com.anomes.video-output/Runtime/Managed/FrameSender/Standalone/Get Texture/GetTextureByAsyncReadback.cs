// Based on KlakNDI - NDI plugin for Unity
// https://github.com/keijiro/KlakNDI
// WARNING: This code is not fully agnostic of the sender. It is still tied to NDI Sender (see shader)
// We should make it generic, if some day we have the utility to use it with a second sender

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace UVO
{
    [ExecuteInEditMode]
    //[AddComponentMenu(PluginCommon.PRODUCTION_PLUGIN_NAME + "/Internal/GetTexture AsyncReadback")]
    [RequireComponent(typeof(Camera))]
    public sealed class GetTextureByAsyncReadback : ManagedMonobehaviour
    {
        #region Delegates
        public delegate void OnNewFrameEvent(Frame frame);
        OnNewFrameEvent OnNewFrameEventDelegate = null;

        public delegate void OnSyncFrameEvent();
        OnSyncFrameEvent OnSyncFrameEventDelegate = null;

        public void RegisterOnNewFrameEvent(OnNewFrameEvent delegateToRegister)
        {
            OnNewFrameEventDelegate = delegateToRegister;
        }

        public void RegisterOnSyncFrameEvent(OnSyncFrameEvent delegateToRegister)
        {
            OnSyncFrameEventDelegate = delegateToRegister;
        }

        public void UnregisterDelegates()
        {
            OnSyncFrameEventDelegate = null;
            OnNewFrameEventDelegate = null;
        }

        #endregion


        #region Format option

        bool _alphaSupport = false;

        #endregion

        #region Private members

        Material ndiConversionShader;
        RenderTexture ndiCompliantTexture;
        int lastFrameCount = -1;
        const int queueSize = 4;
        #endregion

        #region Frame readback queue

        public struct Frame
        {
            public int width, height;
            public bool alpha;
            public AsyncGPUReadbackRequest readback;
        }

        Queue<Frame> _frameQueue = new Queue<Frame>(queueSize);

        void QueueFrame(RenderTexture source)
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

            // Allocate a new render texture.
            ndiCompliantTexture = RenderTexture.GetTemporary(
                source.width / 2, (_alphaSupport ? 3 : 2) * source.height / 2, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear
            );

            // Lazy initialization of the conversion shader.
            if (ndiConversionShader == null)
            {
                ndiConversionShader = new Material(Shader.Find("UVO/Standalone/NdiConversion"));
                ndiConversionShader.hideFlags = HideFlags.DontSave;
            }

            var linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
            if (linear)
            {
                ndiConversionShader.SetInt("_isLinear", 1);
            }
            else
            {
                ndiConversionShader.SetInt("_isLinear", 0);
            }

            // Apply the conversion shader.
            Graphics.Blit(source, ndiCompliantTexture, ndiConversionShader, _alphaSupport ? 1 : 0);

            // Request readback.
            _frameQueue.Enqueue(new Frame
            {
                width = source.width,
                height = source.height,
                alpha = _alphaSupport,
                readback = AsyncGPUReadback.Request(ndiCompliantTexture)
            });
        }

        void ProcessQueue()
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
                    //Debug.LogWarning($"{PluginData.PRODUCTION_PLUGIN_NAME}: GPU readback error was detected.");
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
                if (OnNewFrameEventDelegate != null)
                {
                    OnNewFrameEventDelegate.Invoke(frame);
                }

                // Done. Remove the frame from the queue.
                _frameQueue.Dequeue();
            }

            // Edit mode: We're not sure when the readback buffer will be
            // disposed, so let's synchronize with the sender to prevent it
            // from accessing disposed memory area.
            if (!Application.isPlaying)
            {
                if (OnSyncFrameEventDelegate != null)
                {
                    OnSyncFrameEventDelegate.Invoke();
                }
            }
        }


        // Delayed update callback
        // The readback queue works on the assumption that the next update will
        // come in a short period of time. In editor, this assumption is not
        // guaranteed -- updates can be discontinuous. The last update before
        // a pause won't be shown immediately, and it will be delayed until the
        // next user action. This can mess up editor interactivity.
        // To solve this problem, we use EditorApplication.delayUpdate to send
        // discontinuous updates in a synchronous fashion.

        bool _delayUpdateForEditorAdded;

        void DelayedUpdateForEditor()
        {
            _delayUpdateForEditorAdded = false;
            // Process the readback queue to send the last update.
            ProcessQueue();
        }


        #endregion


        #region MonoBehaviour implementation

        IEnumerator Start()
        {
            // Only run the sync coroutine in the play mode.
            if (!Application.isPlaying)
            {
                yield break;
            }

            // Synchronize with the async sender at the end of every frame.
            for (var wait = new WaitForEndOfFrame(); ;)
            {
                yield return wait;
                if (OnSyncFrameEventDelegate != null)
                {
                    OnSyncFrameEventDelegate.Invoke();
                }
            }
        }

        void OnDestroy()
        {
            if (ndiConversionShader != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(ndiConversionShader);
                }
                else
                {
                    DestroyImmediate(ndiConversionShader);
                }
                ndiConversionShader = null;
            }
        }

        void OnDisable()
        {
            if (ndiCompliantTexture != null)
            {
                RenderTexture.ReleaseTemporary(ndiCompliantTexture);
                ndiCompliantTexture = null;
            }

            if (!Application.isPlaying)
            {
                _delayUpdateForEditorAdded = false;
            }

        }

        void Update()
        {
            // Edit mode: Register the delayed update callback.
            if (!Application.isPlaying && !_delayUpdateForEditorAdded)
            {
                EditorApplication.delayCall += DelayedUpdateForEditor;
                _delayUpdateForEditorAdded = true;
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (source != null)
            {
                // Process the readback queue before enqueuing.
                ProcessQueue();

                // Push the source image into the readback queue.
                QueueFrame(source);
            }

            // Dumb blit
            Graphics.Blit(source, destination);
        }

        #endregion
    }

}