using UnityEngine;
using UnityEditor;

namespace UVO
{
    using Tools;
    [ExecuteInEditMode]
    //[AddComponentMenu(PluginCommon.PRODUCTION_PLUGIN_NAME + "/OpenGLCoreSyphon")]
    [RequireComponent(typeof(Camera))]
    public sealed class OpenGLCoreSyphon : ManagedMonobehaviour
    {
        private bool serverCreated = false;

        #region Gamma shader

        [SerializeField] Shader gammaShader;
        Material _gammaMaterial;

        // Material with lazy shader initialization
        Material gammaMaterial
        {
            get
            {
                if (_gammaMaterial == null)
                {
                    _gammaMaterial = new Material(gammaShader);
                    _gammaMaterial.hideFlags = HideFlags.DontSave;
                }
                return _gammaMaterial;
            }
        }
        #endregion

        #region TextureFetcher
        void ReceiveCameraTexture(RenderTexture sourceTexture)
        {
            if (!Utils.isRendererOpenGLCore())
            {
                return;
            }
            lazyInit();
            if (!serverCreated)
            {
                var linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
                // Publish the content of the source buffer.
                NativePluginBridge.SYPHON.CreateGLCoreServer(gameObject.name, sourceTexture.GetNativeTexturePtr(), sourceTexture.width, sourceTexture.height, linear);
                serverCreated = true;
            }
            NativePluginBridge.SYPHON.GLCoreUpdateTextureId(sourceTexture.GetNativeTexturePtr());
            GraphicEventManager.OldSchoolIssuePluginEvent((int)NativePluginBridge.SYPHON.GraphicEvent.PublishFrame);
        }

        void OnTextureResize()
        {
            // Will trigger a new server creation
            NativePluginBridge.SYPHON.ShutdownGLCoreServer();
            serverCreated = false;
        }

        private void CreateTextureGetterAndRegister()
        {
            MonoBehaviourManager.AddManagedComponent<GetTextureByOnRenderImage>(gameObject, (textureFetcher) =>
            {
                if (textureFetcher != null)
                {
                    textureFetcher.registerOnNewTexture(ReceiveCameraTexture);
                    textureFetcher.registerOnTextureResize(OnTextureResize);
                }
                else
                {
                    Debug.LogError($"{PluginData.PRODUCTION_PLUGIN_NAME}: GL SYPHON: unexpected missing textureFetcher");
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
            if (!Utils.isRendererOpenGLCore())
            {
                return;
            }
            NativePluginBridge.setApiState((int)NativePluginBridge.ApiStates.SendSyphonGLCore);
            gammaShader = Shader.Find("UVO/Standalone/CoreGammaCorrect");
            lazyInit();
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
            // Lazy initialization for the internal objects.
            if (_gammaMaterial == null)
            {
                _gammaMaterial = new Material(gammaShader);
                _gammaMaterial.hideFlags = HideFlags.DontSave;
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

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                MonoBehaviourManager.DestroyManagedComponent<GetTextureByOnRenderImage>(gameObject, null);
            }

            NativePluginBridge.SYPHON.ShutdownGLCoreServer();
            serverCreated = false;
            // Release the camera.
            var componentCamera = GetComponent<Camera>();
            componentCamera.targetTexture = null;
            componentCamera.ResetAspect();

            // Release temporary assets.
            if (_gammaMaterial)
            {
                DestroyImmediate(_gammaMaterial);
            }
            _gammaMaterial = null;
        }

        #endregion

    }
}
