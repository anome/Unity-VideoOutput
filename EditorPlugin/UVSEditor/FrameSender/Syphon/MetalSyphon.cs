using UnityEngine;
using UnityEditor;

namespace UVS
{
    using Common;
    [ExecuteAlways]
    //[AddComponentMenu(PluginCommon.PRODUCTION_PLUGIN_NAME + "/MetalSyphon")]
    [RequireComponent(typeof(Camera))]
    public sealed class MetalSyphon : MonoBehaviour
    {
        #region METAL variables
        Texture metalServerTexture;
        Shader metalBlitShader;
        Material metalBlitMaterial;

        #endregion


        #region TextureFetcher
        void ReceiveCameraTexture(RenderTexture sourceTexture)
        {
            if (!Utils.isRendererMetal())
            {
                return;
            }
            lazyInit();
            // Capture the camera render.
            var temp = RenderTexture.GetTemporary(
                metalServerTexture.width, metalServerTexture.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Default
            );
            // Alpha support set to false by default
            Graphics.Blit(sourceTexture, temp, metalBlitMaterial, 0);
            Graphics.CopyTexture(temp, metalServerTexture);
            RenderTexture.ReleaseTemporary(temp);
            GraphicEventManager.IssuePluginEvent((int)SyphonGraphicEvent.PublishFrame, System.IntPtr.Zero);
        }

        void OnTextureResize()
        {
            GraphicEventManager.IssuePluginEvent((int)SyphonGraphicEvent.StopServer, System.IntPtr.Zero);
            metalServerTexture = null;
        }

        private void CreateTextureGetterAndRegister()
        {
            ComponentManager.AddManagedComponent<GetTextureByOnRenderImage>(gameObject, (textureFetcher) =>
            {
                if (textureFetcher != null)
                {
                    textureFetcher.registerOnNewTexture(ReceiveCameraTexture);
                    textureFetcher.registerOnTextureResize(OnTextureResize);
                }
                else
                {
                    Debug.LogError("METAL SYPHON: unexpected missing textureFetcher");
                }

            });
        }

        #endregion

        #region MonoBehaviour implementation

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void OnEnable()
        {
            if (!Utils.isRendererMetal())
            {
                return;
            }
            NativePluginBridge.setApiState((int)NativePluginBridge.ApiStates.SendSyphonMetal);
            metalBlitShader = Shader.Find("UVS/Hidden/MetalBlit");
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
        }
        private void OnDisable()
        {
            destroyEverything();
        }

        private void OnDestroy()
        {
            destroyEverything();
        }

        private void OnApplicationQuit()
        {
            destroyEverything();
        }

        #endregion

        #region Internal 

        void lazyInit()
        {
            // Lazy initialization for the server plugin.
            if (metalServerTexture == null)
            {
                var componentCamera = GetComponent<Camera>();
                int width = componentCamera.pixelWidth;
                int height = componentCamera.pixelHeight;
                int gammaColorSpace = 0;
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                {
                    gammaColorSpace = 1;
                }
                // Create the server texture as an external 2D texture.
                metalServerTexture = Texture2D.CreateExternalTexture(
                    width, height, TextureFormat.RGBA32, false, false,
                    NativePluginBridge.SYPHON.CreateMetalServer(gameObject.name, width, height, gammaColorSpace));
            }

            // Lazy initialization for the internal objects.
            if (metalBlitMaterial == null)
            {
                metalBlitMaterial = new Material(metalBlitShader);
                metalBlitMaterial.hideFlags = HideFlags.DontSave;
            }
        }

        private void destroyEverything()
        {
            // Unregister delegates properly
            GetTextureByOnRenderImage getTextureComponent = gameObject.GetComponent<GetTextureByOnRenderImage>();
            if (getTextureComponent != null)
            {
                getTextureComponent.UnregisterDelegates();
            }

            // Don't bother to destroy managed components in case of game starting
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                ComponentManager.DestroyManagedComponent<GetTextureByOnRenderImage>(gameObject, null);
            }

            GraphicEventManager.IssuePluginEvent((int)SyphonGraphicEvent.StopServer, System.IntPtr.Zero);

            if (metalServerTexture != null)
            {
                if (Application.isPlaying)
                {
                    DestroyImmediate(metalServerTexture);
                }
                else
                {
                    DestroyImmediate(metalServerTexture);
                }
            }
            metalServerTexture = null;

            // Dispose the internal objects.
            if (metalBlitMaterial != null)
            {
                if (Application.isPlaying)
                {
                    DestroyImmediate(metalBlitMaterial);
                }
                else
                {
                    DestroyImmediate(metalBlitMaterial);
                }
            }
        }

        #endregion
    }

}
