using System.Text;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

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

            // Custom read-only scrollable multiline text field for 'wrapper'
            EditorGUILayout.LabelField("Wrapper:");
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(800));

            // Format the JSON string for better readability
            string formattedJson = FormatJson(serializedObject.FindProperty("serializedJSON").stringValue);
            EditorGUILayout.TextArea(formattedJson, GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();

            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
        }

        private string FormatJson(string json)
        {
            int indentSize = 2;
            int indentLevel = 0;
            bool inQuotes = false;
            bool escapeNext = false;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];

                if (escapeNext)
                {
                    sb.Append(ch);
                    escapeNext = false;
                }
                else
                {
                    switch (ch)
                    {
                        case '\\':
                            sb.Append(ch);
                            escapeNext = true;
                            break;
                        case '"':
                            sb.Append(ch);
                            inQuotes = !inQuotes;
                            break;
                        case '{':
                        case '[':
                            sb.Append(ch);
                            if (!inQuotes)
                            {
                                sb.AppendLine();
                                indentLevel++;
                                sb.Append(new string(' ', indentLevel * indentSize));
                            }

                            break;
                        case '}':
                        case ']':
                            if (!inQuotes)
                            {
                                sb.AppendLine();
                                indentLevel--;
                                sb.Append(new string(' ', indentLevel * indentSize));
                            }

                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            if (!inQuotes)
                            {
                                sb.AppendLine();
                                sb.Append(new string(' ', indentLevel * indentSize));
                            }

                            break;
                        case ':':
                            sb.Append(ch);
                            if (!inQuotes)
                                sb.Append(" ");
                            break;
                        default:
                            sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString();
        }
    }
}