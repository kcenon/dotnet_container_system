/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using Xunit;
using ContainerSystem.Core;
using ContainerSystem.Values;

namespace ContainerSystem.Tests;

/// <summary>
/// Unit tests for ValueFactory class.
/// Tests factory creation, serialization, and deserialization.
/// </summary>
public class ValueFactoryTests
{
    [Fact]
    public void Create_IntValue_ReturnsCorrectType()
    {
        // Arrange & Act
        var value = ValueFactory.Create(ValueTypes.IntValue, "test", 42);

        // Assert
        Assert.IsType<IntValue>(value);
        Assert.Equal("test", value.Name);
        Assert.Equal(42, value.ToInt());
    }

    [Fact]
    public void Create_StringValue_ReturnsCorrectType()
    {
        // Arrange & Act
        var value = ValueFactory.Create(ValueTypes.StringValue, "name", "Alice");

        // Assert
        Assert.IsType<StringValue>(value);
        Assert.Equal("name", value.Name);
        Assert.Equal("Alice", value.ToString());
    }

    [Fact]
    public void Create_BoolValue_ReturnsCorrectType()
    {
        // Arrange & Act
        var value = ValueFactory.Create(ValueTypes.BoolValue, "flag", true);

        // Assert
        Assert.IsType<BoolValue>(value);
        Assert.Equal("flag", value.Name);
        Assert.True(value.ToBoolean());
    }

    [Fact]
    public void Create_DoubleValue_ReturnsCorrectType()
    {
        // Arrange & Act
        var value = ValueFactory.Create(ValueTypes.DoubleValue, "pi", 3.14159);

        // Assert
        Assert.IsType<DoubleValue>(value);
        Assert.Equal("pi", value.Name);
        Assert.Equal(3.14159, value.ToDouble(), 5);
    }

    [Fact]
    public void Create_LongValue_WithinRange_Succeeds()
    {
        // Arrange & Act
        var value = ValueFactory.Create(ValueTypes.LongValue, "timestamp", 1234567890);

        // Assert
        Assert.IsType<LongValue>(value);
        Assert.Equal("timestamp", value.Name);
        Assert.Equal(1234567890, value.ToLong());
    }

    [Fact]
    public void Create_UnsupportedType_ThrowsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            ValueFactory.Create(ValueTypes.NullValue, "null", 0));
    }

    [Fact]
    public void CreateTyped_Int_CreatesIntValue()
    {
        // Arrange & Act
        var value = ValueFactory.CreateTyped("count", 100);

        // Assert
        Assert.IsType<IntValue>(value);
        Assert.Equal(100, value.ToInt());
    }

    [Fact]
    public void CreateTyped_String_CreatesStringValue()
    {
        // Arrange & Act
        var value = ValueFactory.CreateTyped("message", "Hello World");

        // Assert
        Assert.IsType<StringValue>(value);
        Assert.Equal("Hello World", value.ToString());
    }

    [Fact]
    public void CreateTyped_Bool_CreatesBoolValue()
    {
        // Arrange & Act
        var value = ValueFactory.CreateTyped("enabled", false);

        // Assert
        Assert.IsType<BoolValue>(value);
        Assert.False(value.ToBoolean());
    }

    [Fact]
    public void CreateTyped_ByteArray_CreatesBytesValue()
    {
        // Arrange
        byte[] data = { 1, 2, 3, 4, 5 };

        // Act
        var value = ValueFactory.CreateTyped("data", data);

        // Assert
        Assert.IsType<BytesValue>(value);
        Assert.Equal(data, value.ToBytes());
    }

    [Fact]
    public void SerializeWithHeader_IntValue_IncludesAllComponents()
    {
        // Arrange
        var value = new IntValue("count", 42);

        // Act
        var serialized = ValueFactory.SerializeWithHeader(value);

        // Assert
        Assert.NotNull(serialized);
        Assert.True(serialized.Length > 0);

        // Verify format: [type:1][name_len:4][name][value_size:4][value:4]
        // Minimum size: 1 + 4 + 5 ("count") + 4 + 4 = 18 bytes
        Assert.True(serialized.Length >= 18);

        // Verify type byte
        Assert.Equal((byte)ValueTypes.IntValue, serialized[0]);
    }

    [Fact]
    public void Deserialize_IntValue_RestoresOriginal()
    {
        // Arrange
        var original = new IntValue("test", 12345);
        var serialized = ValueFactory.SerializeWithHeader(original);

        // Act
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Type, deserialized.Type);
        Assert.Equal(original.ToInt(), deserialized.ToInt());
    }

    [Fact]
    public void Deserialize_StringValue_RestoresOriginal()
    {
        // Arrange
        var original = new StringValue("message", "Hello, World!");
        var serialized = ValueFactory.SerializeWithHeader(original);

        // Act
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<StringValue>(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.ToString(), deserialized.ToString());
    }

    [Fact]
    public void Deserialize_DoubleValue_RestoresOriginal()
    {
        // Arrange
        var original = new DoubleValue("pi", 3.14159265359);
        var serialized = ValueFactory.SerializeWithHeader(original);

        // Act
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<DoubleValue>(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.ToDouble(), deserialized.ToDouble(), 10);
    }

    [Fact]
    public void Deserialize_LongValue_32BitRange_RestoresOriginal()
    {
        // Arrange
        var original = new LongValue("timestamp", 1234567890);
        var serialized = ValueFactory.SerializeWithHeader(original);

        // Act
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<LongValue>(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.ToLong(), deserialized.ToLong());
    }

    [Fact]
    public void Deserialize_BoolValue_RestoresOriginal()
    {
        // Arrange
        var original = new BoolValue("enabled", true);
        var serialized = ValueFactory.SerializeWithHeader(original);

        // Act
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<BoolValue>(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.ToBoolean(), deserialized.ToBoolean());
    }

    [Fact]
    public void Deserialize_BytesValue_RestoresOriginal()
    {
        // Arrange
        byte[] data = { 0x01, 0x02, 0x03, 0xFF, 0xFE };
        var original = new BytesValue("binary", data);
        var serialized = ValueFactory.SerializeWithHeader(original);

        // Act
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        Assert.NotNull(deserialized);
        Assert.IsType<BytesValue>(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(data, deserialized.ToBytes());
    }

    [Theory]
    [InlineData(short.MinValue)]
    [InlineData((short)0)]
    [InlineData(short.MaxValue)]
    public void RoundTrip_ShortValue_PreservesValue(short testValue)
    {
        // Arrange
        var original = new ShortValue("short_test", testValue);
        var serialized = ValueFactory.SerializeWithHeader(original);

        // Act
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        Assert.Equal(original.ToShort(), deserialized.ToShort());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1000000)]
    [InlineData(0)]
    [InlineData(1000000)]
    [InlineData(int.MaxValue)]
    public void RoundTrip_IntValue_PreservesValue(int testValue)
    {
        // Arrange
        var original = new IntValue("int_test", testValue);
        var serialized = ValueFactory.SerializeWithHeader(original);

        // Act
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        Assert.Equal(original.ToInt(), deserialized.ToInt());
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(3.14159f)]
    [InlineData(-273.15f)]
    [InlineData(float.MinValue)]
    [InlineData(float.MaxValue)]
    public void RoundTrip_FloatValue_PreservesValue(float testValue)
    {
        // Arrange
        var original = new FloatValue("float_test", testValue);
        var serialized = ValueFactory.SerializeWithHeader(original);

        // Act
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        Assert.Equal(original.ToFloat(), deserialized.ToFloat(), 6);
    }
}
