// Based on https://github.com/UnityCommunity/UnityLibrary/blob/master/Assets/Scripts/Editor/AddDefineSymbols.cs
// Thank you https://github.com/UnityCommunity/

namespace UVO
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// Adds the given define symbols to PlayerSettings define symbols.
    /// Just add your own define symbols to the Symbols property at the below.
    /// </summary>
    [InitializeOnLoad]
    public class PluginCustomSymbols : Editor
    {
        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        static PluginCustomSymbols()
        {

            string[] PluginSymbols = new string[] {
            RenderPipelinePreprocessorSymbol(),
            GraphicsEnginePreprocessorSymbol()
            };
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').Where(define => !define.Contains("UVO_")).ToList();
            allDefines.AddRange(PluginSymbols.Except(allDefines));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }

        /// <summary>
        /// Detect render pipeline and returns the plugin preprocessor symbol used inside this plugin
        /// </summary>
        static string RenderPipelinePreprocessorSymbol()
        {
            UVO.RenderPipelineDetector.PipelineType type = UVO.RenderPipelineDetector.DetectPipeline();
            switch (type)
            {
                case UVO.RenderPipelineDetector.PipelineType.BuiltInPipeline:
                    return "UVO_PIPELINE_BUILT_IN";
                case UVO.RenderPipelineDetector.PipelineType.UniversalPipeline:
                    return "UVO_PIPELINE_UNIVERSAL";
                case UVO.RenderPipelineDetector.PipelineType.HDPipeline:
                    return "UVO_PIPELINE_HD";
                case UVO.RenderPipelineDetector.PipelineType.Unsupported:
                    return "UVO_PIPELINE_UNSUPPORTED";

                default:
                    Debug.LogError("UVO: Unexpected pipeline type:" + type);
                    return "";
            }
        }

        /// <summary>
        /// Detect graphic engine API and returns the plugin preprocessor symbol used inside this plugin
        /// </summary>
        static string GraphicsEnginePreprocessorSymbol()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11)
            {
                return "UVO_API_DIRECTX11";
            }
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore)
            {
                return "UVO_API_GLCORE";
            }
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
            {
                return "UVO_API_METAL";
            }

            return "UVO_API_UNSUPPORTED";
        }
    }

}
