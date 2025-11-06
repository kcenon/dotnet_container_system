/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;
using ContainerSystem.Core;

namespace ContainerSystem.Values;

/// <summary>
/// Factory for creating and deserializing Value objects.
/// Implements the Factory pattern for type-safe value creation.
/// </summary>
public static class ValueFactory
{
    /// <summary>
    /// Creates a new value of the specified type with the given data.
    /// </summary>
    /// <param name="type">The type of value to create</param>
    /// <param name="name">The name/key of the value</param>
    /// <param name="data">The data for the value</param>
    /// <returns>A new Value instance</returns>
    /// <exception cref="NotSupportedException">If the type is not supported</exception>
    public static Value Create(ValueTypes type, string name, object data)
    {
        return type switch
        {
            ValueTypes.BoolValue => new BoolValue(name, Convert.ToBoolean(data)),
            ValueTypes.ShortValue => new ShortValue(name, Convert.ToInt16(data)),
            ValueTypes.UShortValue => new UShortValue(name, Convert.ToUInt16(data)),
            ValueTypes.IntValue => new IntValue(name, Convert.ToInt32(data)),
            ValueTypes.UIntValue => new UIntValue(name, Convert.ToUInt32(data)),
            ValueTypes.LongValue => new LongValue(name, Convert.ToInt64(data)),
            ValueTypes.ULongValue => new ULongValue(name, Convert.ToUInt64(data)),
            ValueTypes.LLongValue => new LLongValue(name, Convert.ToInt64(data)),
            ValueTypes.ULLongValue => new ULLongValue(name, Convert.ToUInt64(data)),
            ValueTypes.FloatValue => new FloatValue(name, Convert.ToSingle(data)),
            ValueTypes.DoubleValue => new DoubleValue(name, Convert.ToDouble(data)),
            ValueTypes.StringValue => new StringValue(name, data.ToString() ?? string.Empty),
            ValueTypes.BytesValue => new BytesValue(name, data as byte[] ?? Array.Empty<byte>()),
            ValueTypes.ContainerValue => new ContainerValue(name),
            ValueTypes.ArrayValue => new ArrayValue(name),
            _ => throw new NotSupportedException($"ValueType {type} is not supported")
        };
    }

    /// <summary>
    /// Deserializes a value from binary data.
    /// Reads the wire format: [type:1][name_len:4 LE][name:UTF-8][value_size:4 LE][value:bytes]
    /// </summary>
    /// <param name="data">Binary data to deserialize</param>
    /// <returns>Deserialized Value instance</returns>
    /// <exception cref="InvalidOperationException">If deserialization fails</exception>
    public static Value Deserialize(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        return DeserializeFromReader(reader);
    }

