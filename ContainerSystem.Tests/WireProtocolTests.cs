/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;
using ContainerSystem.Values;
using Xunit;

namespace ContainerSystem.Tests;

/// <summary>
/// Tests for WireProtocol class - C++ Wire Protocol compatibility.
///
/// Tests:
/// - Basic serialization/deserialization roundtrip
/// - Header field encoding/decoding
/// - Value type encoding for all types
/// - Special character escaping
/// - Nested container handling
/// - Error handling for malformed data
/// </summary>
public class WireProtocolTests : IDisposable
{
    private readonly ValueContainer _container;

    public WireProtocolTests()
    {
        _container = new ValueContainer();
    }

    public void Dispose()
    {
        _container.Dispose();
    }

    // ========================================================================
    // Basic Serialization Tests
    // ========================================================================

    [Fact]
    public void Serialize_EmptyContainer_ProducesValidFormat()
    {
        var wireData = WireProtocol.Serialize(_container);

        Assert.Contains("@header={{", wireData);
        Assert.Contains("@data={{", wireData);
        Assert.EndsWith("}};", wireData);
    }

    [Fact]
    public void Serialize_ContainsMessageType()
    {
        _container.MessageType = "test_message";

        var wireData = WireProtocol.Serialize(_container);

        // Message type is field ID 5
        Assert.Contains("[5,test_message]", wireData);
    }

    [Fact]
    public void Serialize_ContainsSourceAndTarget()
    {
        _container.SetSource("source_client", "source_sub");
        _container.SetTarget("target_server", "target_sub");

        var wireData = WireProtocol.Serialize(_container);

        // Target = 1, Target sub = 2, Source = 3, Source sub = 4
        Assert.Contains("[1,target_server]", wireData);
        Assert.Contains("[2,target_sub]", wireData);
        Assert.Contains("[3,source_client]", wireData);
        Assert.Contains("[4,source_sub]", wireData);
    }

    // ========================================================================
    // Value Type Serialization Tests
    // ========================================================================

    [Fact]
    public void Serialize_IntValue()
    {
        _container.Add(new IntValue("count", 42));

        var wireData = WireProtocol.Serialize(_container);

        // Format: [name,type,value]
        Assert.Contains("[count,4,42]", wireData);
    }

    [Fact]
    public void Serialize_StringValue()
    {
        _container.Add(new StringValue("name", "hello"));

        var wireData = WireProtocol.Serialize(_container);

        Assert.Contains("[name,12,hello]", wireData);
    }

    [Fact]
    public void Serialize_BoolValue()
    {
        _container.Add(new BoolValue("flag", true));
        _container.Add(new BoolValue("disabled", false));

        var wireData = WireProtocol.Serialize(_container);

        Assert.Contains("[flag,1,1]", wireData);
        Assert.Contains("[disabled,1,0]", wireData);
    }

    [Fact]
    public void Serialize_DoubleValue()
    {
        _container.Add(new DoubleValue("price", 99.99));

        var wireData = WireProtocol.Serialize(_container);

        Assert.Contains("[price,11,", wireData);
    }

    [Fact]
    public void Serialize_FloatValue()
    {
        _container.Add(new FloatValue("ratio", 0.5f));

        var wireData = WireProtocol.Serialize(_container);

        Assert.Contains("[ratio,10,", wireData);
    }

    [Fact]
    public void Serialize_BytesValue()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        _container.Add(new BytesValue("data", bytes));

        var wireData = WireProtocol.Serialize(_container);

