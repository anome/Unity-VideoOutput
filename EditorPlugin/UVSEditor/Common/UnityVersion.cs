using System;
using UnityEngine;
using System.Collections.Generic;

namespace UVS.Common
{

    public sealed class UnityVersion
    {
        public int year { get; }
        public int month { get; }

        public UnityVersion(int year, int month)
        {
            this.year = year;
            this.month = month;
        }
    }

    public static class CurrentUnityVersion
    {

        public static class Major
        {
            public static int Year
            {
                get
                {
                    return Int32.Parse(StringSplitter.Split(Application.unityVersion, '.')[0]);
                }
            }

            public static int Month
            {
                get
                {
                    return Int32.Parse(StringSplitter.Split(Application.unityVersion, '.')[1]);
                }
            }
        }


        public static string Minor
        {
            get
            {
                return StringSplitter.Split(Application.unityVersion, '.')[2];
            }
        }

        public static bool IsCurrentVersionAbove(int year, int month)
        {
            return (Major.Year == year && month <= Major.Month) || (year < Major.Year);
        }

        // Why is there a StringSplitter class here when we have access to the String library and its own Split implementation ??
        // Here's why:
        // This plugin is compiled as a managed DLL that's run by Unity, inside Unity context
        // This managed DLL needs to be compiled with the right .NET symbols to work.
        // For some unclear reason, the String.Split method is only available in the .NET Portable symbols as stated here
        // https://answers.unity.com/questions/1478418/custom-dll-missingmethodexception-string-stringspl.html?sort=votes
        // Which means that in Unity, without this symbols calling the String.Split fires a 
        // MissingMethodException: string[] string.Split(char,System.StringSplitOptions)
        // To avoid this issue, the intuitive solution would be to add the .NET Portable symbol to our build, which was my first approach
        // But as of today, I could not figure out a way to alter the VisualStudio project .csproj file for bundling .NET Portable 
        // AND keep the .csproj working on Windows and OSX as a cross-platform project
        // Indeed, to this day, Visual Studio on Windows has a bug regarding project files that the dev team flagged as "low priority" 
        // As stated here
        // https://developercommunity.visualstudio.com/content/problem/317628/your-project-does-not-reference-netframeworkversio.html
        // This bug makes the working .csproj on OSX unusable on Windows, and I could not find a viable solution to it in a decent amount of time
        // So, that's why.
        public static class StringSplitter
        {
            public static string[] Split(string stringToSplit, char splitCharacter)
            {
                List<string> elements = new List<string>();

                for (int i = 0, startIndexToSplit = 0; i < stringToSplit.Length; i++)
                {
                    if (stringToSplit[i] == splitCharacter)
                    {
                        int lengthToTake = i - startIndexToSplit;
                        string part = stringToSplit.Substring(startIndexToSplit, lengthToTake);
                        elements.Add(part);
                        startIndexToSplit = i + 1;
                    }
                }
                return elements.ToArray();
            }
        }
    }
}