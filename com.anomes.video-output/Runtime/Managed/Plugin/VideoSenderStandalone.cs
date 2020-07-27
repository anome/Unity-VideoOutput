using UnityEngine;
using UnityEditor;

#if UVO_PIPELINE_BUILT_IN
namespace UVO
{
    using Tools;

    [HelpURL(PluginData.HELP_URL)]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu(PluginData.PRODUCTION_PLUGIN_NAME + "/" + PluginData.PRODUCTION_PLUGIN_NAME + " (Standalone)")]
    [RequireComponent(typeof(Camera))]
    public class VideoSenderStandalone : MonoBehaviour
    {
        [SerializeField, HideInInspector] private SendTechnique _previousSenderType = SendTechnique.SyphonSpout;
        [SerializeField] public SendTechnique _outputType = SendTechnique.SyphonSpout;

        private void Awake()
        {
            Application.runInBackground = true;
            CreateManagedComponent();
        }

        // called when a field value has been changed by user
        private void OnValidate()
        {
            // Avoid side effects
            if (!enabled)
            {
                return;
            }
            if (_previousSenderType != _outputType)
            {
                _previousSenderType = _outputType;
                DestroyManagedComponents(() =>
                {
                    CreateManagedComponent();
                });
            }
            // Make sure everything still exists
            else
            {
                CreateManagedComponent();
            }

        }

        void OnEnable()
        {
            CreateManagedComponent();
        }

        void OnDisable()
        {
             MonoBehaviourManager.WipeCleanManagedComponents(gameObject);
        }

        void OnDestroy()
        {
            MonoBehaviourManager.WipeCleanManagedComponents(gameObject);
        }


        private void handleCreatedManagedComponent(ManagedMonobehaviour component)
        {
           // Deprecated
        }

        private void CreateManagedComponent()
        {
            // Avoid side effects of managed components being enabled while this component is disabled
            if (!enabled)
            {
                return;
            }

            switch (_outputType)
            {
                case SendTechnique.SyphonSpout:
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
#if UVO_API_METAL
                    MonoBehaviourManager.AddManagedComponent<MetalSyphon>(gameObject, handleCreatedManagedComponent);
#else
                     Debug.LogError(PluginData.PRODUCTION_PLUGIN_NAME + ": This graphic API is not supported. Please switch your API to Metal in Project Settings -> Player");
#endif
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#if UVO_API_DIRECTX11
                    MonoBehaviourManager.AddManagedComponent<SpoutSender>(gameObject, handleCreatedManagedComponent);
#else
                    Debug.LogError(PluginData.PRODUCTION_PLUGIN_NAME + ": This graphic API is not supported. Please switch your API to DirectX11 in Project Settings -> Player");
#endif
#endif
                
                    break;
                case SendTechnique.NDI:
#if UVO_API_DIRECTX11 || UVO_API_METAL
                    MonoBehaviourManager.AddManagedComponent<NdiSender>(gameObject, handleCreatedManagedComponent);
#else
                    string switchToPlatform = "";
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    switchToPlatform = "Metal";
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    switchToPlatform = "DirectX D11";
#endif
                    Debug.LogError(PluginData.PRODUCTION_PLUGIN_NAME + ": This graphic API is not supported. Please switch your API to " + switchToPlatform + " in Project Settings -> Player");
#endif
                    break;

                default:
                    Debug.LogError("Unexpected value of output type");
                    break;
            }

        }

        private void DestroyManagedComponents(System.Action callback)
        {
            MonoBehaviourManager.WipeCleanManagedComponents(gameObject);
            callback.Invoke();
        }
    }
}
#endif