#if UVO_ASMDEF_HAS_CORE_SRP
using UnityEngine;
using UnityEngine.Rendering;

namespace UVO.SRP
{
    class SpoutSender
    {
        System.IntPtr nativeSender = System.IntPtr.Zero;
        Texture2D sharedTexture;
        const string blitShaderName = "UVO/SRP/SpoutBlit";
        Material blitMaterial;
        string outputName  = "UnityVideoOutput";

        public void Setup(string outputName="UnityVideoOutput")
        {
            // create the color material
            if (Shader.Find(blitShaderName) != null)
            {
                blitMaterial = new Material(Shader.Find(blitShaderName));
            }
            else
            {
                Debug.LogError($"{PluginData.PRODUCTION_PLUGIN_NAME}: Cannot start Spout Sender. Unable to find shader '{blitShaderName}'.");
                return;
            }
            this.outputName = outputName;
            NativePluginBridge.setApiState((int)NativePluginBridge.ApiStates.SendSpout);
        }
        

        private void LazyCreateSpoutSender(CommandBuffer cmd, int textureWidth, int textureHeight)
        {
            // Check for resizes
            if (nativeSender != System.IntPtr.Zero && sharedTexture != null)
            {
                var senderWidth = NativePluginBridge.SPOUT.GetTextureWidth(nativeSender);
                var senderHeight = NativePluginBridge.SPOUT.GetTextureHeight(nativeSender);
                if (senderWidth != textureWidth || senderHeight != textureHeight)
                {
                    // kill sender entirely
                    System.IntPtr renderEventFc = UVO.NativePluginBridge.GetRenderEventFunc();
                    cmd.IssuePluginEventAndData(renderEventFc, (int)NativePluginBridge.SPOUT.GraphicEvent.Dispose, nativeSender);
                    nativeSender = System.IntPtr.Zero;
                    CoreUtils.Destroy(sharedTexture);
                    sharedTexture = null;
                    return;
                }
            }

            if (nativeSender == System.IntPtr.Zero)
            {
                nativeSender = NativePluginBridge.SPOUT.CreateSender(outputName, textureWidth, textureHeight);
                if (nativeSender == System.IntPtr.Zero)
                {
                    Debug.LogWarning($"{PluginData.PRODUCTION_PLUGIN_NAME}: Spout not ready...");
                    // Spout may not be ready.
                    return;
                } 
            }

            // Shared texture lazy init
            if (sharedTexture == null && nativeSender != System.IntPtr.Zero)
            {
                var texturePointer = NativePluginBridge.SPOUT.GetTexturePointer(nativeSender);
                if (texturePointer != System.IntPtr.Zero)
                {
                    var width = NativePluginBridge.SPOUT.GetTextureWidth(nativeSender);
                    var height = NativePluginBridge.SPOUT.GetTextureHeight(nativeSender);
                    sharedTexture = Texture2D.CreateExternalTexture(
                        width, height,
                        TextureFormat.ARGB32, false, false, texturePointer
                    );
                    sharedTexture.hideFlags = HideFlags.DontSave;
                }
            }
        }

        public void SendTextureByCameraBridge(CommandBuffer cmd, RenderTargetIdentifier textureToSend, int width, int height)
        {
            LazyCreateSpoutSender(cmd, width, height);
            if (blitMaterial == null)
            {
                Debug.LogError($"{PluginData.PRODUCTION_PLUGIN_NAME}: Cannot Capture camera for Spout Sender. Unable to find shader '{blitShaderName}'.");
                return;
            }

            blitMaterial.SetInt("_flipTexture", 1);

            if (sharedTexture != null)
            {
                var tempRT = RenderTexture.GetTemporary
                   (sharedTexture.width, sharedTexture.height);

                cmd.Blit(textureToSend, tempRT, blitMaterial);
                cmd.CopyTexture(tempRT, sharedTexture);
                RenderTexture.ReleaseTemporary(tempRT);
            }
            // Update the plugin internal state.
            if (nativeSender != System.IntPtr.Zero)
            {
                System.IntPtr renderEventFunc = UVO.NativePluginBridge.GetRenderEventFunc();
                cmd.IssuePluginEventAndData(renderEventFunc, (int)NativePluginBridge.SPOUT.GraphicEvent.Update, nativeSender);
                // cmd.IssuePluginCustomTextureUpdateV2 // TODO: this is very interesting for later versions
                // Old school: UVO.Tools.GraphicEventManager.IssuePluginEvent((int)NativePluginBridge.SPOUT.GraphicEvent.Update, spoutSender);
            }
        }

        public void Cleanup()
        {
            CoreUtils.Destroy(blitMaterial);
            // TODO: proper clean of spout sender
            // For a proper kill we would need a commandBuffer to send the kill graphic event
            // For now it disappears when unity disappears
        }
    }
}

#endif