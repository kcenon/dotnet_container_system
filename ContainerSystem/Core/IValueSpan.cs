/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

namespace ContainerSystem.Core;

/// <summary>
/// Interface for values that support zero-copy serialization using Span&lt;T&gt;.
/// Provides high-performance alternatives to byte array allocation.
/// </summary>
/// <remarks>
/// This interface is optional and can be implemented by value types that benefit
/// from zero-copy operations. Not all value types need to implement this interface.
/// </remarks>
public interface IValueSpan
{
    /// <summary>
    /// Attempts to serialize the value into the provided destination span.
    /// </summary>
    /// <param name="destination">The destination span to write to</param>
    /// <param name="bytesWritten">Number of bytes actually written</param>
    /// <returns>True if serialization succeeded; false if destination was too small</returns>
    bool TrySerialize(Span<byte> destination, out int bytesWritten);

    /// <summary>
    /// Gets the serialized value as a read-only span.
    /// May allocate temporary memory if the value is not already in memory.
    /// </summary>
    /// <returns>Read-only span of the serialized data</returns>
    ReadOnlySpan<byte> AsSpan();
}
