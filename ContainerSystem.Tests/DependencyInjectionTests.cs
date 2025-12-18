/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;
using ContainerSystem.DI;
using ContainerSystem.Values;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ContainerSystem.Tests;

/// <summary>
/// Tests for dependency injection integration.
///
/// Tests:
/// - Service registration via AddContainerSystem
/// - IValueContainerFactory functionality
/// - IWireProtocolSerializer functionality
/// - Options configuration
/// </summary>
public class DependencyInjectionTests
{
    // ========================================================================
    // Service Registration Tests
    // ========================================================================

    [Fact]
    public void AddContainerSystem_RegistersServices()
    {
        var services = new ServiceCollection();

        services.AddContainerSystem();

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IValueContainerFactory>());
        Assert.NotNull(provider.GetService<IWireProtocolSerializer>());
        Assert.NotNull(provider.GetService<ContainerSystemOptions>());
    }

    [Fact]
    public void AddContainerSystem_RegistersAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddContainerSystem();

        var provider = services.BuildServiceProvider();
        var factory1 = provider.GetRequiredService<IValueContainerFactory>();
        var factory2 = provider.GetRequiredService<IValueContainerFactory>();

        Assert.Same(factory1, factory2);
    }

    [Fact]
    public void AddContainerSystem_WithOptions_ConfiguresOptions()
    {
        var services = new ServiceCollection();

        services.AddContainerSystem(options =>
        {
            options.EnableThreadSafetyByDefault = true;
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<ContainerSystemOptions>();

        Assert.True(options.EnableThreadSafetyByDefault);
    }

    [Fact]
    public void AddContainerSystem_DoesNotOverwriteExistingServices()
    {
        var services = new ServiceCollection();
        var customFactory = new ValueContainerFactory();
        services.AddSingleton<IValueContainerFactory>(customFactory);

        services.AddContainerSystem();

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IValueContainerFactory>();

        Assert.Same(customFactory, factory);
    }

    // ========================================================================
    // ValueContainerFactory Tests
    // ========================================================================

    [Fact]
    public void Factory_Create_ReturnsNewContainer()
    {
        var factory = new ValueContainerFactory();

        using var container = factory.Create();

        Assert.NotNull(container);
        Assert.Equal("data_container", container.MessageType);
    }

    [Fact]
    public void Factory_CreateWithMessageType_SetsMessageType()
    {
        var factory = new ValueContainerFactory();

        using var container = factory.Create("custom_type");

        Assert.Equal("custom_type", container.MessageType);
    }

    [Fact]
    public void Factory_CreateWithFullMetadata_SetsAllProperties()
    {
        var factory = new ValueContainerFactory();

        using var container = factory.Create(
            sourceId: "client",
            sourceSubId: "session1",
            targetId: "server",
            targetSubId: "handler1",
            messageType: "request",
            version: "2.0.0.0");

        Assert.Equal("client", container.SourceId);
        Assert.Equal("session1", container.SourceSubId);
        Assert.Equal("server", container.TargetId);
        Assert.Equal("handler1", container.TargetSubId);
        Assert.Equal("request", container.MessageType);
        Assert.Equal("2.0.0.0", container.Version);
    }

    [Fact]
    public void Factory_FromJson_DeserializesContainer()
    {
        var factory = new ValueContainerFactory();
        using var original = factory.Create("test_message");
        original.Add(new StringValue("key", "value"));
        var json = original.Serialize();

        using var restored = factory.FromJson(json);

        Assert.Equal("test_message", restored.MessageType);
        Assert.Equal("value", restored.GetValue("key")?.ToString());
    }

    [Fact]
    public void Factory_FromBytes_DeserializesContainer()
    {
        var factory = new ValueContainerFactory();
        using var original = factory.Create("test_message");
        original.Add(new IntValue("number", 42));
        var bytes = original.SerializeArray();

        using var restored = factory.FromBytes(bytes);

        Assert.Equal("test_message", restored.MessageType);
        Assert.Equal(42, restored.GetValue("number")?.ToInt());
    }

    [Fact]
    public void Factory_CreateBuilder_ReturnsBuilder()
    {
        var factory = new ValueContainerFactory();

        var builder = factory.CreateBuilder();

        Assert.NotNull(builder);
    }

    [Fact]
    public void Factory_WithThreadSafetyOption_EnablesThreadSafety()
    {
        var options = new ContainerSystemOptions { EnableThreadSafetyByDefault = true };
        var factory = new ValueContainerFactory(options);

        using var container = factory.Create();

        Assert.True(container.IsThreadSafe);
    }

    [Fact]
    public void Factory_WithoutThreadSafetyOption_DisablesThreadSafety()
    {
        var options = new ContainerSystemOptions { EnableThreadSafetyByDefault = false };
        var factory = new ValueContainerFactory(options);

        using var container = factory.Create();

        Assert.False(container.IsThreadSafe);
    }

    // ========================================================================
    // WireProtocolSerializer Tests
    // ========================================================================

    [Fact]
    public void Serializer_Serialize_ReturnsWireProtocolString()
    {
        var serializer = new WireProtocolSerializer();
        using var container = new ValueContainer();
        container.Add(new StringValue("name", "test"));

        var wireData = serializer.Serialize(container);

        Assert.NotNull(wireData);
        Assert.Contains("@header={{", wireData);
        Assert.Contains("@data={{", wireData);
    }

    [Fact]
    public void Serializer_SerializeToBytes_ReturnsByteArray()
    {
        var serializer = new WireProtocolSerializer();
        using var container = new ValueContainer();
        container.Add(new IntValue("count", 100));

        var bytes = serializer.SerializeToBytes(container);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void Serializer_Deserialize_RestoresContainer()
    {
        var serializer = new WireProtocolSerializer();
        using var original = new ValueContainer();
        original.SetSource("src", "sub");
        original.Add(new DoubleValue("value", 3.14));
        var wireData = serializer.Serialize(original);

        using var restored = serializer.Deserialize(wireData);

        Assert.Equal("src", restored.SourceId);
        Assert.Equal("sub", restored.SourceSubId);
        var restoredValue = restored.GetValue("value")?.ToDouble();
        Assert.NotNull(restoredValue);
        Assert.Equal(3.14, restoredValue.Value, 2);
    }

    [Fact]
    public void Serializer_DeserializeBytes_RestoresContainer()
    {
        var serializer = new WireProtocolSerializer();
        using var original = new ValueContainer();
        original.Add(new BoolValue("flag", true));
        var bytes = serializer.SerializeToBytes(original);

        using var restored = serializer.Deserialize(bytes);

        Assert.True(restored.GetValue("flag")?.ToBoolean());
    }

    [Fact]
    public void Serializer_TryDeserialize_ReturnsTrueForValidData()
    {
        var serializer = new WireProtocolSerializer();
        using var original = new ValueContainer();
        var wireData = serializer.Serialize(original);

        var result = serializer.TryDeserialize(wireData, out var container);

        Assert.True(result);
        Assert.NotNull(container);
        container?.Dispose();
    }

    [Fact]
    public void Serializer_TryDeserialize_ReturnsFalseForInvalidData()
    {
        var serializer = new WireProtocolSerializer();

        var result = serializer.TryDeserialize("invalid data", out var container);

        Assert.False(result);
        Assert.Null(container);
    }

    // ========================================================================
    // Integration Tests
    // ========================================================================

    [Fact]
    public void Integration_FactoryAndSerializer_WorkTogether()
    {
        var services = new ServiceCollection();
        services.AddContainerSystem();
        var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IValueContainerFactory>();
        var serializer = provider.GetRequiredService<IWireProtocolSerializer>();

        using var container = factory.Create("integration_test");
        container.SetSource("client", "session");
        container.Add(new StringValue("message", "Hello, DI!"));

        var wireData = serializer.Serialize(container);
        using var restored = serializer.Deserialize(wireData);

        Assert.Equal("integration_test", restored.MessageType);
        Assert.Equal("client", restored.SourceId);
        Assert.Equal("Hello, DI!", restored.GetValue("message")?.ToString());
    }

    [Fact]
    public void Integration_BuilderFromFactory_WorksCorrectly()
    {
        var services = new ServiceCollection();
        services.AddContainerSystem(options => options.EnableThreadSafetyByDefault = true);
        var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IValueContainerFactory>();
        var builder = factory.CreateBuilder();

        using var container = builder
            .WithSource("test_client")
            .WithMessageType("builder_test")
            .WithValue(new IntValue("count", 42))
            .Build();

        Assert.True(container.IsThreadSafe);
        Assert.Equal("builder_test", container.MessageType);
        Assert.Equal(42, container.GetValue("count")?.ToInt());
    }
}
