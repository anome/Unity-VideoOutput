// Based on RenderPipelineUtils of https://github.com/MadGoat-Studio (thank you!)
using UnityEngine.Rendering;

namespace UVO
{
    public static class RenderPipelineDetector
    {

        public enum PipelineType
        {
            Unsupported,
            BuiltInPipeline,
            UniversalPipeline,
            HDPipeline
        }

        /// <summary>
        /// Returns the type of renderpipeline that is currently running
        /// </summary>
        /// <returns></returns>
        public static PipelineType DetectPipeline()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                // SRP
                var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (srpType.Contains("HDRenderPipelineAsset"))
                {
                    return PipelineType.HDPipeline;
                }
                else if (srpType.Contains("UniversalRenderPipelineAsset") || srpType.Contains("LightweightRenderPipelineAsset"))
                {
                    return PipelineType.UniversalPipeline;
                }
                else return PipelineType.Unsupported;
            }
#elif UNITY_2017_1_OR_NEWER
                if (GraphicsSettings.renderPipelineAsset != null) {
                    // SRP not supported before 2019
                    return PipelineType.Unsupported;
                }
#endif
            // no SRP
            return PipelineType.BuiltInPipeline;
        }
    }
}
