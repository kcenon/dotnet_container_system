/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;
using ContainerSystem.Messaging;
using ContainerSystem.Values;
using Xunit;

namespace ContainerSystem.Tests;

/// <summary>
/// Tests for <see cref="ContainerBuilder"/> fluent API.
///
/// Tests:
/// - Basic fluent builder pattern
/// - Source/target configuration
/// - Message type and version configuration
/// - Value addition methods
/// - Thread safety configuration
/// - Static factory methods
/// </summary>
public class ContainerBuilderTests
{
    // ========================================================================
    // Basic Builder Tests
    // ========================================================================

    [Fact]
    public void Builder_Build_CreatesDefaultContainer()
    {
        using var container = new ContainerBuilder().Build();

        Assert.NotNull(container);
        Assert.Equal("data_container", container.MessageType);
        Assert.Equal("1.0.0.0", container.Version);
        Assert.Equal(string.Empty, container.SourceId);
        Assert.Equal(string.Empty, container.TargetId);
        Assert.Equal(0, container.Count);
    }

    [Fact]
    public void Builder_Create_ReturnsNewInstance()
    {
        var builder = ContainerBuilder.Create();
        Assert.NotNull(builder);
    }

    // ========================================================================
    // Source/Target Configuration Tests
    // ========================================================================

    [Fact]
    public void Builder_WithSource_SetsSourceId()
    {
        using var container = new ContainerBuilder()
            .WithSource("client1")
            .Build();

        Assert.Equal("client1", container.SourceId);
        Assert.Equal(string.Empty, container.SourceSubId);
    }

    [Fact]
    public void Builder_WithSource_SetsSourceIdAndSubId()
    {
        using var container = new ContainerBuilder()
            .WithSource("client1", "session1")
            .Build();

        Assert.Equal("client1", container.SourceId);
        Assert.Equal("session1", container.SourceSubId);
    }

    [Fact]
    public void Builder_WithTarget_SetsTargetId()
    {
        using var container = new ContainerBuilder()
            .WithTarget("server1")
            .Build();

        Assert.Equal("server1", container.TargetId);
        Assert.Equal(string.Empty, container.TargetSubId);
    }

    [Fact]
    public void Builder_WithTarget_SetsTargetIdAndSubId()
    {
        using var container = new ContainerBuilder()
            .WithTarget("server1", "handler1")
            .Build();

        Assert.Equal("server1", container.TargetId);
        Assert.Equal("handler1", container.TargetSubId);
    }

    [Fact]
    public void Builder_WithSourceAndTarget_ConfiguresBothEndpoints()
    {
        using var container = new ContainerBuilder()
            .WithSource("client", "session")
            .WithTarget("server", "handler")
            .Build();

        Assert.Equal("client", container.SourceId);
        Assert.Equal("session", container.SourceSubId);
        Assert.Equal("server", container.TargetId);
        Assert.Equal("handler", container.TargetSubId);
    }

