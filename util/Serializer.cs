using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
            var dict = serializer.SerializeObjectToDict("root", obj);
            return DictToJson(dict);
        }

        private Dictionary<string, object> SerializeObjectToDict(string path, object obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (_objectToPath.TryGetValue(obj, out string existingPath))
            {
                return new Dictionary<string, object> { { "$ref", existingPath } };
            }

            var dict = new Dictionary<string, object>();

            if (!(obj.GetType().IsPrimitive || obj is string || obj.GetType().IsEnum))
            {
                _objectToPath[obj] = path;
            }

            dict["$type"] = obj.GetType().AssemblyQualifiedName;

            if (UnityTypes.SerializeUnityType(obj, out var unityTypeDict))
            {
                foreach (var kvp in unityTypeDict)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }
            else if (obj is IList list)
            {
                var values = new List<object>();
                for (int i = 0; i < list.Count; i++)
                {
                    values.Add(SerializeObjectToDict($"{path}[{i}]", list[i]));
                }
                dict["$values"] = values;
            }
            else if (obj.GetType().IsPrimitive || obj is string || obj.GetType().IsEnum)
            {
                dict["$value"] = obj;
            }
            else
            {
                var fieldInfos = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fieldInfos)
                {
                    if (field.IsNotSerialized) continue;
                    dict[field.Name] = SerializeObjectToDict($"{path}.{field.Name}", field.GetValue(obj));
                }
                
                if (obj is UnityEngine.Object unityObj)
                {
                    dict["name"] = unityObj.name;
                    dict["hideFlags"] = unityObj.hideFlags;
                }
            }

            return dict;
        }

        public static T Deserialize<T>(string json)
        {
            var serializer = new Serializer();
            var dict = JsonToDict(json);
            return (T)serializer.DeserializeObjectFromDict("root", dict);
        }

        private object DeserializeObjectFromDict(string path, Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return null;
            }

            if (dict.TryGetValue("$ref", out object refPath))
            {
                return _pathToObject[(string)refPath];
            }

            string typeString = (string)dict["$type"];
            Type type = Type.GetType(typeString);
            object obj;

            if (type.IsPrimitive || type == typeof(string))
            {
                return dict["$value"];
            }

            if (type.IsEnum)
            {
                return Enum.Parse(type, (string)dict["$value"]);
            }

            if (UnityTypes.DeserializeUnityType(type, dict, out obj))
            {
                _pathToObject[path] = obj;
                return obj;
            }

            if (dict.TryGetValue("$values", out var values))
            {
                var list = (IList)Activator.CreateInstance(type);
                var valuesList = (List<object>)values;
                for (int i = 0; i < valuesList.Count; i++)
                {
                    list.Add(DeserializeObjectFromDict($"{path}[{i}]", (Dictionary<string, object>)valuesList[i]));
                }
                _pathToObject[path] = list;
                return list;
            }

            if (type.IsSubclassOf(typeof(Object)))
            {
                var unityObj = ScriptableObject.CreateInstance(type);
                unityObj.name = (string)dict["name"];
                unityObj.hideFlags = (HideFlags)dict["hideFlags"];
                obj = unityObj;
            }
            else
            {
                obj = Activator.CreateInstance(type);
            }

            _pathToObject[path] = obj;

            foreach (var kvp in dict)
            {
                if (kvp.Key == "$type") continue;
                var field = type.GetField(kvp.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(obj, DeserializeObjectFromDict($"{path}.{field.Name}", (Dictionary<string, object>)kvp.Value));
                }
            }

            return obj;
        }

        private static string DictToJson(Dictionary<string, object> dict)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            bool first = true;
            foreach (var kvp in dict)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append($"\"{kvp.Key}\":");
                ValueToJson(kvp.Value, sb);
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static void ValueToJson(object value, StringBuilder sb)
        {
            if (value == null)
            {
                sb.Append("null");
            }
            else if (value is string s)
            {
                sb.Append($"\"{EscapeJsonString(s)}\"");
            }
            else if (value is bool b)
            {
                sb.Append(b.ToString().ToLower());
            }
            else if (value is float f)
            {
                sb.Append(f.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (value is double d)
            {
                sb.Append(d.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (value is decimal m)
            {
                sb.Append(m.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            else if (value is Dictionary<string, object> dict)
            {
                sb.Append(DictToJson(dict));
            }
            else if (value is IList l)
            {
                sb.Append("[");
                bool first = true;
                foreach (var item in l)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    ValueToJson(item, sb);
                }
                sb.Append("]");
            }
            else if (value.GetType().IsEnum)
            {
                sb.Append($"\"{value}\"");
            }
            else
            {
                sb.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        private static string EscapeJsonString(string s)
        {
            return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f");
        }

        private static Dictionary<string, object> JsonToDict(string json)
        {
            int index = 0;
            return ParseObject(json, ref index);
        }

        private static Dictionary<string, object> ParseObject(string json, ref int index)
        {
            var dict = new Dictionary<string, object>();

            ConsumeWhitespace(json, ref index);
            if (json[index++] != '{') throw new FormatException("Expected '{'");

            while (true)
            {
                ConsumeWhitespace(json, ref index);
                if (json[index] == '}')
                {
                    index++;
                    return dict;
                }

                string key = ParseString(json, ref index);
                ConsumeWhitespace(json, ref index);
                if (json[index++] != ':') throw new FormatException("Expected ':'");
                ConsumeWhitespace(json, ref index);

                dict[key] = ParseValue(json, ref index);

                ConsumeWhitespace(json, ref index);
                if (json[index] == ',')
                {
                    index++;
                }
                else if (json[index] != '}')
                {
                    throw new FormatException("Expected ',' or '}'");
                }
            }
        }

        private static object ParseValue(string json, ref int index)
        {
            ConsumeWhitespace(json, ref index);

            switch (json[index])
            {
                case '"': 
                    string stringValue = ParseString(json, ref index);
                    // We can't check for enum type here, so we'll return the string value
                    // The caller (DeserializeObjectFromDict) will handle enum conversion if needed
                    return stringValue;
                case '{': return ParseObject(json, ref index);
                case '[': return ParseArray(json, ref index);
                case 't': case 'f': return ParseBoolean(json, ref index);
                case 'n': return ParseNull(json, ref index);
                default: return ParseNumber(json, ref index);
            }
        }


        private static string ParseString(string json, ref int index)
        {
            var sb = new StringBuilder();
            index++; // Skip opening quote
            while (true)
            {
                if (index >= json.Length) throw new FormatException("Unterminated string");
                char c = json[index++];
                if (c == '"') return sb.ToString();
                if (c == '\\')
                {
                    if (index >= json.Length) throw new FormatException("Unterminated string escape");
                    c = json[index++];
                    switch (c)
                    {
                        case '"': case '\\': case '/': sb.Append(c); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (index + 4 > json.Length) throw new FormatException("Invalid Unicode escape");
                            sb.Append((char)Convert.ToUInt16(json.Substring(index, 4), 16));
                            index += 4;
                            break;
                        default: throw new FormatException("Invalid string escape");
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        private static List<object> ParseArray(string json, ref int index)
        {
            var list = new List<object>();
            index++; // Skip opening bracket

            while (true)
            {
                ConsumeWhitespace(json, ref index);
                if (json[index] == ']')
                {
                    index++;
                    return list;
                }

                list.Add(ParseValue(json, ref index));

                ConsumeWhitespace(json, ref index);
                if (json[index] == ',')
                {
                    index++;
                }
                else if (json[index] != ']')
                {
                    throw new FormatException("Expected ',' or ']'");
                }
            }
        }

        private static bool ParseBoolean(string json, ref int index)
        {
            if (json.Substring(index, 4) == "true")
            {
                index += 4;
                return true;
            }
            if (json.Substring(index, 5) == "false")
            {
                index += 5;
                return false;
            }
            throw new FormatException("Expected 'true' or 'false'");
        }

        private static object ParseNull(string json, ref int index)
        {
            if (json.Substring(index, 4) == "null")
            {
                index += 4;
                return null;
            }
            throw new FormatException("Expected 'null'");
        }

        private static object ParseNumber(string json, ref int index)
        {
            int startIndex = index;
            bool isFloat = false;
            bool hasExponent = false;

            // Allow leading minus sign
            if (json[index] == '-')
                index++;

            // Parse integer part
            while (index < json.Length && char.IsDigit(json[index]))
                index++;

            // Parse fractional part
            if (index < json.Length && json[index] == '.')
            {
                isFloat = true;
                index++;
                while (index < json.Length && char.IsDigit(json[index]))
                    index++;
            }

            // Parse exponent part
            if (index < json.Length && (json[index] == 'e' || json[index] == 'E'))
            {
                hasExponent = true;
                index++;
                if (index < json.Length && (json[index] == '+' || json[index] == '-'))
                    index++;
                while (index < json.Length && char.IsDigit(json[index]))
                    index++;
            }

            string numberStr = json.Substring(startIndex, index - startIndex);

            if (isFloat || hasExponent)
            {
                if (double.TryParse(numberStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double doubleResult))
                    return doubleResult;
            }
            else
            {
                if (long.TryParse(numberStr, out long longResult))
                    return longResult;
            }

            throw new FormatException($"Invalid number format: {numberStr}");
        }


        private static void ConsumeWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }
        }
    }
}
