using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

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

            if (obj is UnityEngine.Object unityObject)
            {
                dict["$name"] = unityObject.name;
                dict["$hideFlags"] = (int)unityObject.hideFlags;
            }

            if (UnityTypes.SerializeUnityType(obj, out var unityTypeDict))
            {
                foreach (var kvp in unityTypeDict)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }
            else if (obj is IDictionary dictionary)
            {
                var keys = new List<object>();
                var values = new List<object>();
                foreach (DictionaryEntry entry in dictionary)
                {
                    keys.Add(SerializeObjectToDict($"{path}[key]", entry.Key));
                    values.Add(SerializeObjectToDict($"{path}[{entry.Key}]", entry.Value));
                }
                dict["$keys"] = keys;
                dict["$values"] = values;
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
            else if (obj.GetType().IsPrimitive || obj is string)
            {
                dict["$value"] = obj;
            }
            else if (obj.GetType().IsEnum)
            {
                dict["$value"] = obj.ToString();
            }
            else
            {
                Type currentType = obj.GetType();
                while (currentType != null)
                {
                    var fieldInfos = currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                           BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var field in fieldInfos)
                    {
                        if (field.IsNotSerialized) continue;
                        dict[field.Name] = SerializeObjectToDict($"{path}.{field.Name}", field.GetValue(obj));
                    }
                    currentType = currentType.BaseType;
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
            }
            else if (dict.TryGetValue("$keys", out var keys) && dict.TryGetValue("$values", out var values))
            {
                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    var dictionary = (IDictionary)Activator.CreateInstance(type);
                    var keysList = (List<object>)keys;
                    var valuesList = (List<object>)values;
                    for (int i = 0; i < keysList.Count; i++)
                    {
                        object key = DeserializeObjectFromDict($"{path}[key]", (Dictionary<string, object>)keysList[i]);
                        object value = DeserializeObjectFromDict($"{path}[{key}]", (Dictionary<string, object>)valuesList[i]);
                        dictionary.Add(key, value);
                    }
                    obj = dictionary;
                }
                else
                {
                    throw new InvalidOperationException($"Expected IDictionary, but got {type}");
                }
                _pathToObject[path] = obj;
            }
            else if (dict.TryGetValue("$values", out var listValues))
            {
                var list = (IList)Activator.CreateInstance(type);
                var valuesList = (List<object>)listValues;
                var elementType = type.GetGenericArguments()[0];
                for (int i = 0; i < valuesList.Count; i++)
                {
                    var item = DeserializeObjectFromDict($"{path}[{i}]", (Dictionary<string, object>)valuesList[i]);
                    if (item != null)
                    {
                        if (!elementType.IsAssignableFrom(item.GetType()))
                        {
                            try
                            {
                                item = Convert.ChangeType(item, elementType);
                            }
                            catch (InvalidCastException)
                            {
                                Debug.LogWarning(
                                    $"Unable to convert value of type {item.GetType()} to {elementType} for list element at index {i}");
                                continue;
                            }
                        }
                        list.Add(item);
                    }
                }
                obj = list;
                _pathToObject[path] = obj;
            }
            else
            {
                if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    obj = ScriptableObject.CreateInstance(type);
                }
                else
                {
                    obj = Activator.CreateInstance(type);
                }

                _pathToObject[path] = obj;

                if (obj is UnityEngine.Object unityObj)
                {
                    if (dict.TryGetValue("$name", out object name))
                    {
                        unityObj.name = (string)name;
                    }
                    if (dict.TryGetValue("$hideFlags", out object hideFlags))
                    {
                        unityObj.hideFlags = (HideFlags)Convert.ToInt32(hideFlags);
                    }
                }

                Type currentType = type;
                while (currentType != null)
                {
                    foreach (var kvp in dict)
                    {
                        if (kvp.Key == "$type" || kvp.Key == "$name" || kvp.Key == "$hideFlags") continue;

                        var field = currentType.GetField(kvp.Key,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                            BindingFlags.DeclaredOnly);
                        if (field != null)
                        {
                            object fieldValue = DeserializeObjectFromDict($"{path}.{field.Name}",
                                (Dictionary<string, object>)kvp.Value);
                            if (fieldValue != null && !field.FieldType.IsAssignableFrom(fieldValue.GetType()))
                            {
                                if (field.FieldType.IsEnum)
                                {
                                    fieldValue = Enum.Parse(field.FieldType, fieldValue.ToString());
                                }
                                else
                                {
                                    try
                                    {
                                        fieldValue = Convert.ChangeType(fieldValue, field.FieldType);
                                    }
                                    catch (InvalidCastException)
                                    {
                                        Debug.LogWarning(
                                            $"Unable to assign value of type {fieldValue.GetType()} to field {field.Name} of type {field.FieldType}");
                                        continue;
                                    }
                                }
                            }
                            field.SetValue(obj, fieldValue);
                        }
                    }
                    currentType = currentType.BaseType;
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
                case 't':
                case 'f': return ParseBoolean(json, ref index);
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
                        case '"':
                        case '\\':
                        case '/':
                            sb.Append(c);
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
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
                if (double.TryParse(numberStr, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double doubleResult))
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