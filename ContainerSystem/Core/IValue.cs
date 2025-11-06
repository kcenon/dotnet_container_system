/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

namespace ContainerSystem.Core;

/// <summary>
/// Interface for all values stored in the container system.
/// Defines the contract that all value types must implement.
/// </summary>
public interface IValue
{
    /// <summary>
    /// Gets or sets the name of this value.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets the type of this value.
    /// </summary>
    ValueTypes Type { get; }

    /// <summary>
    /// Gets the parent value (if nested).
    /// </summary>
    Value? Parent { get; }

    /// <summary>
    /// Gets the number of child values.
    /// </summary>
    int ChildCount { get; }

    /// <summary>
    /// Checks if this value is null.
    /// </summary>
    bool IsNull();

    /// <summary>
    /// Checks if this value is binary data.
    /// </summary>
    bool IsBytes();

    /// <summary>
    /// Checks if this value is boolean.
    /// </summary>
    bool IsBoolean();

    /// <summary>
    /// Checks if this value is numeric.
    /// </summary>
    bool IsNumeric();

    /// <summary>
    /// Checks if this value is a string.
    /// </summary>
    bool IsString();

    /// <summary>
    /// Checks if this value is a container.
    /// </summary>
    bool IsContainer();

    /// <summary>
    /// Gets the raw data as string.
    /// </summary>
    string Data();

    /// <summary>
    /// Gets the size of the data in bytes.
    /// </summary>
    int Size();

    /// <summary>
    /// Serializes this value to a byte array.
    /// </summary>
    byte[] Serialize();

    /// <summary>
    /// Serializes this value to JSON format.
    /// </summary>
    string ToJson();

    /// <summary>
    /// Serializes this value to XML format.
    /// </summary>
    string ToXml();

    /// <summary>
    /// Converts to boolean.
    /// </summary>
    bool ToBoolean();

    /// <summary>
    /// Converts to short integer.
    /// </summary>
    short ToShort();

    /// <summary>
    /// Converts to unsigned short integer.
    /// </summary>
    ushort ToUShort();

    /// <summary>
    /// Converts to integer.
    /// </summary>
    int ToInt();

    /// <summary>
    /// Converts to unsigned integer.
    /// </summary>
    uint ToUInt();

    /// <summary>
    /// Converts to long integer.
    /// </summary>
    long ToLong();

    /// <summary>
    /// Converts to unsigned long integer.
    /// </summary>
    ulong ToULong();

    /// <summary>
    /// Converts to float.
    /// </summary>
    float ToFloat();

    /// <summary>
    /// Converts to double.
    /// </summary>
    double ToDouble();

    /// <summary>
    /// Converts to string.
    /// </summary>
    new string ToString();

    /// <summary>
    /// Converts to byte array.
    /// </summary>
    byte[] ToBytes();
}
