/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;
using ContainerSystem.Messaging;

namespace ContainerSystem.DI;

/// <summary>
/// Factory interface for creating <see cref="ValueContainer"/> instances.
/// Provides dependency injection support for container creation.
/// </summary>
public interface IValueContainerFactory
{
    /// <summary>
    /// Creates a new empty <see cref="ValueContainer"/>.
    /// </summary>
    /// <returns>A new <see cref="ValueContainer"/> instance</returns>
    ValueContainer Create();

    /// <summary>
    /// Creates a new <see cref="ValueContainer"/> with the specified message type.
    /// </summary>
    /// <param name="messageType">The message type identifier</param>
    /// <returns>A new <see cref="ValueContainer"/> instance</returns>
    ValueContainer Create(string messageType);

    /// <summary>
    /// Creates a new <see cref="ValueContainer"/> with full metadata specification.
    /// </summary>
    /// <param name="sourceId">The source ID</param>
    /// <param name="sourceSubId">The source sub-ID</param>
    /// <param name="targetId">The target ID</param>
    /// <param name="targetSubId">The target sub-ID</param>
    /// <param name="messageType">The message type</param>
    /// <param name="version">The version (default: "1.0.0.0")</param>
    /// <returns>A new <see cref="ValueContainer"/> instance</returns>
    ValueContainer Create(
        string sourceId,
        string sourceSubId,
        string targetId,
        string targetSubId,
        string messageType,
        string version = "1.0.0.0");

    /// <summary>
    /// Creates a new <see cref="ValueContainer"/> from a serialized JSON string.
    /// </summary>
    /// <param name="jsonData">The JSON string to deserialize</param>
    /// <returns>A deserialized <see cref="ValueContainer"/> instance</returns>
    ValueContainer FromJson(string jsonData);

    /// <summary>
    /// Creates a new <see cref="ValueContainer"/> from a byte array.
    /// </summary>
    /// <param name="data">The byte array containing serialized data</param>
    /// <returns>A deserialized <see cref="ValueContainer"/> instance</returns>
    ValueContainer FromBytes(byte[] data);

    /// <summary>
    /// Creates a new <see cref="ContainerBuilder"/> for fluent container construction.
    /// </summary>
    /// <returns>A new <see cref="ContainerBuilder"/> instance</returns>
    ContainerBuilder CreateBuilder();
}
