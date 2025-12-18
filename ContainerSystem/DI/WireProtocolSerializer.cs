/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;

namespace ContainerSystem.DI;

/// <summary>
/// Default implementation of <see cref="IWireProtocolSerializer"/>.
/// Wraps the static <see cref="WireProtocol"/> class for dependency injection support.
/// </summary>
public class WireProtocolSerializer : IWireProtocolSerializer
{
    /// <inheritdoc/>
    public string Serialize(ValueContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);
        return WireProtocol.Serialize(container);
    }

    /// <inheritdoc/>
    public byte[] SerializeToBytes(ValueContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);
        return WireProtocol.SerializeToBytes(container);
    }

    /// <inheritdoc/>
    public ValueContainer Deserialize(string wireData)
    {
        return WireProtocol.Deserialize(wireData);
    }

    /// <inheritdoc/>
    public ValueContainer Deserialize(byte[] wireData)
    {
        ArgumentNullException.ThrowIfNull(wireData);
        return WireProtocol.Deserialize(wireData);
    }

    /// <inheritdoc/>
    public bool TryDeserialize(string wireData, out ValueContainer? container)
    {
        return WireProtocol.TryDeserialize(wireData, out container);
    }
}
