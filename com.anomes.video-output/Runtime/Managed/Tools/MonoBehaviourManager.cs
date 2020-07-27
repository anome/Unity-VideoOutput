using UnityEngine;

namespace UVO.Tools
{
    public class MonoBehaviourManager : ScriptableObject
    {
        public static void AddManagedComponent<T>(GameObject gameObject, System.Action<T> callback, bool hideComponent = true) where T : ManagedMonobehaviour
        {
            if (gameObject.GetComponent<T>() == null)
            {
                T component = gameObject.AddComponent<T>();
                if (hideComponent)
                {
                    component.hideFlags |= HideFlags.HideInInspector | HideFlags.HideInHierarchy | HideFlags.HideAndDontSave;
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
                component.hideFlags = HideFlags.NotEditable;
                if (!component.enabled)
                {
                    component.enabled = true;
                }
                callback?.Invoke(component);
            }
        }

        public static void EnableManagedComponent<T>(GameObject gameObject) where T : ManagedMonobehaviour
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                component.enabled = true;
            }
        }

        public static void DisableManagedComponent<T>(GameObject gameObject) where T : ManagedMonobehaviour
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                component.enabled = false;
            }
        }

        public static void WipeCleanManagedComponents(GameObject gameObject)
        {
            var managedComponents = gameObject.GetComponents(typeof(ManagedMonobehaviour));
            foreach(ManagedMonobehaviour managedComponent in managedComponents)
            {
                managedComponent.enabled = false;
            }
            foreach (ManagedMonobehaviour managedComponent in managedComponents)
            {
                Utils.ExecuteMethodSafelyInUnityContext(() =>
                {
                    Utils.DestroyUnityObject(managedComponent);
                });
            }
        }

        public static void DestroyManagedComponent<T>(GameObject gameObject, System.Action callback) where T : ManagedMonobehaviour
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