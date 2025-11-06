/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;

namespace ContainerSystem.Utilities;

/// <summary>
/// Builder pattern implementation for creating ValueContainer instances.
/// Provides a fluent API for constructing complex message containers.
/// </summary>
/// <example>
/// <code>
/// var container = new MessageContainerBuilder("user_profile")
///     .WithSource("client_app", "session_123")
///     .WithTarget("server_api", "profile_handler")
///     .WithVersion("2.0.0")
///     .AddValue(new StringValue("username", "john"))
///     .AddValue(new IntValue("age", 30))
///     .Build();
/// </code>
/// </example>
public class MessageContainerBuilder
{
    private readonly ValueContainer _container;

    /// <summary>
    /// Creates a new builder with the specified message type.
    /// </summary>
    /// <param name="messageType">The type of message to create</param>
    public MessageContainerBuilder(string messageType)
    {
        _container = new ValueContainer
        {
            MessageType = messageType
        };
    }

    /// <summary>
    /// Creates a new builder from an existing container.
    /// Useful for modifying existing containers.
    /// </summary>
    /// <param name="container">The container to build from</param>
    public MessageContainerBuilder(ValueContainer container)
    {
        _container = container;
    }

    /// <summary>
    /// Sets the source IDs for the message.
    /// </summary>
    /// <param name="sourceId">Main source identifier</param>
    /// <param name="sourceSubId">Optional source sub-identifier</param>
    /// <returns>This builder for method chaining</returns>
    public MessageContainerBuilder WithSource(string sourceId, string sourceSubId = "")
    {
        _container.SetSource(sourceId, sourceSubId);
        return this;
    }

    /// <summary>
    /// Sets the target IDs for the message.
    /// </summary>
    /// <param name="targetId">Main target identifier</param>
    /// <param name="targetSubId">Optional target sub-identifier</param>
    /// <returns>This builder for method chaining</returns>
    public MessageContainerBuilder WithTarget(string targetId, string targetSubId = "")
    {
        _container.SetTarget(targetId, targetSubId);
        return this;
    }

    /// <summary>
    /// Sets the protocol version.
    /// </summary>
    /// <param name="version">Version string (e.g., "2.0.0")</param>
    /// <returns>This builder for method chaining</returns>
    public MessageContainerBuilder WithVersion(string version)
    {
        // Note: ValueContainer.Version is read-only, would need constructor support
        // For now, this is a placeholder for when Version becomes settable
        return this;
    }

    /// <summary>
    /// Sets the message type.
    /// </summary>
    /// <param name="messageType">The message type</param>
    /// <returns>This builder for method chaining</returns>
    public MessageContainerBuilder WithMessageType(string messageType)
    {
        _container.MessageType = messageType;
        return this;
    }

    /// <summary>
    /// Adds a value to the container.
    /// </summary>
    /// <param name="value">The value to add</param>
    /// <returns>This builder for method chaining</returns>
    public MessageContainerBuilder AddValue(Value value)
    {
        _container.Add(value);
        return this;
    }

    /// <summary>
    /// Adds multiple values to the container.
    /// </summary>
    /// <param name="values">The values to add</param>
    /// <returns>This builder for method chaining</returns>
    public MessageContainerBuilder AddValues(params Value[] values)
    {
        foreach (var value in values)
        {
            _container.Add(value);
        }
        return this;
    }

    /// <summary>
    /// Adds multiple values from an enumerable.
    /// </summary>
    /// <param name="values">The values to add</param>
    /// <returns>This builder for method chaining</returns>
    public MessageContainerBuilder AddValues(IEnumerable<Value> values)
    {
        foreach (var value in values)
        {
            _container.Add(value);
        }
        return this;
    }

    /// <summary>
    /// Conditionally adds a value based on a predicate.
    /// </summary>
    /// <param name="condition">Condition to check</param>
    /// <param name="value">Value to add if condition is true</param>
    /// <returns>This builder for method chaining</returns>
    public MessageContainerBuilder AddValueIf(bool condition, Value value)
    {
        if (condition)
        {
            _container.Add(value);
        }
        return this;
    }

    /// <summary>
    /// Applies a custom configuration action to the container.
    /// </summary>
    /// <param name="configure">Configuration action</param>
    /// <returns>This builder for method chaining</returns>
    public MessageContainerBuilder Configure(Action<ValueContainer> configure)
    {
        configure(_container);
        return this;
    }

    /// <summary>
    /// Builds and returns the configured container.
    /// </summary>
    /// <returns>The configured ValueContainer</returns>
    public ValueContainer Build()
    {
        return _container;
    }

    /// <summary>
    /// Implicit conversion to ValueContainer for convenience.
    /// Allows using the builder directly where a container is expected.
    /// </summary>
    /// <param name="builder">The builder to convert</param>
    public static implicit operator ValueContainer(MessageContainerBuilder builder)
    {
        return builder.Build();
    }
}

/// <summary>
/// Extension methods for ValueContainer to enable builder pattern.
/// </summary>
public static class ValueContainerBuilderExtensions
{
    /// <summary>
    /// Creates a builder from an existing container.
    /// </summary>
    /// <param name="container">The container</param>
    /// <returns>A new builder initialized with the container</returns>
    public static MessageContainerBuilder ToBuilder(this ValueContainer container)
    {
        return new MessageContainerBuilder(container);
    }

    /// <summary>
    /// Fluent method to add a value and return the container.
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="value">Value to add</param>
    /// <returns>The container for method chaining</returns>
    public static ValueContainer AddAndReturn(this ValueContainer container, Value value)
    {
        container.Add(value);
        return container;
    }
}
