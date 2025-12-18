/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;

namespace ContainerSystem.Messaging;

/// <summary>
/// Fluent builder for creating <see cref="ValueContainer"/> instances.
/// Provides an idiomatic .NET API that aligns with the C++ architecture.
///
/// Usage:
/// <code>
/// var container = new ContainerBuilder()
///     .WithSource("client", "session1")
///     .WithTarget("server", "handler1")
///     .WithMessageType("request")
///     .WithValue(new StringValue("action", "login"))
///     .Build();
/// </code>
/// </summary>
public class ContainerBuilder
{
    private string _messageType;
    private string _version;
    private string _sourceId;
    private string _sourceSubId;
    private string _targetId;
    private string _targetSubId;
    private bool _threadSafeEnabled;
    private readonly List<Value> _values;

    /// <summary>
    /// Initializes a new instance of <see cref="ContainerBuilder"/>.
    /// </summary>
    public ContainerBuilder()
    {
        _messageType = "data_container";
        _version = "1.0.0.0";
        _sourceId = string.Empty;
        _sourceSubId = string.Empty;
        _targetId = string.Empty;
        _targetSubId = string.Empty;
        _threadSafeEnabled = false;
        _values = new List<Value>();
    }

    #region Source/Target Configuration

    /// <summary>
    /// Sets the source identifier for the container.
    /// </summary>
    /// <param name="id">The source ID</param>
    /// <param name="subId">The source sub-ID (optional)</param>
    /// <returns>This builder instance for method chaining</returns>
    public ContainerBuilder WithSource(string id, string subId = "")
    {
        _sourceId = id ?? throw new ArgumentNullException(nameof(id));
        _sourceSubId = subId ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Sets the target identifier for the container.
    /// </summary>
    /// <param name="id">The target ID</param>
    /// <param name="subId">The target sub-ID (optional)</param>
    /// <returns>This builder instance for method chaining</returns>
    public ContainerBuilder WithTarget(string id, string subId = "")
    {
        _targetId = id ?? throw new ArgumentNullException(nameof(id));
        _targetSubId = subId ?? string.Empty;
        return this;
    }

    #endregion

    #region Metadata Configuration

    /// <summary>
    /// Sets the message type for the container.
    /// </summary>
    /// <param name="messageType">The message type identifier</param>
    /// <returns>This builder instance for method chaining</returns>
    public ContainerBuilder WithMessageType(string messageType)
    {
        _messageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
        return this;
    }

    /// <summary>
    /// Sets the version for the container.
    /// </summary>
    /// <param name="version">The version string</param>
    /// <returns>This builder instance for method chaining</returns>
    public ContainerBuilder WithVersion(string version)
    {
        _version = version ?? throw new ArgumentNullException(nameof(version));
        return this;
    }

    #endregion

    #region Value Configuration

    /// <summary>
    /// Adds a value to the container.
    /// </summary>
    /// <param name="value">The value to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public ContainerBuilder WithValue(Value value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _values.Add(value);
        return this;
    }

    /// <summary>
    /// Adds multiple values to the container.
    /// </summary>
    /// <param name="values">The values to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public ContainerBuilder WithValues(IEnumerable<Value> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values.AddRange(values);
        return this;
    }

    /// <summary>
    /// Adds multiple values to the container.
    /// </summary>
    /// <param name="values">The values to add</param>
    /// <returns>This builder instance for method chaining</returns>
    public ContainerBuilder WithValues(params Value[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values.AddRange(values);
        return this;
    }

    #endregion

    #region Thread Safety Configuration

    /// <summary>
    /// Enables thread-safe mode for the built container.
    /// </summary>
    /// <returns>This builder instance for method chaining</returns>
    public ContainerBuilder WithThreadSafety()
    {
        _threadSafeEnabled = true;
        return this;
    }

    /// <summary>
    /// Sets the thread-safe mode for the built container.
    /// </summary>
    /// <param name="enabled">True to enable thread safety</param>
    /// <returns>This builder instance for method chaining</returns>
    public ContainerBuilder WithThreadSafety(bool enabled)
    {
        _threadSafeEnabled = enabled;
        return this;
    }

    #endregion

    #region Build

    /// <summary>
    /// Builds and returns the configured <see cref="ValueContainer"/>.
    /// </summary>
    /// <returns>A new <see cref="ValueContainer"/> instance with the configured settings</returns>
    public ValueContainer Build()
    {
        var container = new ValueContainer(
            _sourceId,
            _sourceSubId,
            _targetId,
            _targetSubId,
            _messageType,
            _version);

        foreach (var value in _values)
        {
            container.Add(value);
        }

        if (_threadSafeEnabled)
        {
            container.EnableThreadSafety();
        }

        return container;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Creates a new builder instance.
    /// </summary>
    /// <returns>A new <see cref="ContainerBuilder"/> instance</returns>
    public static ContainerBuilder Create() => new();

    /// <summary>
    /// Creates a builder from an existing container (for modification).
    /// </summary>
    /// <param name="container">The container to copy settings from</param>
    /// <returns>A new builder with the container's settings</returns>
    public static ContainerBuilder FromContainer(ValueContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        var builder = new ContainerBuilder()
            .WithSource(container.SourceId, container.SourceSubId)
            .WithTarget(container.TargetId, container.TargetSubId)
            .WithMessageType(container.MessageType)
            .WithVersion(container.Version);

        if (container.IsThreadSafe)
        {
            builder.WithThreadSafety();
        }

        foreach (var value in container)
        {
            builder.WithValue(value);
        }

        return builder;
    }

    /// <summary>
    /// Creates a request container builder with common defaults.
    /// </summary>
    /// <param name="sourceId">The source ID</param>
    /// <param name="targetId">The target ID</param>
    /// <returns>A builder configured for request messages</returns>
    public static ContainerBuilder CreateRequest(string sourceId, string targetId)
    {
        return new ContainerBuilder()
            .WithMessageType("request")
            .WithSource(sourceId)
            .WithTarget(targetId);
    }

    /// <summary>
    /// Creates a response container builder with common defaults.
    /// </summary>
    /// <param name="sourceId">The source ID</param>
    /// <param name="targetId">The target ID</param>
    /// <returns>A builder configured for response messages</returns>
    public static ContainerBuilder CreateResponse(string sourceId, string targetId)
    {
        return new ContainerBuilder()
            .WithMessageType("response")
            .WithSource(sourceId)
            .WithTarget(targetId);
    }

    #endregion
}
