using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace com.jsch.UnityUtil.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void TestSerializeDeserializeVector2()
        {
            Vector2 original = new Vector2(1.5f, 2.5f);
            string json = Serializer.Serialize(original);
            Vector2 deserialized = Serializer.Deserialize<Vector2>(json);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void TestSerializeDeserializeVector3()
        {
            Vector3 original = new Vector3(1.5f, 2.5f, 3.5f);
            string json = Serializer.Serialize(original);
            Vector3 deserialized = Serializer.Deserialize<Vector3>(json);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void TestSerializeDeserializeVector4()
        {
            Vector4 original = new Vector4(1.5f, 2.5f, 3.5f, 4.5f);
            string json = Serializer.Serialize(original);
            Vector4 deserialized = Serializer.Deserialize<Vector4>(json);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void TestSerializeDeserializeQuaternion()
        {
            Quaternion original = Quaternion.Euler(30, 60, 90);
            string json = Serializer.Serialize(original);
            Quaternion deserialized = Serializer.Deserialize<Quaternion>(json);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void TestSerializeDeserializeColor()
        {
            Color original = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            string json = Serializer.Serialize(original);
            Color deserialized = Serializer.Deserialize<Color>(json);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void TestSerializeDeserializeColor32()
        {
            Color32 original = new Color32(100, 150, 200, 255);
            string json = Serializer.Serialize(original);
            Color32 deserialized = Serializer.Deserialize<Color32>(json);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void TestSerializeDeserializeRect()
        {
            Rect original = new Rect(10, 20, 30, 40);
            string json = Serializer.Serialize(original);
            Rect deserialized = Serializer.Deserialize<Rect>(json);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void TestSerializeDeserializeBounds()
        {
            Bounds original = new Bounds(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
            string json = Serializer.Serialize(original);
            Bounds deserialized = Serializer.Deserialize<Bounds>(json);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void TestSerializeDeserializeMatrix4x4()
        {
            Matrix4x4 original =
                Matrix4x4.TRS(new Vector3(1, 2, 3), Quaternion.Euler(30, 60, 90), new Vector3(2, 2, 2));
            string json = Serializer.Serialize(original);
            Matrix4x4 deserialized = Serializer.Deserialize<Matrix4x4>(json);
            Assert.AreEqual(original, deserialized);
        }

        [Test]
        public void TestSerializeDeserializeAnimationCurve()
        {
            AnimationCurve original = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
            string json = Serializer.Serialize(original);
            AnimationCurve deserialized = Serializer.Deserialize<AnimationCurve>(json);
            Assert.AreEqual(original.keys.Length, deserialized.keys.Length);
            for (int i = 0; i < original.keys.Length; i++)
            {
                Assert.AreEqual(original.keys[i].time, deserialized.keys[i].time);
                Assert.AreEqual(original.keys[i].value, deserialized.keys[i].value);
            }
        }

        [Test]
        public void TestSerializeDeserializeGradient()
        {
            Gradient original = new Gradient();
            original.SetKeys(
                new GradientColorKey[]
                    { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            string json = Serializer.Serialize(original);
            Gradient deserialized = Serializer.Deserialize<Gradient>(json);
            Assert.AreEqual(original.colorKeys.Length, deserialized.colorKeys.Length);
            Assert.AreEqual(original.alphaKeys.Length, deserialized.alphaKeys.Length);
        }

        [Test]
        public void TestSerializeDeserializeLayerMask()
        {
            LayerMask original = LayerMask.GetMask("Default", "TransparentFX");
            string json = Serializer.Serialize(original);
            LayerMask deserialized = Serializer.Deserialize<LayerMask>(json);
            Assert.AreEqual(original.value, deserialized.value);
        }

        [Test]
        public void TestSerializeDeserializeRoundTripComplexScriptableObject()
        {
            ComplexScriptableObject originalObj = ScriptableObject.CreateInstance<ComplexScriptableObject>();
            originalObj.intValue = 42;
            originalObj.floatValue = 3.14f;
            originalObj.stringValue = "Hello, World!";
            originalObj.vector3Value = new Vector3(1, 2, 3);
            originalObj.colorValue = Color.red;
            originalObj.nestedObject = new NestedObject { name = "Nested", value = 100 };
            originalObj.intList = new List<int> { 1, 2, 3, 4, 5 };

            string json = Serializer.Serialize(originalObj);
            ComplexScriptableObject deserializedObj = Serializer.Deserialize<ComplexScriptableObject>(json);

            Assert.AreEqual(originalObj.intValue, deserializedObj.intValue);
            Assert.AreEqual(originalObj.floatValue, deserializedObj.floatValue);
            Assert.AreEqual(originalObj.stringValue, deserializedObj.stringValue);
            Assert.AreEqual(originalObj.vector3Value, deserializedObj.vector3Value);
            Assert.AreEqual(originalObj.colorValue, deserializedObj.colorValue);
            Assert.AreEqual(originalObj.nestedObject.name, deserializedObj.nestedObject.name);
            Assert.AreEqual(originalObj.nestedObject.value, deserializedObj.nestedObject.value);
            CollectionAssert.AreEqual(originalObj.intList, deserializedObj.intList);
        }

        [Test]
        public void TestSerializeDeserializeRoundTripWithNullValues()
        {
            ComplexScriptableObject originalObj = ScriptableObject.CreateInstance<ComplexScriptableObject>();
            originalObj.stringValue = null;
            originalObj.nestedObject = null;
            originalObj.intList = null;

            string json = Serializer.Serialize(originalObj);
            ComplexScriptableObject deserializedObj = Serializer.Deserialize<ComplexScriptableObject>(json);

            Assert.IsNull(deserializedObj.stringValue);
            Assert.IsNull(deserializedObj.nestedObject);
            Assert.IsNull(deserializedObj.intList);
        }

        [Test]
        public void TestSerializeDeserializeRoundTripWithEmptyList()
        {
            ComplexScriptableObject originalObj = ScriptableObject.CreateInstance<ComplexScriptableObject>();
            originalObj.intList = new List<int>();

            string json = Serializer.Serialize(originalObj);
            ComplexScriptableObject deserializedObj = Serializer.Deserialize<ComplexScriptableObject>(json);

            Assert.IsNotNull(deserializedObj.intList);
            Assert.AreEqual(0, deserializedObj.intList.Count);
        }

        [Test]
        public void TestSerializeDeserializeRoundTripWithNestedLists()
        {
            ComplexScriptableObject originalObj = ScriptableObject.CreateInstance<ComplexScriptableObject>();
            originalObj.nestedListObject = new NestedListObject
            {
                stringList = new List<string> { "a", "b", "c" },
                vectorList = new List<Vector3> { new Vector3(1, 2, 3), new Vector3(4, 5, 6) }
            };

            string json = Serializer.Serialize(originalObj);
            ComplexScriptableObject deserializedObj = Serializer.Deserialize<ComplexScriptableObject>(json);

            Assert.IsNotNull(deserializedObj.nestedListObject);
            CollectionAssert.AreEqual(originalObj.nestedListObject.stringList,
                deserializedObj.nestedListObject.stringList);
            CollectionAssert.AreEqual(originalObj.nestedListObject.vectorList,
                deserializedObj.nestedListObject.vectorList);
        }

        [Test]
        public void TestSerializeDeserializeRoundTripWithCircularReference()
        {
            ComplexScriptableObject originalObj = ScriptableObject.CreateInstance<ComplexScriptableObject>();
            originalObj.circularReference = new CircularReferenceObject();
            originalObj.circularReference.parent = originalObj;

            string json = Serializer.Serialize(originalObj);
            ComplexScriptableObject deserializedObj = Serializer.Deserialize<ComplexScriptableObject>(json);

            Assert.IsNotNull(deserializedObj.circularReference);
            Assert.AreSame(deserializedObj, deserializedObj.circularReference.parent);
        }

        [Test]
        public void TestSerializeDeserializeRoundTripWithInheritance()
        {
            DerivedScriptableObject originalObj = ScriptableObject.CreateInstance<DerivedScriptableObject>();
            originalObj.intValue = 42;
            originalObj.derivedValue = "Derived";

            string json = Serializer.Serialize(originalObj);
            DerivedScriptableObject deserializedObj = Serializer.Deserialize<DerivedScriptableObject>(json);

            Assert.AreEqual(originalObj.intValue, deserializedObj.intValue);
            Assert.AreEqual(originalObj.derivedValue, deserializedObj.derivedValue);
        }

        [Test]
        public void TestSerializeDeserializeEnum()
        {
            TestEnum originalEnum = TestEnum.Value2;
            string json = Serializer.Serialize(originalEnum);
            TestEnum deserializedEnum = Serializer.Deserialize<TestEnum>(json);

            Assert.AreEqual(originalEnum, deserializedEnum);
            Assert.AreEqual(TestEnum.Value2, deserializedEnum);
        }
    }

// Test classes
    public class ComplexScriptableObject : ScriptableObject
    {
        public int intValue;
        public float floatValue;
        public string stringValue;
        public Vector3 vector3Value;
        public Color colorValue;
        public NestedObject nestedObject;
        public List<int> intList;
        public NestedListObject nestedListObject;
        public CircularReferenceObject circularReference;
    }

    public class NestedObject
    {
        public string name;
        public int value;
    }

    public class NestedListObject
    {
        public List<string> stringList;
        public List<Vector3> vectorList;
    }

    public class CircularReferenceObject
    {
        public ComplexScriptableObject parent;
    }

    public class DerivedScriptableObject : ComplexScriptableObject
    {
        public string derivedValue;
    }

    public enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }
}