        // Bytes should be base64 encoded
        var base64 = Convert.ToBase64String(bytes);
        Assert.Contains($"[data,13,{base64}]", wireData);
    }

    // ========================================================================
    // Deserialization Tests
    // ========================================================================

    [Fact]
    public void Deserialize_ValidWireData_Success()
    {
        var wireData = "@header={{[5,test_type];[6,1.0.0.0];}}@data={{[count,4,42];}};";

        using var result = WireProtocol.Deserialize(wireData);

        Assert.Equal("test_type", result.MessageType);
    }

    [Fact]
    public void Deserialize_IntValue_CorrectValue()
    {
        var wireData = "@header={{}}@data={{[count,4,42];}};";

        using var result = WireProtocol.Deserialize(wireData);

        var value = result.GetValue("count");
        Assert.NotNull(value);
        Assert.IsType<IntValue>(value);
        Assert.Equal(42, value.ToInt());
    }

    [Fact]
    public void Deserialize_StringValue_CorrectValue()
    {
        var wireData = "@header={{}}@data={{[message,12,hello world];}};";

        using var result = WireProtocol.Deserialize(wireData);

        var value = result.GetValue("message");
        Assert.NotNull(value);
        Assert.IsType<StringValue>(value);
        Assert.Equal("hello world", value.ToString());
    }

    [Fact]
    public void Deserialize_BoolValue_True()
    {
        var wireData = "@header={{}}@data={{[enabled,1,1];}};";

        using var result = WireProtocol.Deserialize(wireData);

        var value = result.GetValue("enabled");
        Assert.NotNull(value);
        Assert.True(value.ToBoolean());
    }

    [Fact]
    public void Deserialize_BoolValue_False()
    {
        var wireData = "@header={{}}@data={{[disabled,1,0];}};";

        using var result = WireProtocol.Deserialize(wireData);

        var value = result.GetValue("disabled");
        Assert.NotNull(value);
        Assert.False(value.ToBoolean());
    }

    [Fact]
    public void Deserialize_SourceAndTarget()
    {
        var wireData = "@header={{[1,target];[2,target_sub];[3,source];[4,source_sub];}}@data={{}};";

        using var result = WireProtocol.Deserialize(wireData);

        Assert.Equal("target", result.TargetId);
        Assert.Equal("target_sub", result.TargetSubId);
        Assert.Equal("source", result.SourceId);
        Assert.Equal("source_sub", result.SourceSubId);
    }

    // ========================================================================
    // Roundtrip Tests
    // ========================================================================

    [Fact]
    public void Roundtrip_BasicContainer()
    {
        _container.SetSource("src", "src_sub");
        _container.SetTarget("tgt", "tgt_sub");
        _container.MessageType = "request";
        _container.Add(new IntValue("id", 123));
        _container.Add(new StringValue("name", "test"));

        var wireData = WireProtocol.Serialize(_container);
        using var result = WireProtocol.Deserialize(wireData);

        Assert.Equal("src", result.SourceId);
        Assert.Equal("src_sub", result.SourceSubId);
        Assert.Equal("tgt", result.TargetId);
        Assert.Equal("tgt_sub", result.TargetSubId);
        Assert.Equal("request", result.MessageType);
        Assert.Equal(123, result.GetValue("id")?.ToInt());
        Assert.Equal("test", result.GetValue("name")?.ToString());
    }

    [Fact]
    public void Roundtrip_AllNumericTypes()
    {
        _container.Add(new ShortValue("short", 100));
        _container.Add(new UShortValue("ushort", 200));
        _container.Add(new IntValue("int", 300));
        _container.Add(new UIntValue("uint", 400));
        _container.Add(new LongValue("long", 500));
        _container.Add(new ULongValue("ulong", 600));
        _container.Add(new LLongValue("llong", 7000000000L));
        _container.Add(new ULLongValue("ullong", 8000000000UL));
        _container.Add(new FloatValue("float", 1.5f));
        _container.Add(new DoubleValue("double", 2.5));

        var wireData = WireProtocol.Serialize(_container);
        using var result = WireProtocol.Deserialize(wireData);

        Assert.Equal((short)100, result.GetValue("short")?.ToShort());
        Assert.Equal((ushort)200, result.GetValue("ushort")?.ToUShort());
        Assert.Equal(300, result.GetValue("int")?.ToInt());
        Assert.Equal(400u, result.GetValue("uint")?.ToUInt());
        Assert.Equal(500, result.GetValue("long")?.ToLong());
        Assert.Equal(600u, result.GetValue("ulong")?.ToULong());
        Assert.Equal(7000000000L, result.GetValue("llong")?.ToLong());
        Assert.Equal(8000000000UL, result.GetValue("ullong")?.ToULong());
    }

    [Fact]
    public void Roundtrip_BytesValue()
    {
        var originalBytes = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE };
        _container.Add(new BytesValue("binary", originalBytes));

        var wireData = WireProtocol.Serialize(_container);
        using var result = WireProtocol.Deserialize(wireData);

        var value = result.GetValue("binary");
        Assert.NotNull(value);
        Assert.Equal(originalBytes, value.ToBytes());
    }

    // ========================================================================
    // Special Character Escaping Tests
    // ========================================================================

    [Fact]
    public void Roundtrip_SpecialCharactersInString()
    {
        _container.Add(new StringValue("special", "hello[world],test;value"));

        var wireData = WireProtocol.Serialize(_container);
        using var result = WireProtocol.Deserialize(wireData);

        Assert.Equal("hello[world],test;value", result.GetValue("special")?.ToString());
    }

    [Fact]
    public void Roundtrip_SpecialCharactersInName()
    {
        _container.SetSource("source[1]", "sub;2");

        var wireData = WireProtocol.Serialize(_container);
        using var result = WireProtocol.Deserialize(wireData);

        Assert.Equal("source[1]", result.SourceId);
        Assert.Equal("sub;2", result.SourceSubId);
    }

    // ========================================================================
    // Error Handling Tests
    // ========================================================================

    [Fact]
    public void Deserialize_NullData_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => WireProtocol.Deserialize((string)null!));
    }

    [Fact]
    public void Deserialize_EmptyData_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => WireProtocol.Deserialize(string.Empty));
    }

    [Fact]
    public void TryDeserialize_ValidData_ReturnsTrue()
    {
        var wireData = "@header={{}}@data={{[test,4,1];}};";

        var success = WireProtocol.TryDeserialize(wireData, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        result?.Dispose();
    }

    [Fact]
    public void TryDeserialize_InvalidData_ReturnsFalse()
    {
        var success = WireProtocol.TryDeserialize("invalid data", out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    // ========================================================================
    // Byte Array Serialization Tests
    // ========================================================================

    [Fact]
    public void SerializeToBytes_ProducesValidUtf8()
    {
        _container.Add(new StringValue("test", "value"));

        var bytes = WireProtocol.SerializeToBytes(_container);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        // Should be valid UTF-8
        var decoded = System.Text.Encoding.UTF8.GetString(bytes);
        Assert.Contains("@header={{", decoded);
    }

    [Fact]
    public void Deserialize_FromBytes_Success()
    {
        _container.Add(new IntValue("count", 99));

        var bytes = WireProtocol.SerializeToBytes(_container);
        using var result = WireProtocol.Deserialize(bytes);

        Assert.Equal(99, result.GetValue("count")?.ToInt());
    }

    // ========================================================================
    // C++ Compatibility Tests
    // ========================================================================

    [Fact]
    public void Serialize_MatchesCppFormat()
    {
        _container.SetTarget("server", "main");
        _container.SetSource("client", "app");
        _container.MessageType = "request";
        _container.Add(new StringValue("action", "ping"));

        var wireData = WireProtocol.Serialize(_container);

        // Verify structure matches C++ format
        Assert.StartsWith("@header={{", wireData);
        Assert.Contains("}}@data={{", wireData);
        Assert.EndsWith("}};", wireData);

        // Verify field ordering (header fields)
        Assert.Contains("[1,server]", wireData);
        Assert.Contains("[2,main]", wireData);
        Assert.Contains("[3,client]", wireData);
        Assert.Contains("[4,app]", wireData);
        Assert.Contains("[5,request]", wireData);

        // Verify data format
        Assert.Contains("[action,12,ping]", wireData);
    }

    [Fact]
    public void Deserialize_CppGeneratedData()
    {
        // Sample data as would be generated by C++ system
        var cppData = "@header={{[1,target_server];[3,source_client];[5,data_container];[6,1.0.0.0];}}@data={{[user_id,4,12345];[username,12,john_doe];[active,1,1];}};";

        using var result = WireProtocol.Deserialize(cppData);

        Assert.Equal("target_server", result.TargetId);
        Assert.Equal("source_client", result.SourceId);
        Assert.Equal("data_container", result.MessageType);
        Assert.Equal(12345, result.GetValue("user_id")?.ToInt());
        Assert.Equal("john_doe", result.GetValue("username")?.ToString());
        Assert.True(result.GetValue("active")?.ToBoolean());
    }

    [Theory]
    [InlineData(ValueTypes.BoolValue, 1)]
    [InlineData(ValueTypes.ShortValue, 2)]
    [InlineData(ValueTypes.UShortValue, 3)]
    [InlineData(ValueTypes.IntValue, 4)]
    [InlineData(ValueTypes.UIntValue, 5)]
    [InlineData(ValueTypes.LongValue, 6)]
    [InlineData(ValueTypes.ULongValue, 7)]
    [InlineData(ValueTypes.LLongValue, 8)]
    [InlineData(ValueTypes.ULLongValue, 9)]
    [InlineData(ValueTypes.FloatValue, 10)]
    [InlineData(ValueTypes.DoubleValue, 11)]
    [InlineData(ValueTypes.StringValue, 12)]
    [InlineData(ValueTypes.BytesValue, 13)]
    public void TypeIds_MatchCppDefinition(ValueTypes type, int expectedId)
    {
        Assert.Equal(expectedId, (int)type);
    }

    // ========================================================================
    // Multiple Values Tests
    // ========================================================================

    [Fact]
    public void Roundtrip_MultipleValues()
    {
        for (int i = 0; i < 10; i++)
        {
            _container.Add(new IntValue($"value_{i}", i * 10));
        }

        var wireData = WireProtocol.Serialize(_container);
        using var result = WireProtocol.Deserialize(wireData);

        Assert.Equal(10, result.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i * 10, result.GetValue($"value_{i}")?.ToInt());
        }
    }
}
