using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace UVO.Tools
{
    //[ExecuteInEditMode]
    // Execute in edit mode works but causes crash on play and on Unity quit
    //[AddComponentMenu(PluginCommon.PRODUCTION_PLUGIN_NAME + "/Internal/NativePlugin Logger")]
    public class NativePluginLogger : MonoBehaviour
    {

        public delegate void LoggerCallback(System.IntPtr value);

        const string _dllName = PluginData.NATIVE_DLL_NAME;
        [DllImport(_dllName)]
        private static extern void subscribeAsLoggerListener(LoggerCallback callback);

        [DllImport(_dllName)]
        private static extern void unsubscribeAsLoggerListener();


        [MonoPInvokeCallback(typeof(LoggerCallback))]
        private static void OnLog(System.IntPtr value)
        {
            Debug.Log(_dllName + ":" + Marshal.PtrToStringAnsi(value));
        }

        private bool isRegistered = false;


        void OnEnable()
        {
            if (!isRegistered)
            {
                subscribeAsLoggerListener(OnLog);
                isRegistered = true;
            }
        }

        void OnDisable()
        {
            isRegistered = false;
        }

        void OnDestroy()
        {
            unsubscribeAsLoggerListener();
            isRegistered = false;
        }


        void OnApplicationQuit()
        {
            unsubscribeAsLoggerListener();
            isRegistered = false;
        }
    }
}
