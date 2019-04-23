using UnityEngine;
using UnityEditor;

namespace UVS
{
    using Common;

    [HelpURL("https://github.com/anome/Unity-plugin")]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu(PluginData.PRODUCTION_PLUGIN_NAME + "/VideoSender")]
    [RequireComponent(typeof(Camera))]
    public class VideoSender : MonoBehaviour
    {

        private SenderTechniques.SenderTechniqueEnum? _previousSenderType = null;
        [SerializeField] [SenderTechniquesInList()] public SenderTechniques.SenderTechniqueEnum _senderTechnique;

        private void Awake()
        {
            Application.runInBackground = true;
            CreateManagedComponent();
        }

        private void handleCreatedManagedComponent(MonoBehaviour component)
        {
            if (component != null)
            {
                UpdateManagedComponent();
            }
            else
            {
                Debug.LogError("Unexpected missing subComponent");
            }
        }

        private void CreateManagedComponent()
        {
            switch (_senderTechnique)
            {
                case SenderTechniques.SenderTechniqueEnum.OpenGLSyphon:
                    ComponentManager.AddManagedComponent<OpenGLCoreSyphon>(gameObject, handleCreatedManagedComponent);
                    break;
                case SenderTechniques.SenderTechniqueEnum.MetalSyphon:
                    ComponentManager.AddManagedComponent<MetalSyphon>(gameObject, handleCreatedManagedComponent);
                    break;
                case SenderTechniques.SenderTechniqueEnum.Spout:
                    ComponentManager.AddManagedComponent<SpoutSender>(gameObject, handleCreatedManagedComponent);
                    break;
                case SenderTechniques.SenderTechniqueEnum.NDI:
                    ComponentManager.AddManagedComponent<NdiSender>(gameObject, handleCreatedManagedComponent);
                    break;

                default:
                    Debug.LogError("Unexpected value of sender type");
                    break;
            }
            // Avoid side effects of components being enabled while this component is disabled
            if (!enabled)
            {
                ComponentManager.DisableManagedComponent<OpenGLCoreSyphon>(gameObject);
                ComponentManager.DisableManagedComponent<MetalSyphon>(gameObject);
                ComponentManager.DisableManagedComponent<NdiSender>(gameObject);
                ComponentManager.DisableManagedComponent<SpoutSender>(gameObject);
            }
        }

        private void UpdateManagedComponent()
        {
            switch (_senderTechnique)
            {
                case SenderTechniques.SenderTechniqueEnum.OpenGLSyphon:
                    {
                        OpenGLCoreSyphon sender = gameObject.GetComponent<OpenGLCoreSyphon>();
                    }
                    break;
                case SenderTechniques.SenderTechniqueEnum.MetalSyphon:
                    {
                        MetalSyphon sender = gameObject.GetComponent<MetalSyphon>();
                    }
                    break;
                case SenderTechniques.SenderTechniqueEnum.Spout:
                    {
                        SpoutSender sender = gameObject.GetComponent<SpoutSender>();
                    }
                    break;
                case SenderTechniques.SenderTechniqueEnum.NDI:
                    {
                        NdiSender sender = gameObject.GetComponent<NdiSender>();
                    }
                    break;

                default:
                    Debug.LogError("Unexpected value of sender technique");
                    break;
            }
        }

        // called when a field value has been changed by user
        private void OnValidate()
        {
            // Lazy init
            if (_previousSenderType == null)
            {
                _previousSenderType = _senderTechnique;
            }
            if (_previousSenderType != _senderTechnique)
            {
                _previousSenderType = _senderTechnique;
                DestroyManagedComponents(() =>
                {
                    CreateManagedComponent();
                });
            }
            else
            {
                UpdateManagedComponent();
            }
        }

        void OnEnable()
        {
            ComponentManager.EnableManagedComponent<OpenGLCoreSyphon>(gameObject);
            ComponentManager.EnableManagedComponent<MetalSyphon>(gameObject);
            ComponentManager.EnableManagedComponent<NdiSender>(gameObject);
            ComponentManager.EnableManagedComponent<SpoutSender>(gameObject);
        }

        void OnDisable()
        {
            ComponentManager.DisableManagedComponent<OpenGLCoreSyphon>(gameObject);
            ComponentManager.DisableManagedComponent<MetalSyphon>(gameObject);
            ComponentManager.DisableManagedComponent<NdiSender>(gameObject);
            ComponentManager.DisableManagedComponent<SpoutSender>(gameObject);
        }

        void OnDestroy()
        {
            // Don't bother to destroy managed components in case of game starting
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            else
            {
                // OnDestroy method doesnt really like callbacks or passing the gameObject
                // So manual destroy of everything without callbacks
                ComponentManager.DestroyManagedComponent<OpenGLCoreSyphon>(gameObject, null);
                ComponentManager.DestroyManagedComponent<MetalSyphon>(gameObject, null);
                ComponentManager.DestroyManagedComponent<NdiSender>(gameObject, null);
                ComponentManager.DestroyManagedComponent<SpoutSender>(gameObject, null);
            }

        }

        private void DestroyManagedComponents(System.Action callback)
        {
            // TODO: switch from callback hell to async
            ComponentManager.DestroyManagedComponent<OpenGLCoreSyphon>(gameObject, () =>
            {
                ComponentManager.DestroyManagedComponent<MetalSyphon>(gameObject, () =>
                {
                    ComponentManager.DestroyManagedComponent<NdiSender>(gameObject, () =>
                    {
                        ComponentManager.DestroyManagedComponent<SpoutSender>(gameObject, () =>
                        {
                            callback?.Invoke();
                        });
                    });
                });
            });
        }

    }

}