    [Fact]
    public void Builder_WithSource_ThrowsOnNull()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithSource(null!));
    }

    [Fact]
    public void Builder_WithTarget_ThrowsOnNull()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithTarget(null!));
    }

    // ========================================================================
    // Metadata Configuration Tests
    // ========================================================================

    [Fact]
    public void Builder_WithMessageType_SetsMessageType()
    {
        using var container = new ContainerBuilder()
            .WithMessageType("request")
            .Build();

        Assert.Equal("request", container.MessageType);
    }

    [Fact]
    public void Builder_WithVersion_SetsVersion()
    {
        using var container = new ContainerBuilder()
            .WithVersion("2.0.0.0")
            .Build();

        Assert.Equal("2.0.0.0", container.Version);
    }

    [Fact]
    public void Builder_WithMessageType_ThrowsOnNull()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithMessageType(null!));
    }

    [Fact]
    public void Builder_WithVersion_ThrowsOnNull()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithVersion(null!));
    }

    // ========================================================================
    // Value Configuration Tests
    // ========================================================================

    [Fact]
    public void Builder_WithValue_AddsSingleValue()
    {
        using var container = new ContainerBuilder()
            .WithValue(new StringValue("name", "test"))
            .Build();

        Assert.Equal(1, container.Count);
        Assert.Equal("test", container.GetValue("name")?.ToString());
    }

    [Fact]
    public void Builder_WithValue_AddsMultipleValues()
    {
        using var container = new ContainerBuilder()
            .WithValue(new StringValue("str", "hello"))
            .WithValue(new IntValue("num", 42))
            .WithValue(new BoolValue("flag", true))
            .Build();

        Assert.Equal(3, container.Count);
        Assert.Equal("hello", container.GetValue("str")?.ToString());
        Assert.Equal(42, container.GetValue("num")?.ToInt());
        Assert.True(container.GetValue("flag")?.ToBoolean());
    }

    [Fact]
    public void Builder_WithValues_AddsEnumerable()
    {
        var values = new List<Value>
        {
            new StringValue("a", "1"),
            new StringValue("b", "2"),
            new StringValue("c", "3")
        };

        using var container = new ContainerBuilder()
            .WithValues(values)
            .Build();

        Assert.Equal(3, container.Count);
    }

    [Fact]
    public void Builder_WithValues_AddsParams()
    {
        using var container = new ContainerBuilder()
            .WithValues(
                new IntValue("x", 1),
                new IntValue("y", 2),
                new IntValue("z", 3))
            .Build();

        Assert.Equal(3, container.Count);
    }

    [Fact]
    public void Builder_WithValue_ThrowsOnNull()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithValue(null!));
    }

    [Fact]
    public void Builder_WithValues_ThrowsOnNull()
    {
        var builder = new ContainerBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithValues((IEnumerable<Value>)null!));
    }

    // ========================================================================
    // Thread Safety Configuration Tests
    // ========================================================================

    [Fact]
    public void Builder_WithThreadSafety_EnablesThreadSafeMode()
    {
        using var container = new ContainerBuilder()
            .WithThreadSafety()
            .Build();

        Assert.True(container.IsThreadSafe);
    }

    [Fact]
    public void Builder_WithThreadSafety_False_DisablesThreadSafeMode()
    {
        using var container = new ContainerBuilder()
            .WithThreadSafety(false)
            .Build();

        Assert.False(container.IsThreadSafe);
    }

    [Fact]
    public void Builder_Default_ThreadSafetyDisabled()
    {
        using var container = new ContainerBuilder().Build();
        Assert.False(container.IsThreadSafe);
    }

    // ========================================================================
    // Static Factory Method Tests
    // ========================================================================

    [Fact]
    public void Builder_FromContainer_CopiesAllSettings()
    {
        using var original = new ValueContainer(
            "source", "sourceSub",
            "target", "targetSub",
            "custom_type", "2.0.0.0");
        original.Add(new StringValue("key", "value"));
        original.EnableThreadSafety();

        var builder = ContainerBuilder.FromContainer(original);
        using var copy = builder.Build();

        Assert.Equal("source", copy.SourceId);
        Assert.Equal("sourceSub", copy.SourceSubId);
        Assert.Equal("target", copy.TargetId);
        Assert.Equal("targetSub", copy.TargetSubId);
        Assert.Equal("custom_type", copy.MessageType);
        Assert.Equal("2.0.0.0", copy.Version);
        Assert.Equal(1, copy.Count);
        Assert.True(copy.IsThreadSafe);
    }

    [Fact]
    public void Builder_FromContainer_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => ContainerBuilder.FromContainer(null!));
    }

    [Fact]
    public void Builder_CreateRequest_ConfiguresRequestDefaults()
    {
        using var container = ContainerBuilder
            .CreateRequest("client", "server")
            .Build();

        Assert.Equal("request", container.MessageType);
        Assert.Equal("client", container.SourceId);
        Assert.Equal("server", container.TargetId);
    }

    [Fact]
    public void Builder_CreateResponse_ConfiguresResponseDefaults()
    {
        using var container = ContainerBuilder
            .CreateResponse("server", "client")
            .Build();

        Assert.Equal("response", container.MessageType);
        Assert.Equal("server", container.SourceId);
        Assert.Equal("client", container.TargetId);
    }

    // ========================================================================
    // Fluent Chaining Tests
    // ========================================================================

    [Fact]
    public void Builder_SupportsFullFluentChain()
    {
        using var container = new ContainerBuilder()
            .WithSource("client", "session")
            .WithTarget("server", "handler")
            .WithMessageType("request")
            .WithVersion("1.2.3.4")
            .WithValue(new StringValue("action", "login"))
            .WithValue(new StringValue("user", "admin"))
            .WithThreadSafety()
            .Build();

        Assert.Equal("client", container.SourceId);
        Assert.Equal("session", container.SourceSubId);
        Assert.Equal("server", container.TargetId);
        Assert.Equal("handler", container.TargetSubId);
        Assert.Equal("request", container.MessageType);
        Assert.Equal("1.2.3.4", container.Version);
        Assert.Equal(2, container.Count);
        Assert.True(container.IsThreadSafe);
    }

    [Fact]
    public void Builder_CanOverwritePreviousSettings()
    {
        using var container = new ContainerBuilder()
            .WithSource("first")
            .WithSource("second")
            .WithMessageType("type1")
            .WithMessageType("type2")
            .Build();

        Assert.Equal("second", container.SourceId);
        Assert.Equal("type2", container.MessageType);
    }
}
