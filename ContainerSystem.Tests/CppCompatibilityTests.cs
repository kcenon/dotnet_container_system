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
/// Tests to verify compatibility between .NET and C++ container_system implementations.
///
/// Critical verifications:
/// - ValueTypes enum order matches C++ value_types
/// - Serialization format matches C++ output
/// - Type ID mappings are identical
/// </summary>
public class CppCompatibilityTests
{
    // ========================================================================
    // ValueTypes Enum Order Tests
    // Verifies that enum values match C++ value_types exactly
    // ========================================================================

    [Theory]
    [InlineData(ValueTypes.NullValue, 0)]
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
    [InlineData(ValueTypes.StringValue, 12)]  // IMPORTANT: Must be 12 to match C++
    [InlineData(ValueTypes.BytesValue, 13)]   // IMPORTANT: Must be 13 to match C++
    [InlineData(ValueTypes.ContainerValue, 14)]
    [InlineData(ValueTypes.ArrayValue, 15)]
    public void ValueTypes_MatchesCppValueTypes(ValueTypes type, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)type);
    }

    // ========================================================================
    // Type String Conversion Tests
    // Verifies bidirectional conversion matches C++ convert_value_type()
    // ========================================================================

    [Theory]
    [InlineData("0", ValueTypes.NullValue)]
    [InlineData("1", ValueTypes.BoolValue)]
    [InlineData("2", ValueTypes.ShortValue)]
    [InlineData("3", ValueTypes.UShortValue)]
    [InlineData("4", ValueTypes.IntValue)]
    [InlineData("5", ValueTypes.UIntValue)]
    [InlineData("6", ValueTypes.LongValue)]
    [InlineData("7", ValueTypes.ULongValue)]
    [InlineData("8", ValueTypes.LLongValue)]
    [InlineData("9", ValueTypes.ULLongValue)]
    [InlineData("10", ValueTypes.FloatValue)]
    [InlineData("11", ValueTypes.DoubleValue)]
    [InlineData("12", ValueTypes.StringValue)]   // string_value in C++
    [InlineData("13", ValueTypes.BytesValue)]    // bytes_value in C++
    [InlineData("14", ValueTypes.ContainerValue)]
    [InlineData("15", ValueTypes.ArrayValue)]
    public void FromTypeString_MatchesCppConversion(string typeString, ValueTypes expected)
    {
        var result = ValueTypesExtensions.FromTypeString(typeString);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ValueTypes.NullValue, "0")]
    [InlineData(ValueTypes.BoolValue, "1")]
    [InlineData(ValueTypes.StringValue, "12")]   // CRITICAL: Must be "12"
    [InlineData(ValueTypes.BytesValue, "13")]    // CRITICAL: Must be "13"
    [InlineData(ValueTypes.ContainerValue, "14")]
    public void ToTypeString_MatchesCppConversion(ValueTypes type, string expected)
    {
        var result = type.ToTypeString();
        Assert.Equal(expected, result);
    }

    // ========================================================================
    // Value Class Type Property Tests
    // ========================================================================

    [Fact]
    public void StringValue_HasCorrectTypeId()
    {
        var sv = new StringValue("test", "value");
        Assert.Equal(ValueTypes.StringValue, sv.Type);
        Assert.Equal(12, (int)sv.Type);
    }

    [Fact]
    public void BytesValue_HasCorrectTypeId()
    {
        var bv = new BytesValue("test", new byte[] { 1, 2, 3 });
        Assert.Equal(ValueTypes.BytesValue, bv.Type);
        Assert.Equal(13, (int)bv.Type);
    }

    // ========================================================================
    // JSON Serialization Format Tests
    // Verifies serialized type IDs match expected values
    // ========================================================================

    [Fact]
    public void StringValue_SerializesWithTypeId12()
    {
        var sv = new StringValue("name", "value");
        var json = sv.ToJson();

        Assert.Contains("\"type\":12", json);
    }

    [Fact]
    public void BytesValue_SerializesWithTypeId13()
    {
        var bv = new BytesValue("name", new byte[] { 1, 2, 3 });
        var json = bv.ToJson();

        Assert.Contains("\"type\":13", json);
    }

    // ========================================================================
    // Container Deserialization Tests
    // Verifies that containers can deserialize C++ generated JSON
    // ========================================================================

    [Fact]
    public void Container_DeserializesStringValueFromCppFormat()
    {
        // JSON as would be generated by C++ with string_value = 12
        var cppJson = @"{
            ""message_type"":""test"",
            ""version"":""1.0.0.0"",
            ""source_id"":"""",
            ""source_sub_id"":"""",
            ""target_id"":"""",
            ""target_sub_id"":"""",
            ""values"":[
                {""name"":""test_string"",""type"":12,""data"":""hello""}
            ]
        }";

        using var container = new ValueContainer();
        var success = container.Deserialize(cppJson);

        Assert.True(success);
        var value = container.GetValue("test_string");
        Assert.NotNull(value);
        Assert.IsType<StringValue>(value);
        Assert.Equal("hello", value.ToString());
    }

    [Fact]
    public void Container_DeserializesBytesValueFromCppFormat()
    {
        // JSON as would be generated by C++ with bytes_value = 13
        // Using base64 encoded data
        var cppJson = @"{
            ""message_type"":""test"",
            ""version"":""1.0.0.0"",
            ""source_id"":"""",
            ""source_sub_id"":"""",
            ""target_id"":"""",
            ""target_sub_id"":"""",
            ""values"":[
                {""name"":""test_bytes"",""type"":13,""data"":""AQID"",""encoding"":""base64""}
            ]
        }";

        using var container = new ValueContainer();
        var success = container.Deserialize(cppJson);

        Assert.True(success);
        var value = container.GetValue("test_bytes");
        Assert.NotNull(value);
        Assert.IsType<BytesValue>(value);
        Assert.Equal(new byte[] { 1, 2, 3 }, value.ToBytes());
    }

    // ========================================================================
    // Cross-Platform Type Size Compatibility Tests
    // ========================================================================

    [Fact]
    public void IntValue_Is4Bytes()
    {
        var iv = new IntValue("test", 12345);
        Assert.Equal(4, iv.Size());
    }

    [Fact]
    public void LongValue_Is4Bytes_MatchesCpp32Bit()
    {
        // C++ long is 32-bit on Windows, so LongValue should also be 4 bytes
        var lv = new LongValue("test", 12345);
        Assert.Equal(4, lv.Size());
    }

    [Fact]
    public void LLongValue_Is8Bytes_MatchesCpp64Bit()
    {
        // C++ long long is 64-bit, so LLongValue should be 8 bytes
        var llv = new LLongValue("test", 12345L);
        Assert.Equal(8, llv.Size());
    }

    [Fact]
    public void FloatValue_Is4Bytes()
    {
        var fv = new FloatValue("test", 1.5f);
        Assert.Equal(4, fv.Size());
    }

    [Fact]
    public void DoubleValue_Is8Bytes()
    {
        var dv = new DoubleValue("test", 1.5);
        Assert.Equal(8, dv.Size());
    }

    // ========================================================================
    // Unknown Type Handling Tests
    // ========================================================================

    [Fact]
    public void FromTypeString_UnknownReturnsNullValue()
    {
        var result = ValueTypesExtensions.FromTypeString("999");
        Assert.Equal(ValueTypes.NullValue, result);
    }

    [Fact]
    public void FromTypeString_EmptyReturnsNullValue()
    {
        var result = ValueTypesExtensions.FromTypeString("");
        Assert.Equal(ValueTypes.NullValue, result);
    }
}
