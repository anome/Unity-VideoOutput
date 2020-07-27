// Based on KlakSpout - Spout video frame sharing plugin for Unity
// https://github.com/keijiro/KlakSpout
using UnityEngine;
using UnityEditor;

namespace UVO
{
    using Tools;
    [ExecuteInEditMode]
    //[AddComponentMenu(PluginCommon.PRODUCTION_PLUGIN_NAME + "/Spout Sender")]
    [RequireComponent(typeof(Camera))]
    public sealed class SpoutSender : ManagedMonobehaviour
    {
        System.IntPtr spoutSender;
        Texture2D sharedTexture;
        Material blitMaterial;

        #region Source settings

        [SerializeField] RenderTexture _sourceTexture;

        public RenderTexture sourceTexture
        {
            get { return _sourceTexture; }
            set { _sourceTexture = value; }
        }

        #endregion

        #region Format options

        [SerializeField] bool _alphaSupport;

        public bool alphaSupport
        {
            get { return _alphaSupport; }
            set { _alphaSupport = value; }
        }

        #endregion

        #region TextureFetcher
        private void CreateTextureGetterAndRegister()
        {
            MonoBehaviourManager.AddManagedComponent<GetTextureByOnRenderImage>(gameObject, (textureFetcher) =>
            {
                if (textureFetcher != null)
                {
                    textureFetcher.registerOnNewTexture(SendRenderTexture);
                    textureFetcher.registerOnTextureResize(OnTextureResize);
                }
                else
                {
                    Debug.LogError($"{PluginData.PRODUCTION_PLUGIN_NAME}: SPOUT SENDER: unexpected missing textureFetcher");
                }

            });
        }

        void OnTextureResize()
        {
            // Reset sender entirely
            GraphicEventManager.IssuePluginEvent((int)NativePluginBridge.SPOUT.GraphicEvent.Dispose, spoutSender);
            spoutSender = System.IntPtr.Zero;
            Utils.DestroyUnityObject(sharedTexture);
        }

        void SendRenderTexture(RenderTexture source)
        {
            LazyInitializeResources();
            LazyCreateSpoutSender(source.width, source.height);

            // Shared texture update
            if (sharedTexture != null)
            {
                // Blit shader parameters
                blitMaterial.SetFloat("_ClearAlpha", _alphaSupport ? 0 : 1);

                // We can't directly blit to the shared texture (as it lacks
                // render buffer functionality), so we temporarily allocate a
                // render texture as a middleman, blit the source to it, then
                // copy it to the shared texture using the CopyTexture API.
                var tempRT = RenderTexture.GetTemporary
                    (sharedTexture.width, sharedTexture.height);
                Graphics.Blit(source, tempRT, blitMaterial, 0);
                Graphics.CopyTexture(tempRT, sharedTexture);
                RenderTexture.ReleaseTemporary(tempRT);
            }
        }

        #endregion


        #region MonoBehaviour implementation

        void OnEnable()
        {
            NativePluginBridge.setApiState((int)NativePluginBridge.ApiStates.SendSpout);
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
            if (gameObject.GetComponent<GetTextureByOnRenderImage>() == null)
            {
                CreateTextureGetterAndRegister();
            }

            // Update the plugin internal state.
            if (spoutSender != System.IntPtr.Zero)
            {
                GraphicEventManager.IssuePluginEvent((int)NativePluginBridge.SPOUT.GraphicEvent.Update, spoutSender);
            }
        }

        private void OnDisable()
        {
            DestroyEverything();
        }

        private void OnDestroy()
        {
            DestroyEverything();
        }

        #endregion

        #region Internal

        private void LazyCreateSpoutSender(int textureWidth, int textureHeight)
        {
            // Plugin lazy initialization
            if (spoutSender == System.IntPtr.Zero)
            {
                spoutSender = NativePluginBridge.SPOUT.CreateSender(gameObject.name, textureWidth, textureHeight);
                if (spoutSender == System.IntPtr.Zero)
                {
                    // Spout may not be ready.
                    return;
                }
            }

            // Shared texture lazy initialization
            if (sharedTexture == null && spoutSender != System.IntPtr.Zero)
            {
                var texturePointer = NativePluginBridge.SPOUT.GetTexturePointer(spoutSender);
                if (texturePointer != System.IntPtr.Zero)
                {
                    sharedTexture = Texture2D.CreateExternalTexture(
                        NativePluginBridge.SPOUT.GetTextureWidth(spoutSender),
                        NativePluginBridge.SPOUT.GetTextureHeight(spoutSender),
                        TextureFormat.ARGB32, false, false, texturePointer
                    );
                    sharedTexture.hideFlags = HideFlags.DontSave;
                }
            }
        }

        private void LazyInitializeResources()
        {
            // Blit shader lazy initialization
            if (blitMaterial == null)
            {
                blitMaterial = new Material(Shader.Find("UVO/Standalone/SpoutBlit"));
                blitMaterial.hideFlags = HideFlags.DontSave;
            }
        }

        private void DestroyEverything()
        {
            if (spoutSender != System.IntPtr.Zero)
            {
                GraphicEventManager.IssuePluginEvent((int)NativePluginBridge.SPOUT.GraphicEvent.Dispose, spoutSender);
                spoutSender = System.IntPtr.Zero;
            }
            // Unregister delegates properly
            GetTextureByOnRenderImage getTextureComponent = gameObject.GetComponent<GetTextureByOnRenderImage>();
            if (getTextureComponent != null)
            {
                getTextureComponent.UnregisterDelegates();
            }
            // Don't bother to destroy managed components in case of game starting
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                MonoBehaviourManager.DestroyManagedComponent<GetTextureByOnRenderImage>(gameObject, null);
            }
            Utils.DestroyUnityObject(blitMaterial);
            Utils.DestroyUnityObject(sharedTexture);
        }

        #endregion
    }


}
