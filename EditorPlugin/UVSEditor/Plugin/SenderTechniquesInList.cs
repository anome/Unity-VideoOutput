using System;
using UnityEngine;
using UnityEditor;

namespace UVS
{
    // Unity UI Element
    public class SenderTechniquesInList : PropertyAttribute
    {

        public delegate SenderTechniques.SenderTechniqueEnum[] GetRichEnumList();

        public SenderTechniquesInList()
        {
            List = SenderTechniques.AvailableTechniques();
        }

        public SenderTechniques.SenderTechniqueEnum[] List
        {
            get;
            private set;
        }

        public string displayName
        {
            get
            {
                return "Technique";
            }
        }
    }


    [CustomPropertyDrawer(typeof(SenderTechniquesInList))]
    public class RichEnumInListDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enumInList = attribute as SenderTechniquesInList;
            var list = enumInList.List;

            //Populate readable list
            var stringList = new string[list.Length];
            for (var i = 0; i < list.Length; i++)
            {
                stringList[i] = SenderTechniques.GetReadableFormatFromEnum(list[i]);
            }
            // enum int values list
            var enumIntList = new int[list.Length];
            for (var i = 0; i < list.Length; i++)
            {
                enumIntList[i] = (int)list[i];
            }

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                int index = Mathf.Max(0, Array.IndexOf(enumIntList, property.enumValueIndex));
                index = EditorGUI.Popup(position, property.displayName, index, stringList);
                property.enumValueIndex = (int)list[index];
            }
            else
            {
                base.OnGUI(position, property, new GUIContent("ERROR"));
            }
        }
    }

}
