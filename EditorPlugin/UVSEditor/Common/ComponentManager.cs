using UnityEngine;

namespace UVS.Common
{
    public class ComponentManager : ScriptableObject
    {
        public static void AddManagedComponent<T>(GameObject gameObject, System.Action<T> callback, bool hideComponent = true) where T : MonoBehaviour
        {
            if (gameObject.GetComponent<T>() == null)
            {
                T component = gameObject.AddComponent<T>();
                if (hideComponent)
                {
                    component.hideFlags |= HideFlags.HideInInspector;
                }
                component.hideFlags |= HideFlags.NotEditable;
                callback?.Invoke(component);
            }
            // If component is already here, make it comply
            else
            {
                var component = gameObject.GetComponent<T>();
                if (hideComponent)
                {
                    component.hideFlags |= HideFlags.HideInInspector;
                }
                component.hideFlags |= HideFlags.NotEditable;
                if (!component.enabled)
                {
                    component.enabled = true;
                }
                callback?.Invoke(component);
            }
        }

        public static void EnableManagedComponent<T>(GameObject gameObject) where T : MonoBehaviour
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                component.enabled = true;
            }
        }

        public static void DisableManagedComponent<T>(GameObject gameObject) where T : MonoBehaviour
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                component.enabled = false;
            }
        }

        public static void DestroyManagedComponent<T>(GameObject gameObject, System.Action callback) where T : MonoBehaviour
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                Utils.ExecuteMethodSafelyInUnityContext(() =>
                {
                    Utils.DestroyUnityObject(component);
                    callback?.Invoke();
                });
            }
            else
            {
                callback?.Invoke();
            }

        }
    }
}