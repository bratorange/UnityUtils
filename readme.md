# Unity Serialization Utility

This Unity package provides a robust serialization system for Unity objects, including support for various Unity-specific types and complex object structures.

## Features

- Serialize and deserialize Unity objects to/from JSON
- Support for Unity-specific types (Vector2, Vector3, Color, etc.)
- Handling of circular references
- Support for inheritance in serialized objects
- Custom editor for visualizing serialized data

## Installation

1. In Unity, go to Window > Package Manager
2. Click the "+" button and choose "Add package from git URL"
3. Enter the following URL: `https://github.com/yourusername/UnitySerializationUtility.git`

## Usage
Create a new game object and attach the `SerializationTest` script to it. This script demonstrates how to serialize and deserialize objects using the `Serializer` class.


### Basic Serialization

```csharp
using com.jsch.UnityUtil;

// Serialize an object to JSON
MyObject myObject = new MyObject();
string json = Serializer.Serialize(myObject);

// Deserialize JSON back to an object
MyObject deserializedObject = Serializer.Deserialize<MyObject>(json);
