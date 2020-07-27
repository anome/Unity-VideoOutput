using UnityEngine;
using UnityEngine.Rendering;

namespace UVO.Tools
{
    public class GraphicEventManager : ScriptableObject
    {
        static CommandBuffer _commandBuffer;
        public static void IssuePluginEvent(int pluginEvent, System.IntPtr extraData)
        {
            if (_commandBuffer == null)
            {
                _commandBuffer = new CommandBuffer();
            }

            if (extraData == System.IntPtr.Zero)
            {
                _commandBuffer.IssuePluginEvent(NativePluginBridge.GetRenderEventFunc(), pluginEvent);
            }
            else
            {
                _commandBuffer.IssuePluginEventAndData(NativePluginBridge.GetRenderEventFunc(), pluginEvent, extraData);
            }

            Graphics.ExecuteCommandBuffer(_commandBuffer);
            _commandBuffer.Clear();
        }

        public static void OldSchoolIssuePluginEvent(int pluginEvent)
        {
            GL.IssuePluginEvent(NativePluginBridge.GetRenderEventFunc(), pluginEvent);
        }
    }
}