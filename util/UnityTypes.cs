using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.jsch.UnityUtil
{
    public static class UnityTypes
    {
        public static bool SerializeUnityType(object obj, out Dictionary<string, object> result)
        {
            result = new Dictionary<string, object>();

            if (obj is Vector2 vector2)
            {
                result["x"] = vector2.x;
                result["y"] = vector2.y;
                return true;
            }

            if (obj is Vector3 vector3)
            {
                result["x"] = vector3.x;
                result["y"] = vector3.y;
                result["z"] = vector3.z;
                return true;
            }

            if (obj is Vector4 vector4)
            {
                result["x"] = vector4.x;
                result["y"] = vector4.y;
                result["z"] = vector4.z;
                result["w"] = vector4.w;
                return true;
            }

            if (obj is Quaternion quaternion)
            {
                result["x"] = quaternion.x;
                result["y"] = quaternion.y;
                result["z"] = quaternion.z;
                result["w"] = quaternion.w;
                return true;
            }

            if (obj is Color color)
            {
                result["r"] = color.r;
                result["g"] = color.g;
                result["b"] = color.b;
                result["a"] = color.a;
                return true;
            }

            if (obj is Color32 color32)
            {
                result["r"] = color32.r;
                result["g"] = color32.g;
                result["b"] = color32.b;
                result["a"] = color32.a;
                return true;
            }

            if (obj is Rect rect)
            {
                result["x"] = rect.x;
                result["y"] = rect.y;
                result["width"] = rect.width;
                result["height"] = rect.height;
                return true;
            }

            if (obj is Bounds bounds)
            {
                result["center"] = new Dictionary<string, object>
                {
                    ["x"] = bounds.center.x,
                    ["y"] = bounds.center.y,
                    ["z"] = bounds.center.z
                };
                result["size"] = new Dictionary<string, object>
                {
                    ["x"] = bounds.size.x,
                    ["y"] = bounds.size.y,
                    ["z"] = bounds.size.z
                };
                return true;
            }

            if (obj is Matrix4x4 matrix)
            {
                result["m"] = new float[16];
                for (int i = 0; i < 16; i++)
                {
                    ((float[])result["m"])[i] = matrix[i];
                }
                return true;
            }

            if (obj is AnimationCurve curve)
            {
                var keys = new List<Dictionary<string, object>>();
                foreach (var k in curve.keys)
                {
                    keys.Add(new Dictionary<string, object>
                    {
                        ["time"] = k.time,
                        ["value"] = k.value,
                        ["inTangent"] = k.inTangent,
                        ["outTangent"] = k.outTangent
                    });
                }
                result["keys"] = keys;
                return true;
            }

            if (obj is Gradient gradient)
            {
                var colorKeys = new List<Dictionary<string, object>>();
                var alphaKeys = new List<Dictionary<string, object>>();
                
                foreach (var ck in gradient.colorKeys)
                {
                    colorKeys.Add(new Dictionary<string, object>
                    {
                        ["color"] = new Dictionary<string, object>
                        {
                            ["r"] = ck.color.r,
                            ["g"] = ck.color.g,
                            ["b"] = ck.color.b,
                            ["a"] = ck.color.a
                        },
                        ["time"] = ck.time
                    });
                }
                
                foreach (var ak in gradient.alphaKeys)
                {
                    alphaKeys.Add(new Dictionary<string, object>
                    {
                        ["alpha"] = ak.alpha,
                        ["time"] = ak.time
                    });
                }
                
                result["colorKeys"] = colorKeys;
                result["alphaKeys"] = alphaKeys;
                return true;
            }

            if (obj is LayerMask layerMask)
            {
                result["value"] = layerMask.value;
                return true;
            }

            return false;
        }

        public static bool DeserializeUnityType(Type type, Dictionary<string, object> dict, out object result)
        {
            result = null;

            if (type == typeof(Vector2))
            {
                result = new Vector2(
                    Convert.ToSingle(dict["x"]),
                    Convert.ToSingle(dict["y"])
                );
                return true;
            }

            if (type == typeof(Vector3))
            {
                result = new Vector3(
                    Convert.ToSingle(dict["x"]),
                    Convert.ToSingle(dict["y"]),
                    Convert.ToSingle(dict["z"])
                );
                return true;
            }

            if (type == typeof(Vector4))
            {
                result = new Vector4(
                    Convert.ToSingle(dict["x"]),
                    Convert.ToSingle(dict["y"]),
                    Convert.ToSingle(dict["z"]),
                    Convert.ToSingle(dict["w"])
                );
                return true;
            }

            if (type == typeof(Quaternion))
            {
                result = new Quaternion(
                    Convert.ToSingle(dict["x"]),
                    Convert.ToSingle(dict["y"]),
                    Convert.ToSingle(dict["z"]),
                    Convert.ToSingle(dict["w"])
                );
                return true;
            }

            if (type == typeof(Color))
            {
                result = new Color(
                    Convert.ToSingle(dict["r"]),
                    Convert.ToSingle(dict["g"]),
                    Convert.ToSingle(dict["b"]),
                    Convert.ToSingle(dict["a"])
                );
                return true;
            }

            if (type == typeof(Color32))
            {
                result = new Color32(
                    Convert.ToByte(dict["r"]),
                    Convert.ToByte(dict["g"]),
                    Convert.ToByte(dict["b"]),
                    Convert.ToByte(dict["a"])
                );
                return true;
            }

            if (type == typeof(Rect))
            {
                result = new Rect(
                    Convert.ToSingle(dict["x"]),
                    Convert.ToSingle(dict["y"]),
                    Convert.ToSingle(dict["width"]),
                    Convert.ToSingle(dict["height"])
                );
                return true;
            }

            if (type == typeof(Bounds))
            {
                var center = (Dictionary<string, object>)dict["center"];
                var size = (Dictionary<string, object>)dict["size"];
                result = new Bounds(
                    new Vector3(
                        Convert.ToSingle(center["x"]),
                        Convert.ToSingle(center["y"]),
                        Convert.ToSingle(center["z"])
                    ),
                    new Vector3(
                        Convert.ToSingle(size["x"]),
                        Convert.ToSingle(size["y"]),
                        Convert.ToSingle(size["z"])
                    )
                );
                return true;
            }

            if (type == typeof(Matrix4x4))
            {
                Matrix4x4 matrix = new Matrix4x4();
                float[] values = ((List<object>)dict["m"]).ConvertAll(v => Convert.ToSingle(v)).ToArray();
                for (int i = 0; i < 16; i++)
                {
                    matrix[i] = values[i];
                }
                result = matrix;
                return true;
            }

            if (type == typeof(AnimationCurve))
            {
                AnimationCurve curve = new AnimationCurve();
                var keys = (List<object>)dict["keys"];
                foreach (Dictionary<string, object> k in keys)
                {
                    curve.AddKey(new Keyframe(
                        Convert.ToSingle(k["time"]),
                        Convert.ToSingle(k["value"]),
                        Convert.ToSingle(k["inTangent"]),
                        Convert.ToSingle(k["outTangent"])
                    ));
                }
                result = curve;
                return true;
            }

            if (type == typeof(Gradient))
            {
                Gradient gradient = new Gradient();
                var colorKeys = (List<object>)dict["colorKeys"];
                var alphaKeys = (List<object>)dict["alphaKeys"];
                
                GradientColorKey[] ck = new GradientColorKey[colorKeys.Count];
                for (int i = 0; i < colorKeys.Count; i++)
                {
                    var key = (Dictionary<string, object>)colorKeys[i];
                    var color = (Dictionary<string, object>)key["color"];
                    ck[i] = new GradientColorKey(
                        new Color(
                            Convert.ToSingle(color["r"]),
                            Convert.ToSingle(color["g"]),
                            Convert.ToSingle(color["b"]),
                            Convert.ToSingle(color["a"])
                        ),
                        Convert.ToSingle(key["time"])
                    );
                }

                GradientAlphaKey[] ak = new GradientAlphaKey[alphaKeys.Count];
                for (int i = 0; i < alphaKeys.Count; i++)
                {
                    var key = (Dictionary<string, object>)alphaKeys[i];
                    ak[i] = new GradientAlphaKey(
                        Convert.ToSingle(key["alpha"]),
                        Convert.ToSingle(key["time"])
                    );
                }

                gradient.SetKeys(ck, ak);
                result = gradient;
                return true;
            }

            if (type == typeof(LayerMask))
            {
                result = (LayerMask)Convert.ToInt32(dict["value"]);
                return true;
            }

            return false;
        }
    }
}
