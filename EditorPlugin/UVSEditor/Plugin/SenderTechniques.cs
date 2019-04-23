using System;
using UnityEngine;
using System.Collections.Generic;

namespace UVS
{
    using Common;

    public sealed class SenderTechnique
    {
        public int enumIndex { get; }
        public string readableName { get; }
        public CompatibilityRules compatibilityRules { get; }
        public SenderTechnique(int enumIndex, string readableName, CompatibilityRules compatibilityRules)
        {
            this.enumIndex = enumIndex;
            this.readableName = readableName;
            this.compatibilityRules = compatibilityRules;
        }
    }

    public sealed class SenderTechniques
    {
        // Enum and techniques needs to be in the same order in array
        public enum SenderTechniqueEnum
        {
            NDI = 0,
            OpenGLSyphon = 1,
            MetalSyphon = 2,
            Spout = 3,
        };


        private static List<SenderTechnique> senderTechniquesList = new List<SenderTechnique>
    {
        new SenderTechnique(0,
            "NDI",
            new CompatibilityRules(
            new UnityVersion(2018,3),
            new [] {RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor},
            new [] {UnityEngine.Rendering.GraphicsDeviceType.Direct3D11, UnityEngine.Rendering.GraphicsDeviceType.Direct3D12,
                    UnityEngine.Rendering.GraphicsDeviceType.Metal}
        )),

        new SenderTechnique(1,
            "Syphon (OpenGL)",
            new CompatibilityRules(
            new UnityVersion(2017,4),
            new [] {RuntimePlatform.OSXEditor},
                new [] {UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore}
        )),

        new SenderTechnique(2,
            "Syphon (Metal)",
            new CompatibilityRules(
            new UnityVersion(2017,4),
            new [] {RuntimePlatform.OSXEditor},
            new [] {UnityEngine.Rendering.GraphicsDeviceType.Metal}
        )),

        new SenderTechnique(3,
            "Spout",
            new CompatibilityRules(
            new UnityVersion(2017,4),
            new [] {RuntimePlatform.WindowsEditor},
            new [] {UnityEngine.Rendering.GraphicsDeviceType.Direct3D11, UnityEngine.Rendering.GraphicsDeviceType.Direct3D12}
        ))
    };

        public static string GetReadableFormatFromEnum(SenderTechniqueEnum senderType)
        {
            return senderTechniquesList[(int)senderType].readableName;
        }

        private static SenderTechniqueEnum GetEnumFromReadableName(string readableName)
        {
            for (int i = 0; i < senderTechniquesList.Count; i++)
            {
                if (senderTechniquesList[i].readableName == readableName)
                {
                    return (SenderTechniqueEnum)i;
                }
            }
            return (SenderTechniqueEnum)(-1);
        }

        public static SenderTechniqueEnum[] AvailableTechniques()
        {
            List<SenderTechniqueEnum> availableTechniques = new List<SenderTechniqueEnum>();

            foreach (SenderTechnique techniqueToTest in senderTechniquesList)
            {
                if (techniqueToTest.compatibilityRules.IsCompatibleWithPlatform())
                {
                    availableTechniques.Add((SenderTechniqueEnum)techniqueToTest.enumIndex);
                }
            }
            return availableTechniques.ToArray(); ;
        }

        public static bool CanIRunThisTechnique(SenderTechniques.SenderTechniqueEnum technique)
        {
            // Only hide the techniques for other platforms
            return senderTechniquesList[(int)technique].compatibilityRules.IsCompatible();
        }

        public static string WhyCantIRunThisTechnique(SenderTechniques.SenderTechniqueEnum technique)
        {
            if (senderTechniquesList[(int)technique].compatibilityRules.IsCompatible())
            {
                return "Your system can run this technique.";
            }
            else
            {
                return senderTechniquesList[(int)technique].compatibilityRules.UserMessageToMakeItCompatible();
            }
        }
    }

}
