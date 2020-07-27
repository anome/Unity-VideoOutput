
#if UVO_ASMDEF_HAS_CORE_SRP
using UnityEngine;
using UnityEngine.Rendering;

namespace UVO.SRP
{
    class SyphonSender
    {
        Texture2D sharedTexture = null;
        Material blitMaterial;
        const string blitShaderName = "UVO/SRP/MetalBlit";
        string outputName  = "UnityVideoOutput";

        public void Setup(string outputName="UnityVideoOutput")
        {
            if (Shader.Find(blitShaderName) != null)
            {
                blitMaterial = new Material(Shader.Find(blitShaderName));
            }
            else
            {
                Debug.LogError($"{PluginData.PRODUCTION_PLUGIN_NAME}: Cannot start Syphon Sender. Unable to find shader '{blitShaderName}'.");
                return;
            }
            this.outputName = outputName;
            NativePluginBridge.setApiState((int)NativePluginBridge.ApiStates.SendSyphonMetal);
        }

        private void LazyCreateSyphonSender(CommandBuffer cmd, int textureWidth, int textureHeight)
        {
            // Texture resize detection
            if(sharedTexture != null)
            {
                if (sharedTexture.width != textureWidth || sharedTexture.height != textureHeight)
                {
                    // kill sender entirely and skip frame
                    System.IntPtr renderEventFc = UVO.NativePluginBridge.GetRenderEventFunc();
                    cmd.IssuePluginEventAndData(renderEventFc, (int)NativePluginBridge.SYPHON.GraphicEvent.StopServer, System.IntPtr.Zero);
                    CoreUtils.Destroy(sharedTexture);
                    sharedTexture = null;
                    return;
                }
            }

            // Shared texture lazy initialization
            if (sharedTexture == null)
            {

                int gammaColorSpace = 0;
                if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
                {
                    gammaColorSpace = 1;
                }
               
                System.IntPtr serverTex = NativePluginBridge.SYPHON.CreateMetalServer(outputName, textureWidth, textureHeight, gammaColorSpace);
                sharedTexture = Texture2D.CreateExternalTexture(textureWidth, textureHeight, TextureFormat.BGRA32, false, false, serverTex);
            }
        }

        public void SendTextureByCameraBridge(CommandBuffer cmd, RenderTargetIdentifier textureToSend, int width, int height)
        {
            LazyCreateSyphonSender(cmd, width, height);
            ;

            if (blitMaterial == null)
            {
                Debug.LogError($"{PluginData.PRODUCTION_PLUGIN_NAME}: Cannot Capture camera for Spout Sender. Unable to find shader '{blitShaderName}'.");
                return;
            }

            if (sharedTexture != null)
            {
                var tempRT = RenderTexture.GetTemporary(sharedTexture.width, sharedTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
                // Alpha support set to false by default
                cmd.Blit(textureToSend, tempRT, blitMaterial, 0);
                cmd.CopyTexture(tempRT, sharedTexture);
                RenderTexture.ReleaseTemporary(tempRT);
                // Inform plugin of change
                System.IntPtr renderEventFc = UVO.NativePluginBridge.GetRenderEventFunc();
                cmd.IssuePluginEventAndData(renderEventFc, (int)NativePluginBridge.SYPHON.GraphicEvent.PublishFrame, System.IntPtr.Zero);
                // Old school way (still works)
                //UVO.Tools.GraphicEventManager.IssuePluginEvent((int)NativePluginBridge.SYPHON.GraphicEvent.PublishFrame, System.IntPtr.Zero);
            }
            else
            {
                Debug.Log($"{PluginData.PRODUCTION_PLUGIN_NAME}: shared texture is null");
            }

        }

        public void Cleanup()
        {
            UVO.Tools.GraphicEventManager.IssuePluginEvent((int)NativePluginBridge.SYPHON.GraphicEvent.StopServer, System.IntPtr.Zero);
            sharedTexture = null;
            CoreUtils.Destroy(blitMaterial);
        }
    }
}

#endif
