using UnityEngine;

namespace UVS
{
    [ExecuteAlways]
    //[AddComponentMenu(PluginCommon.PRODUCTION_PLUGIN_NAME + "/Internal/GetTextureOnRenderImage")]
    [RequireComponent(typeof(Camera))]
    public class GetTextureByOnRenderImage : MonoBehaviour
    {
        private bool renderGameView = true;
        public delegate void OnNewTextureEvent(RenderTexture texture);
        public OnNewTextureEvent onNewTextureDelegate = null;
        public delegate void OnTextureResizeEvent();
        public OnTextureResizeEvent onTextureResizeDelegate = null;

        RenderTexture _renderTexture;

        // Lazy load of camera 
        // Don't use this, use the getter
        private Camera _componentCamera = null;

        private Camera componentCamera
        {
            get
            {
                if (_componentCamera == null)
                {
                    _componentCamera = GetComponent<Camera>();
                }
                return _componentCamera;
            }
        }

        public void registerOnNewTexture(OnNewTextureEvent delegateToRegister)
        {
            onNewTextureDelegate = delegateToRegister;
        }

        public void registerOnTextureResize(OnTextureResizeEvent delegateToRegister)
        {
            onTextureResizeDelegate = delegateToRegister;
        }

        public void UnregisterDelegates()
        {
            onNewTextureDelegate = null;
            onTextureResizeDelegate = null;
        }

        private void Awake()
        {
            Application.runInBackground = true;
        }

        void OnEnable()
        {
            setupTextureToSend();
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (renderGameView)
            {
                Graphics.Blit(source, destination);
            }
            if (onNewTextureDelegate != null)
            {
                onNewTextureDelegate.Invoke(source);
            }
        }

        private void OnDestroy()
        {
            destroyEverything();
        }

        private void OnApplicationQuit()
        {
            destroyEverything();
        }

        private void Update()
        {
            // Handle resize
            if (_renderTexture.width != Screen.width || _renderTexture.height != Screen.height)
            {
                // If not handled by delegate, do nothing
                if (onTextureResizeDelegate != null)
                {
                    setupTextureToSend();
                    onTextureResizeDelegate.Invoke();
                }
            }
        }

        void OnGUI()
        {
            if (renderGameView)
            {
                var rect = new Rect(0, 0, Screen.width, Screen.height);
                GUI.DrawTexture(rect, _renderTexture, ScaleMode.ScaleToFit, false);
            }
        }

        void setupTextureToSend()
        {
            // Detach the render rexture temporarily because changes on
            // camera target texture is not allowed.
            componentCamera.targetTexture = null;
            if (_renderTexture != null)
            {
                _renderTexture.Release();
            }
            // Create a render target.
            _renderTexture = new RenderTexture(Screen.width, Screen.height, 0)
            {
                hideFlags = HideFlags.DontSave,
                antiAliasing = QualitySettings.antiAliasing
            };

            componentCamera.targetTexture = _renderTexture;
            componentCamera.ResetAspect();
        }

        private void destroyEverything()
        {
            UnregisterDelegates();
            // Release the camera.
            componentCamera.targetTexture = null;
            componentCamera.ResetAspect();
            if (_renderTexture)
            {
                DestroyImmediate(_renderTexture);
            }
            _renderTexture = null;
        }

    }

}