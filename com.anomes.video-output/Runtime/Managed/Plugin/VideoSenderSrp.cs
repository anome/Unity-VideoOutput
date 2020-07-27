
#if UVO_ASMDEF_HAS_CORE_SRP
//using UnityEngine.Rendering.HighDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;


namespace UVO
{
    [HelpURL(PluginData.HELP_URL)]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [AddComponentMenu(PluginData.PRODUCTION_PLUGIN_NAME + "/" + PluginData.PRODUCTION_PLUGIN_NAME + " (SRP)")]
    [RequireComponent(typeof(Camera))]
    public class VideoSenderSrp : MonoBehaviour
    {
        [SerializeField] public SendTechnique outputType = SendTechnique.SyphonSpout;
        // Start is called before the first frame update
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        UVO.SRP.SyphonSender syphonSpoutSender = new UVO.SRP.SyphonSender();
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        UVO.SRP.SpoutSender syphonSpoutSender = new UVO.SRP.SpoutSender();
#endif
        UVO.SRP.NdiSender ndiSender = new UVO.SRP.NdiSender();
        Action<RenderTargetIdentifier, CommandBuffer> ndiAction;
        Action<RenderTargetIdentifier, CommandBuffer> syphonSpoutAction;

#if UVO_API_GLCORE || UVO_API_UNSUPPORTED
        private void OnEnable()
        {
            string switchToPlatform = "";
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            switchToPlatform = "Metal";
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
             switchToPlatform = "DirectX D11";
#endif
            Debug.LogError(PluginData.PRODUCTION_PLUGIN_NAME + ": OpenGL API is not supported. Please switch your API to " + switchToPlatform + " in Project Settings -> Player");
        }
#else
        
        private void OnEnable()
        {

            ndiAction = (RenderTargetIdentifier id, CommandBuffer cmd) =>
            {
                var cameraOutput = GetComponent<Camera>();
                ndiSender.SendTextureByCameraBridge(cmd, id, cameraOutput.pixelWidth, cameraOutput.pixelHeight);
            };
            syphonSpoutAction = (RenderTargetIdentifier id, CommandBuffer cmd) =>
            {
                var cameraOutput = GetComponent<Camera>();
                syphonSpoutSender.SendTextureByCameraBridge(cmd, id, cameraOutput.pixelWidth, cameraOutput.pixelHeight);
            };
            CameraCaptureBridge.enabled = true; // Enable when edit?
            RefreshOutputType();
        }

        private void KillCaptures()
        {
            if (ndiAction != null)
            {
                CameraCaptureBridge.RemoveCaptureAction(GetComponent<Camera>(), ndiAction);
            }
            if (syphonSpoutAction != null)
            {
                CameraCaptureBridge.RemoveCaptureAction(GetComponent<Camera>(), syphonSpoutAction);
            }
            ndiSender.Cleanup();
            syphonSpoutSender.Cleanup();
        }

        private void RefreshOutputType()
        {
            KillCaptures();

            switch (outputType)
            {
                case SendTechnique.NDI:
                    if (ndiAction != null)
                    {
                        ndiSender.Setup(gameObject.name);
                        CameraCaptureBridge.AddCaptureAction(GetComponent<Camera>(), ndiAction);
                    }

                    break;
                case SendTechnique.SyphonSpout:
                    if (syphonSpoutAction != null)
                    {
                        syphonSpoutSender.Setup(gameObject.name);
                        CameraCaptureBridge.AddCaptureAction(GetComponent<Camera>(), syphonSpoutAction);
                    }
                    break;
            }
        }
        // Fired only when dropdown value changes
        private void OnValidate()
        {
            RefreshOutputType();
        }

        private void OnDisable()
        {
            KillCaptures();
        }
        private void OnApplicationQuit()
        {
            KillCaptures();
        }
#endif
    }

}

#endif