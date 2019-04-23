using UnityEngine;
using UnityEditor;

namespace UVS
{
    [CustomEditor(typeof(VideoSender))]
    [ExecuteInEditMode]
    sealed class VideoSenderEditor : Editor
    {
        SerializedProperty _senderTechnique;

        void OnEnable()
        {
            _senderTechnique = serializedObject.FindProperty("_senderTechnique");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            string helpboxMessage;
            MessageType helpboxMessageType;
            bool canIRunThisTechnique = SenderTechniques.CanIRunThisTechnique((SenderTechniques.SenderTechniqueEnum)_senderTechnique.enumValueIndex);
            if (canIRunThisTechnique)
            {
                helpboxMessage = "Plugin is running correctly";
                helpboxMessageType = MessageType.Info;
            }
            else
            {
                helpboxMessage = SenderTechniques.WhyCantIRunThisTechnique((SenderTechniques.SenderTechniqueEnum)_senderTechnique.enumValueIndex);
                helpboxMessageType = MessageType.Error;
            }

            EditorGUILayout.PropertyField(_senderTechnique);
            EditorGUILayout.HelpBox(helpboxMessage, helpboxMessageType);
            serializedObject.ApplyModifiedProperties();
        }
    }

}
