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

        internal static bool DeserializeUnityType(Type type, string json, out object result)
        {
            result = null;

            if (type == typeof(Vector2))
            {
                Vector2 v = JsonUtility.FromJson<Vector2>(json);
                result = v;
                return true;
            }

            if (type == typeof(Vector3))
            {
                Vector3 v = JsonUtility.FromJson<Vector3>(json);
                result = v;
                return true;
            }

            if (type == typeof(Vector4))
            {
                Vector4 v = JsonUtility.FromJson<Vector4>(json);
                result = v;
                return true;
            }

            if (type == typeof(Quaternion))
            {
                Quaternion q = JsonUtility.FromJson<Quaternion>(json);
                result = q;
                return true;
            }

            if (type == typeof(Color))
            {
                Color c = JsonUtility.FromJson<Color>(json);
                result = c;
                return true;
            }

            if (type == typeof(Color32))
            {
                Color32 c = JsonUtility.FromJson<Color32>(json);
                result = c;
                return true;
            }

            if (type == typeof(Rect))
            {
                Rect r = JsonUtility.FromJson<Rect>(json);
                result = r;
                return true;
            }

            if (type == typeof(Bounds))
            {
                Bounds b = JsonUtility.FromJson<Bounds>(json);
                result = b;
                return true;
            }

            if (type == typeof(Matrix4x4))
            {
                Matrix4x4 m = JsonUtility.FromJson<Matrix4x4>(json);
                result = m;
                return true;
            }

            if (type == typeof(AnimationCurve))
            {
                AnimationCurve curve = JsonUtility.FromJson<AnimationCurve>(json);
                result = curve;
                return true;
            }

            if (type == typeof(Gradient))
            {
                Gradient gradient = JsonUtility.FromJson<Gradient>(json);
                result = gradient;
                return true;
            }

            if (type == typeof(LayerMask))
            {
                LayerMask layerMask = JsonUtility.FromJson<LayerMask>(json);
                result = layerMask;
                return true;
            }

            return false;
        }
    }
}