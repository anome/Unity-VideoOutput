using UnityEngine;
using UnityEngine.Rendering;

namespace UVS.Common
{
    public sealed class CompatibilityRules
    {
        public UnityVersion minimalRequiredVersion { get; }
        public RuntimePlatform[] compatiblePlatforms { get; }
        public GraphicsDeviceType[] compatibleGraphics { get; }
        public string uiNameForElement { get; }

        public CompatibilityRules(UnityVersion minimalRequiredVersion, RuntimePlatform[] compatiblePlatforms, GraphicsDeviceType[] compatibleGraphics, string uiNameForElement = "option")
        {
            this.minimalRequiredVersion = minimalRequiredVersion;
            this.compatiblePlatforms = compatiblePlatforms;
            this.compatibleGraphics = compatibleGraphics;
            this.uiNameForElement = uiNameForElement;
        }

        public bool IsCompatibleWithPlatform()
        {
            bool isPlatformCompatible = false;
            foreach (RuntimePlatform compatiblePlatform in compatiblePlatforms)
            {
                if (Application.platform == compatiblePlatform)
                {
                    isPlatformCompatible = true;
                    break;
                }
            }
            return isPlatformCompatible;
        }

        public bool IsCompatibleWithUnityVersion()
        {
            return CurrentUnityVersion.IsCurrentVersionAbove(minimalRequiredVersion.year, minimalRequiredVersion.month);
        }

        public bool IsCompatibleWithGraphicApi()
        {

            bool isGraphicCompatible = false;
            foreach (GraphicsDeviceType deviceType in compatibleGraphics)
            {
                if (SystemInfo.graphicsDeviceType == deviceType)
                {
                    isGraphicCompatible = true;
                    break;
                }
            }
            return isGraphicCompatible;
        }

        public bool IsCompatible()
        {
            if (!CurrentUnityVersion.IsCurrentVersionAbove(minimalRequiredVersion.year, minimalRequiredVersion.month))
            {
                return false;
            }

            bool isPlatformCompatible = IsCompatibleWithPlatform();
            if (!isPlatformCompatible)
            {
                return false;
            }

            return IsCompatibleWithGraphicApi() && IsCompatibleWithPlatform();
        }

        public string UserMessageToMakeItCompatible()
        {
            string message = "";

            if (!IsCompatibleWithUnityVersion())
            {
                message += MessageYouNeedUnityVersion("" + minimalRequiredVersion.year + "." + minimalRequiredVersion.month + "\n");
            }


            if (!IsCompatibleWithPlatform())
            {

                string readableCompatiblePlatforms = "";
                if (compatiblePlatforms.Length == 1)
                {
                    readableCompatiblePlatforms += compatiblePlatforms[0];
                }
                else if (1 < compatiblePlatforms.Length)
                {
                    readableCompatiblePlatforms += compatiblePlatforms[0];
                    for (int i = 1; i < compatiblePlatforms.Length; i++)
                    {
                        readableCompatiblePlatforms += " or " + compatiblePlatforms[i];
                    }
                }

                message += MessageOnlyWorksOnPlatform(readableCompatiblePlatforms);
            }


            if (!IsCompatibleWithGraphicApi())
            {
                string readableCompatibleGraphicApis = "";
                if (compatibleGraphics.Length == 1)
                {
                    readableCompatibleGraphicApis += compatibleGraphics[0];
                }
                else if (1 < compatibleGraphics.Length)
                {
                    readableCompatibleGraphicApis += compatibleGraphics[0];
                    for (int i = 1; i < compatibleGraphics.Length; i++)
                    {
                        readableCompatibleGraphicApis += " or " + compatibleGraphics[i];
                    }
                }

                message += MessageChangeForApi(readableCompatibleGraphicApis);
            }

            if (message == "")
            {
                return MessageDefaultNotCompatible();
            }

            return message;
        }

        private string MessageDefaultNotCompatible()
        {
            return "This " + uiNameForElement + " is not supported by your platform";
        }

        private string MessageChangeForApi(string api)
        {
            return "Please switch your Graphics API to " + api + " for this " + uiNameForElement + " (accessible in the Player Settings menu)";
        }

        private string MessageOnlyWorksOnPlatform(string platform)
        {
            return "This " + uiNameForElement + " only works on " + platform + " devices";
        }

        private string MessageYouNeedUnityVersion(string minimalVersion)
        {
            return "This " + uiNameForElement + " requires Unity " + minimalVersion + " or above";
        }

    }
}