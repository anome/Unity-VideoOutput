using UnityEngine;
using UnityEditor;

namespace UVO.Tools
{
    public class Utils : ScriptableObject
    {
        static public bool isRendererMetal()
        {
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal;
        }

        static public bool isRendererDirect3D11()
        {
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11;
        }

        static public bool isRendererDirect3D12()
        {
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12;
        }

        static public bool isRendererOpenGLCore()
        {
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore;
        }

        static public bool isRendererD3X11()
        {
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11;
        }

        static public void DestroyUnityObject(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        // Use with caution. Only use this when outside of a MonoBehavior lifecycle call otherwise it may cause crashes
        static public void ExecuteMethodSafelyInUnityContext(System.Action methodToRun)
        {
            if (Application.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                methodToRun();
            }
            else
            {
                // delay to make sure it doesnt interfere with Unity GUI drawing loops
                EditorApplication.delayCall += () =>
                {
                    methodToRun();
                };
            }
        }
    }
}
