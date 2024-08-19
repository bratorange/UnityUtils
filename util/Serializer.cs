using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.jsch.UnityUtil
{
    public class Serializer
    {
        private readonly Dictionary<object, string> _objectToPath = new Dictionary<object, string>();
        private readonly Dictionary<string, object> _pathToObject = new Dictionary<string, object>();

        public static string Serialize(object obj)
        {
            var serializer = new Serializer();
            var sb = new StringBuilder();
            serializer.SerializeObject("root", obj, sb);
            return sb.ToString();
        }

        private void SerializeObject(string path, object obj, StringBuilder sb)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }

            if (_objectToPath.TryGetValue(obj, out string existingPath))
            {
                sb.Append($"{{\"$ref\":\"{existingPath}\"}}");
                return;
            }

            if (!(obj.GetType().IsPrimitive || obj is string || obj.GetType().IsEnum))
            {
                _objectToPath[obj] = path;
            }

            sb.Append("{");
            sb.Append($"\"$type\":\"{obj.GetType().AssemblyQualifiedName}\",");

            if (UnityTypes.SerializeUnityType(obj, sb))
            {
                // Unity type serialization successful
            }
            else if (obj is IList list)
            {
                sb.Append("\"$values\":[");
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    SerializeObject($"{path}[{i}]", list[i], sb);
                }
                sb.Append("]");
            }
            else if (obj.GetType().IsPrimitive || obj is string || obj.GetType().IsEnum)
            {
                sb.Append("\"$value\":");
                SerializePrimitive(obj, sb);
            }
            else
            {
                var fieldInfos = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var kvps = new List<KeyValuePair<string, object>>();
                foreach (var field in fieldInfos)
                {
                    if (field.IsNotSerialized) continue;
                    kvps.Add(new KeyValuePair<string, object>(field.Name, field.GetValue(obj)));
                }
                // some unity-object internal properties are not marked as serialized, so we need to add them manually
                if (obj is UnityEngine.Object unityObj)
                {
                    kvps.Add(new KeyValuePair<string, object>("name", unityObj.name));
                    kvps.Add(new KeyValuePair<string, object>("hideFlags", unityObj.hideFlags));
                }
                
                bool first = true;
                foreach (var kvp in kvps)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append($"\"{kvp.Key}\":");
                    SerializeObject($"{path}.{kvp.Key}", kvp.Value, sb);
                }
            }

            sb.Append("}");
        }

        public static T Deserialize<T>(string json)
        {
            var serializer = new Serializer();
            return (T)serializer.DeserializeObject("root", json);
        }

        private object DeserializeObject(string path, string json)
        {
            json = json.Trim();

            if (json.StartsWith("{\"$ref\":"))
            {
                var refPath = json.Substring(8, json.Length - 9).Trim('"');
                return _pathToObject[refPath];
            }

            var dict = RecoverJsonDictionary(json);

            string typeString = dict["$type"].Trim('"');
            Type type = Type.GetType(typeString);
            object obj;

            if (type.IsPrimitive || type == typeof(string) || type.IsEnum) // case of a primitive
            {
                obj = DeserializePrimitive(dict["$value"], type);
                return obj;
            }

            if (UnityTypes.DeserializeUnityType(type, dict, out obj)) // case of a Unity type
            {
                _pathToObject[path] = obj;
                return obj;
            }

            if (dict.TryGetValue("$values", out var valuesJSON)) // case of a list
            {
                _pathToObject[path] = obj;
                var list = (IList)Activator.CreateInstance(type);
                
                var trimmedJSON = valuesJSON.Trim();
                if (trimmedJSON.StartsWith("[") && trimmedJSON.EndsWith("]"))
                {
                    trimmedJSON = trimmedJSON.Substring(1, trimmedJSON.Length - 2);
                    var items = SplitJsonArray(trimmedJSON);
                    for (int i = 0; i < items.Count; i++)
                    {
                        list.Add(DeserializeObject($"{path}[{i}]", items[i]));
                    }
                }

                return list;
            }

            // case of a class
            _pathToObject[path] = obj;

            if (type.IsSubclassOf(typeof(Object)))
            {
                var unityObj = ScriptableObject.CreateInstance(type);
                unityObj.name = DeserializeObject($"{path}.name", dict["name"]) as string;
                unityObj.hideFlags = DeserializeObject($"{path}.hideFlags", dict["hideFlags"]) as HideFlags? ?? HideFlags.None;
                obj = unityObj;
            }
            else
                obj = Activator.CreateInstance(type);
            foreach (var kvp in dict)
            {
                if (kvp.Key == "$type") continue;
                var field = type.GetField(kvp.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(obj, DeserializeObject($"{path}.{field.Name}",kvp.Value));
                }
            }

            return obj;
        }

        private Dictionary<string, string> RecoverJsonDictionary(string json)
        {
            json = json.Substring(1, json.Length - 2);

            var pairs = SplitJsonPairs(json);
            var dict = new Dictionary<string, string>();

            foreach (var pair in pairs)
            {
                var colonIndex = pair.IndexOf(':');
                var key = pair.Substring(0, colonIndex).Trim().Trim('"');
                var value = pair.Substring(colonIndex + 1).Trim();
                dict[key] = value;
            }

            return dict;
        }


        private object DeserializePrimitive(string value, Type type)
        {
            value = value.Trim('"');
            if (type == typeof(float))
                return float.Parse(value);
            if (type == typeof(double))
                return double.Parse(value);
            if (type == typeof(bool))
                return bool.Parse(value);
            if (type == typeof(int))
                return int.Parse(value);
            if (type == typeof(long))
                return long.Parse(value);
            if (type == typeof(char))
                return value[0];
            if (type == typeof(string))
                return value;
            if (type.IsEnum)
                return Enum.Parse(type, value);

            throw new NotSupportedException($"Unsupported primitive type: {type}");
        }
        private void SerializePrimitive(object obj, StringBuilder sb)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }

            switch (obj)
            {
                case bool b:
                    sb.Append(b.ToString().ToLowerInvariant());
                    break;
                case byte by:
                case sbyte sby:
                case short s:
                case ushort us:
                case int i:
                case uint ui:
                case long l:
                case ulong ul:
                    sb.Append(obj);
                    break;
                case float f:
                    sb.Append(f.ToString("G9", System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case double d:
                    sb.Append(d.ToString("G17", System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case decimal dec:
                    sb.Append(dec.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case char c:
                    sb.Append('"').Append(EscapeChar(c)).Append('"');
                    break;
                case string s:
                    sb.Append('"').Append(EscapeString(s)).Append('"');
                    break;
                case Enum e:
                    sb.Append('"').Append(e.ToString()).Append('"');
                    break;
                default:
                    throw new ArgumentException($"Unsupported primitive type: {obj.GetType()}");
            }
        }
        private string EscapeString(string s)
        {
            return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f");
        }
        private string EscapeChar(char c)
        {
            return c switch
            {
                '\\' => "\\\\",
                '\"' => "\\\"",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\b' => "\\b",
                '\f' => "\\f",
                _ => c.ToString()
            };
        }
        private List<string> SplitJsonPairs(string json)
        {
            var pairs = new List<string>();
            var currentPair = new StringBuilder();
            var depth = 0;
            var inQuotes = false;
            var escapeNext = false;

            foreach (var c in json)
            {
                if (escapeNext)
                {
                    currentPair.Append(c);
                    escapeNext = false;
                    continue;
                }

                if (c == '\\')
                {
                    escapeNext = true;
                    currentPair.Append(c);
                    continue;
                }

                if (c == '"' && !escapeNext)
                {
                    inQuotes = !inQuotes;
                }

                if (!inQuotes)
                {
                    if (c == '{' || c == '[')
                    {
                        depth++;
                    }
                    else if (c == '}' || c == ']')
                    {
                        depth--;
                    }
                }

                if (c == ',' && depth == 0 && !inQuotes)
                {
                    pairs.Add(currentPair.ToString().Trim());
                    currentPair.Clear();
                }
                else
                {
                    currentPair.Append(c);
                }
            }

            if (currentPair.Length > 0)
            {
                pairs.Add(currentPair.ToString().Trim());
            }

            return pairs;
        }
        private List<string> SplitJsonArray(string json)
        {
            var items = new List<string>();
            var currentItem = new StringBuilder();
            var depth = 0;
            var inQuotes = false;

            foreach (var c in json)
            {
                if (c == '"' && (currentItem.Length == 0 || currentItem[currentItem.Length - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                }

                if (!inQuotes)
                {
                    if (c == '{' || c == '[')
                    {
                        depth++;
                    }
                    else if (c == '}' || c == ']')
                    {
                        depth--;
                    }
                }

                if (c == ',' && depth == 0 && !inQuotes)
                {
                    items.Add(currentItem.ToString().Trim());
                    currentItem.Clear();
                }
                else
                {
                    currentItem.Append(c);
                }
            }

            if (currentItem.Length > 0)
            {
                items.Add(currentItem.ToString().Trim());
            }

            return items;
        }

    }
}
