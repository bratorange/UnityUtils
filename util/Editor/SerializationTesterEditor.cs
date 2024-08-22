using System.Text;
using UnityEditor;
using UnityEngine;

namespace com.jsch.UnityUtil
{
    [CustomEditor(typeof(SerializationTester))]
    public class SerializationTesterEditor : Editor
    {
        private Vector2 scrollPosition;

        public override void OnInspectorGUI()
        {
            SerializationTester tester = (SerializationTester)target;

            // Draw runtimeGraph field
            EditorGUI.BeginChangeCheck();
            ScriptableObject newObject = (ScriptableObject)EditorGUILayout.ObjectField("Runtime Object",
                tester.ScriptableObject, typeof(ScriptableObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tester, "Change Runtime Graph");
                tester.ScriptableObject = newObject;
                EditorUtility.SetDirty(tester);
            }

            EditorGUILayout.LabelField("Wrapper:");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(800));

            // Format the JSON string for better readability
            string formattedJson = Serializer.FormatJson(serializedObject.FindProperty("serializedJSON").stringValue);
            EditorGUILayout.TextArea(formattedJson, GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();

            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
        }
    }
}