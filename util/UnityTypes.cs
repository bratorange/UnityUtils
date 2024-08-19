using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace com.jsch.UnityUtil
{
    public static class UnityTypes
    {
        internal static bool SerializeUnityType(object obj, StringBuilder sb)
        {
            if (obj is Vector2 vector2)
            {
                sb.Append($"\"x\":{vector2.x},\"y\":{vector2.y}");
                return true;
            }

            if (obj is Vector3 vector3)
            {
                sb.Append($"\"x\":{vector3.x},\"y\":{vector3.y},\"z\":{vector3.z}");
                return true;
            }

            if (obj is Vector4 vector4)
            {
                sb.Append($"\"x\":{vector4.x},\"y\":{vector4.y},\"z\":{vector4.z},\"w\":{vector4.w}");
                return true;
            }

            if (obj is Quaternion quaternion)
            {
                sb.Append($"\"x\":{quaternion.x},\"y\":{quaternion.y},\"z\":{quaternion.z},\"w\":{quaternion.w}");
                return true;
            }

            if (obj is Color color)
            {
                sb.Append($"\"r\":{color.r},\"g\":{color.g},\"b\":{color.b},\"a\":{color.a}");
                return true;
            }

            if (obj is Color32 color32)
            {
                sb.Append($"\"r\":{color32.r},\"g\":{color32.g},\"b\":{color32.b},\"a\":{color32.a}");
                return true;
            }

            if (obj is Rect rect)
            {
                sb.Append($"\"x\":{rect.x},\"y\":{rect.y},\"width\":{rect.width},\"height\":{rect.height}");
                return true;
            }

            if (obj is Bounds bounds)
            {
                sb.Append($"\"center\":{{\"x\":{bounds.center.x},\"y\":{bounds.center.y},\"z\":{bounds.center.z}}},");
                sb.Append($"\"size\":{{\"x\":{bounds.size.x},\"y\":{bounds.size.y},\"z\":{bounds.size.z}}}");
                return true;
            }

            if (obj is Matrix4x4 matrix)
            {
                sb.Append("\"m\":[");
                for (int i = 0; i < 16; i++)
                {
                    sb.Append(matrix[i]);
                    if (i < 15) sb.Append(",");
                }

                sb.Append("]");
                return true;
            }

            if (obj is AnimationCurve curve)
            {
                sb.Append("\"keys\":[");
                for (int i = 0; i < curve.keys.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    var k = curve.keys[i];
                    sb.Append(
                        $"{{\"time\":{k.time},\"value\":{k.value},\"inTangent\":{k.inTangent},\"outTangent\":{k.outTangent}}}");
                }

                sb.Append("]");
                return true;
            }

            if (obj is Gradient gradient)
            {
                sb.Append("\"colorKeys\":[");
                for (int i = 0; i < gradient.colorKeys.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    var ck = gradient.colorKeys[i];
                    sb.Append(
                        $"{{\"color\":{{\"r\":{ck.color.r},\"g\":{ck.color.g},\"b\":{ck.color.b},\"a\":{ck.color.a}}},\"time\":{ck.time}}}");
                }

                sb.Append("],\"alphaKeys\":[");
                for (int i = 0; i < gradient.alphaKeys.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    var ak = gradient.alphaKeys[i];
                    sb.Append($"{{\"alpha\":{ak.alpha},\"time\":{ak.time}}}");
                }

                sb.Append("]");
                return true;
            }

            if (obj is LayerMask layerMask)
            {
                sb.Append($"\"value\":{layerMask.value}");
                return true;
            }

            return false;
        }

        internal static bool DeserializeUnityType(Type type, Dictionary<string, string> json, out object result)
        {
            result = null;

            if (type == typeof(Vector2))
            {
                result = new Vector2(
                    float.Parse(json["x"]),
                    float.Parse(json["y"])
                );
                return true;
            }

            if (type == typeof(Vector3))
            {
                result = new Vector3(
                    float.Parse(json["x"]),
                    float.Parse(json["y"]),
                    float.Parse(json["z"])
                );
                return true;
            }

            if (type == typeof(Vector4))
            {
                result = new Vector4(
                    float.Parse(json["x"]),
                    float.Parse(json["y"]),
                    float.Parse(json["z"]),
                    float.Parse(json["w"])
                );
                return true;
            }

            if (type == typeof(Quaternion))
            {
                result = new Quaternion(
                    float.Parse(json["x"]),
                    float.Parse(json["y"]),
                    float.Parse(json["z"]),
                    float.Parse(json["w"])
                );
                return true;
            }

            if (type == typeof(Color))
            {
                result = new Color(
                    float.Parse(json["r"]),
                    float.Parse(json["g"]),
                    float.Parse(json["b"]),
                    float.Parse(json["a"])
                );
                return true;
            }

            if (type == typeof(Color32))
            {
                result = new Color32(
                    byte.Parse(json["r"]),
                    byte.Parse(json["g"]),
                    byte.Parse(json["b"]),
                    byte.Parse(json["a"])
                );
                return true;
            }

            if (type == typeof(Rect))
            {
                result = new Rect(
                    float.Parse(json["x"]),
                    float.Parse(json["y"]),
                    float.Parse(json["width"]),
                    float.Parse(json["height"])
                );
                return true;
            }

            if (type == typeof(Bounds))
            {
                Vector3 center = new Vector3(
                    float.Parse(json["center.x"]),
                    float.Parse(json["center.y"]),
                    float.Parse(json["center.z"])
                );
                Vector3 size = new Vector3(
                    float.Parse(json["size.x"]),
                    float.Parse(json["size.y"]),
                    float.Parse(json["size.z"])
                );
                result = new Bounds(center, size);
                return true;
            }

            if (type == typeof(Matrix4x4))
            {
                Matrix4x4 matrix = new Matrix4x4();
                for (int i = 0; i < 16; i++)
                {
                    matrix[i] = float.Parse(json[$"m{i}"]);
                }

                result = matrix;
                return true;
            }

            if (type == typeof(AnimationCurve))
            {
                AnimationCurve curve = new AnimationCurve();
                int keyCount = int.Parse(json["keyCount"]);
                for (int i = 0; i < keyCount; i++)
                {
                    curve.AddKey(new Keyframe(
                        float.Parse(json[$"key{i}.time"]),
                        float.Parse(json[$"key{i}.value"]),
                        float.Parse(json[$"key{i}.inTangent"]),
                        float.Parse(json[$"key{i}.outTangent"])
                    ));
                }

                result = curve;
                return true;
            }

            if (type == typeof(Gradient))
            {
                Gradient gradient = new Gradient();
                int colorKeyCount = int.Parse(json["colorKeyCount"]);
                int alphaKeyCount = int.Parse(json["alphaKeyCount"]);

                GradientColorKey[] colorKeys = new GradientColorKey[colorKeyCount];
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[alphaKeyCount];

                for (int i = 0; i < colorKeyCount; i++)
                {
                    colorKeys[i] = new GradientColorKey(
                        new Color(
                            float.Parse(json[$"colorKey{i}.r"]),
                            float.Parse(json[$"colorKey{i}.g"]),
                            float.Parse(json[$"colorKey{i}.b"]),
                            float.Parse(json[$"colorKey{i}.a"])
                        ),
                        float.Parse(json[$"colorKey{i}.time"])
                    );
                }

                for (int i = 0; i < alphaKeyCount; i++)
                {
                    alphaKeys[i] = new GradientAlphaKey(
                        float.Parse(json[$"alphaKey{i}.alpha"]),
                        float.Parse(json[$"alphaKey{i}.time"])
                    );
                }

                gradient.SetKeys(colorKeys, alphaKeys);
                result = gradient;
                return true;
            }

            if (type == typeof(LayerMask))
            {
                result = (LayerMask)int.Parse(json["value"]);
                return true;
            }

            return false;
        }
    }
}