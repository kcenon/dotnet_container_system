/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;

namespace ContainerSystem.DI;

/// <summary>
/// Service interface for C++ Wire Protocol serialization/deserialization.
/// Provides dependency injection support for wire protocol operations.
/// </summary>
public interface IWireProtocolSerializer
{
    /// <summary>
    /// Serializes a <see cref="ValueContainer"/> to C++ Wire Protocol format.
    /// </summary>
    /// <param name="container">The container to serialize</param>
    /// <returns>Wire protocol string</returns>
    string Serialize(ValueContainer container);

    /// <summary>
    /// Serializes a <see cref="ValueContainer"/> to C++ Wire Protocol format as byte array.
    /// </summary>
    /// <param name="container">The container to serialize</param>
    /// <returns>Wire protocol bytes (UTF-8)</returns>
    byte[] SerializeToBytes(ValueContainer container);

    /// <summary>
    /// Deserializes a C++ Wire Protocol string to <see cref="ValueContainer"/>.
    /// </summary>
    /// <param name="wireData">The wire protocol string</param>
    /// <returns>Deserialized container</returns>
    /// <exception cref="FormatException">If wire data is malformed</exception>
    ValueContainer Deserialize(string wireData);

    /// <summary>
    /// Deserializes a C++ Wire Protocol byte array to <see cref="ValueContainer"/>.
    /// </summary>
    /// <param name="wireData">The wire protocol bytes (UTF-8)</param>
    /// <returns>Deserialized container</returns>
    ValueContainer Deserialize(byte[] wireData);

    /// <summary>
    /// Attempts to deserialize wire data without throwing exceptions.
    /// </summary>
    /// <param name="wireData">The wire protocol string</param>
    /// <param name="container">The deserialized container if successful</param>
    /// <returns>True if deserialization succeeded</returns>
    bool TryDeserialize(string wireData, out ValueContainer? container);
}