    /// <summary>
    /// Deserializes a value from a BinaryReader.
    /// </summary>
    /// <param name="reader">The BinaryReader to read from</param>
    /// <returns>Deserialized Value instance</returns>
    internal static Value DeserializeFromReader(BinaryReader reader)
    {
        try
        {
            // Read header: type (1 byte)
            var type = (ValueTypes)reader.ReadByte();

            // Read name length (4 bytes LE) and name (UTF-8)
            var nameLength = reader.ReadInt32();
            var nameBytes = reader.ReadBytes(nameLength);
            var name = Encoding.UTF8.GetString(nameBytes);

            // Read value size (4 bytes LE)
            var valueSize = reader.ReadInt32();

            // Read value data
            var valueData = reader.ReadBytes(valueSize);

            return DeserializeValue(type, name, valueData);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize value: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Deserializes the value data based on type.
    /// </summary>
    private static Value DeserializeValue(ValueTypes type, string name, byte[] valueData)
    {
        return type switch
        {
            ValueTypes.BoolValue => new BoolValue(name, BitConverter.ToBoolean(valueData, 0)),
            ValueTypes.ShortValue => new ShortValue(name, BitConverter.ToInt16(valueData, 0)),
            ValueTypes.UShortValue => new UShortValue(name, BitConverter.ToUInt16(valueData, 0)),
            ValueTypes.IntValue => new IntValue(name, BitConverter.ToInt32(valueData, 0)),
            ValueTypes.UIntValue => new UIntValue(name, BitConverter.ToUInt32(valueData, 0)),
            ValueTypes.FloatValue => new FloatValue(name, BitConverter.ToSingle(valueData, 0)),
            ValueTypes.DoubleValue => new DoubleValue(name, BitConverter.ToDouble(valueData, 0)),

            // Long/ULong: 32-bit serialization
            ValueTypes.LongValue => new LongValue(name, BitConverter.ToInt32(valueData, 0)),
            ValueTypes.ULongValue => new ULongValue(name, BitConverter.ToUInt32(valueData, 0)),

            // LLong/ULLong: 64-bit serialization
            ValueTypes.LLongValue => new LLongValue(name, BitConverter.ToInt64(valueData, 0)),
            ValueTypes.ULLongValue => new ULLongValue(name, BitConverter.ToUInt64(valueData, 0)),

            ValueTypes.StringValue => DeserializeString(name, valueData),
            ValueTypes.BytesValue => new BytesValue(name, valueData),
            ValueTypes.ContainerValue => DeserializeContainer(name, valueData),
            ValueTypes.ArrayValue => DeserializeArray(name, valueData),

            _ => throw new NotSupportedException($"ValueType {type} deserialization not supported")
        };
    }

    /// <summary>
    /// Deserializes a string value.
    /// </summary>
    private static StringValue DeserializeString(string name, byte[] valueData)
    {
        var str = Encoding.UTF8.GetString(valueData);
        return new StringValue(name, str);
    }

    /// <summary>
    /// Deserializes a container value with nested children.
    /// Format: [child_count:4][child1_len:4][child1_data]...
    /// </summary>
    private static ContainerValue DeserializeContainer(string name, byte[] valueData)
    {
        using var ms = new MemoryStream(valueData);
        using var reader = new BinaryReader(ms);

        var container = new ContainerValue(name);
        var childCount = reader.ReadInt32();

        for (int i = 0; i < childCount; i++)
        {
            var childLength = reader.ReadInt32();
            var childData = reader.ReadBytes(childLength);

            var child = Deserialize(childData);
            container.Add(child);
        }

        return container;
    }

    /// <summary>
    /// Deserializes an array value with elements.
    /// Format: [element_count:4][element1_data][element2_data]...
    /// </summary>
    private static ArrayValue DeserializeArray(string name, byte[] valueData)
    {
        using var ms = new MemoryStream(valueData);
        using var reader = new BinaryReader(ms);

        var elements = new List<Value>();
        var elementCount = reader.ReadInt32();

        for (int i = 0; i < elementCount; i++)
        {
            // Each element is a full serialized value (includes type, name, size)
            var element = DeserializeFromReader(reader);
            elements.Add(element);
        }

        return new ArrayValue(name, elements);
    }

    /// <summary>
    /// Creates a typed value with compile-time type safety.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="name">The name/key</param>
    /// <param name="value">The value</param>
    /// <returns>A new Value instance</returns>
    public static Value CreateTyped<T>(string name, T value)
    {
        return value switch
        {
            bool b => new BoolValue(name, b),
            short s => new ShortValue(name, s),
            ushort us => new UShortValue(name, us),
            int i => new IntValue(name, i),
            uint ui => new UIntValue(name, ui),
            long l => new LongValue(name, l),
            ulong ul => new ULongValue(name, ul),
            float f => new FloatValue(name, f),
            double d => new DoubleValue(name, d),
            string str => new StringValue(name, str),
            byte[] bytes => new BytesValue(name, bytes),
            _ => throw new NotSupportedException($"Type {typeof(T).Name} is not supported")
        };
    }

    /// <summary>
    /// Serializes a value with full wire format header.
    /// Format: [type:1][name_len:4 LE][name:UTF-8][value_size:4 LE][value:bytes]
    /// </summary>
    /// <param name="value">The value to serialize</param>
    /// <returns>Serialized data with header</returns>
    public static byte[] SerializeWithHeader(Value value)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Write type (1 byte)
        writer.Write((byte)value.Type);

        // Write name length and name (UTF-8)
        var nameBytes = Encoding.UTF8.GetBytes(value.Name);
        writer.Write(nameBytes.Length);
        writer.Write(nameBytes);

        // Serialize value data
        var valueData = value.Serialize();

        // Write value size and data
        writer.Write(valueData.Length);
        writer.Write(valueData);

        return ms.ToArray();
    }
}
