// based on KlakNDI - NDI plugin for Unity
// https://github.com/keijiro/KlakNDI

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System;
using UnityEditor;


namespace UVO
{
    using Tools;
    [ExecuteInEditMode]
    //[AddComponentMenu(PluginCommon.PRODUCTION_PLUGIN_NAME + "/NDI Sender")]
    [RequireComponent(typeof(Camera))]
    public sealed class NdiSender : ManagedMonobehaviour
    {
        IntPtr ndiSender = IntPtr.Zero;

        #region TextureFetcher
        void OnReadbackFrame(GetTextureByAsyncReadback.Frame frame)
        {
            // Lazy initialization of the plugin sender instance.
            if (ndiSender == IntPtr.Zero)
            {
                ndiSender = NativePluginBridge.NDI.CreateSender(gameObject.name);
            }
            // Feed the frame data to the sender. It encodes/sends the
            // frame asynchronously.
            unsafe
            {
                NativePluginBridge.NDI.SendFrame(
                    ndiSender, (IntPtr)frame.readback.GetData<Byte>().GetUnsafeReadOnlyPtr(),
                    frame.width, frame.height, frame.alpha ? NativePluginBridge.NDI.PixelEncoding.UYVA : NativePluginBridge.NDI.PixelEncoding.UYVY
                );
            }
        }

        void OnReadbackSync()
        {
            if (ndiSender != IntPtr.Zero)
            {
                NativePluginBridge.NDI.SyncSender(ndiSender);
            }
        }

        private void CreateTextureGetterAndRegister()
        {
            MonoBehaviourManager.AddManagedComponent<GetTextureByAsyncReadback>(gameObject, (textureFetcher) =>
            {
                if (textureFetcher != null)
                {
                    textureFetcher.RegisterOnNewFrameEvent(OnReadbackFrame);
                    textureFetcher.RegisterOnSyncFrameEvent(OnReadbackSync);
                }
                else
                {
                    Debug.LogError($"{PluginData.PRODUCTION_PLUGIN_NAME}: NDI SENDER: unexpected missing textureFetcher");
                }

            });
        }

        #endregion


        #region Monobehaviour implementation
        private void Awake()
        {
            Application.runInBackground = true;
        }

        void OnEnable()
        {
            if (!shouldRunScript())
            {
                return;
            }
            NativePluginBridge.setApiState((int)NativePluginBridge.ApiStates.SendNDI);
            CreateTextureGetterAndRegister();
        }

        private void Update()
        {
            // Quickfix : lazy check that GetTexture component is still here.
            // Needed check to handle edit mode properly
            // When switching between play/edit mode, Unity calls onDestroy and OnEnable multiple times, which causes multiple add/remove of getTexture component
            // Unity Lifecycle doesnt wait for method completions, so if this component is destroyed/added multiple times,
            // add/removes of GetTextureComponent doesnt always fire in the right order
            // which can cause the managed component to be absent when needed.
            if (gameObject.GetComponent<GetTextureByAsyncReadback>() == null)
            {
                CreateTextureGetterAndRegister();
            }
        }

        private void OnDestroy()
        {
            destroyEverything();
        }

        #endregion

        #region Internal

        private bool shouldRunScript()
        {
            return (Utils.isRendererDirect3D11() || Utils.isRendererDirect3D12() || Utils.isRendererMetal());
        }

        private void destroyEverything()
        {
            // Make sure to unregister
            GetTextureByAsyncReadback getTextureComponent = gameObject.GetComponent<GetTextureByAsyncReadback>();
            if (getTextureComponent != null)
            {
                getTextureComponent.UnregisterDelegates();
            }

            // Don't bother to destroy managed components in case of game starting
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                MonoBehaviourManager.DestroyManagedComponent<GetTextureByAsyncReadback>(gameObject, null);
            }

            if (ndiSender != IntPtr.Zero)
            {
                NativePluginBridge.NDI.DestroySender(ndiSender);
                ndiSender = IntPtr.Zero;
            }
        }

        #endregion
    }

}
