/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;
using ContainerSystem.Messaging;

namespace ContainerSystem.DI;

/// <summary>
/// Default implementation of <see cref="IValueContainerFactory"/>.
/// Provides factory methods for creating <see cref="ValueContainer"/> instances.
/// </summary>
public class ValueContainerFactory : IValueContainerFactory
{
    private readonly ContainerSystemOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="ValueContainerFactory"/>.
    /// </summary>
    public ValueContainerFactory()
        : this(new ContainerSystemOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ValueContainerFactory"/> with options.
    /// </summary>
    /// <param name="options">The container system options</param>
    public ValueContainerFactory(ContainerSystemOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public ValueContainer Create()
    {
        var container = new ValueContainer();
        ApplyOptions(container);
        return container;
    }

    /// <inheritdoc/>
    public ValueContainer Create(string messageType)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        var container = new ValueContainer();
        container.MessageType = messageType;
        ApplyOptions(container);
        return container;
    }

    /// <inheritdoc/>
    public ValueContainer Create(
        string sourceId,
        string sourceSubId,
        string targetId,
        string targetSubId,
        string messageType,
        string version = "1.0.0.0")
    {
        var container = new ValueContainer(
            sourceId,
            sourceSubId,
            targetId,
            targetSubId,
            messageType,
            version);
        ApplyOptions(container);
        return container;
    }

    /// <inheritdoc/>
    public ValueContainer FromJson(string jsonData)
    {
        ArgumentNullException.ThrowIfNull(jsonData);

        var container = new ValueContainer(jsonData);
        ApplyOptions(container);
        return container;
    }

    /// <inheritdoc/>
    public ValueContainer FromBytes(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var container = new ValueContainer(data);
        ApplyOptions(container);
        return container;
    }

    /// <inheritdoc/>
    public ContainerBuilder CreateBuilder()
    {
        var builder = new ContainerBuilder();

        if (_options.EnableThreadSafetyByDefault)
        {
            builder.WithThreadSafety();
        }

        return builder;
    }

    private void ApplyOptions(ValueContainer container)
    {
        if (_options.EnableThreadSafetyByDefault)
        {
            container.EnableThreadSafety();
        }
    }
}
