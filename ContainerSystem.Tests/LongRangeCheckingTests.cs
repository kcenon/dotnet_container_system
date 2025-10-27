/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Values;
using Xunit;

namespace ContainerSystem.Tests;

/// <summary>
/// Tests for long/ulong type range checking policy.
///
/// Tests the unified long type policy implementation:
/// - LongValue (type 6): must fit in 32-bit signed range
/// - ULongValue (type 7): must fit in 32-bit unsigned range
/// - Values exceeding range should throw OverflowException
/// </summary>
public class LongRangeCheckingTests
{
    // 32-bit boundary values
    private const int INT32_MIN = int.MinValue;       // -2147483648
    private const int INT32_MAX = int.MaxValue;        // 2147483647
    private const uint UINT32_MAX = uint.MaxValue;     // 4294967295

    // ========================================================================
    // LongValue (type 6) Tests - Signed 32-bit Range
    // ========================================================================

    [Fact]
    public void LongValue_AcceptsValidPositiveValue()
    {
        var lv = new LongValue("test", 1000000L);
        Assert.Equal(1000000L, lv.ToLong());
    }

    [Fact]
    public void LongValue_AcceptsValidNegativeValue()
    {
        var lv = new LongValue("test", -1000000L);
        Assert.Equal(-1000000L, lv.ToLong());
    }

    [Fact]
    public void LongValue_AcceptsZero()
    {
        var lv = new LongValue("test", 0L);
        Assert.Equal(0L, lv.ToLong());
    }

    [Fact]
    public void LongValue_AcceptsInt32Max()
    {
        var lv = new LongValue("test", INT32_MAX);
        Assert.Equal(INT32_MAX, lv.ToLong());
    }

    [Fact]
    public void LongValue_AcceptsInt32Min()
    {
        var lv = new LongValue("test", INT32_MIN);
        Assert.Equal(INT32_MIN, lv.ToLong());
    }

    [Fact]
    public void LongValue_RejectsInt32MaxPlusOne()
    {
        var ex = Assert.Throws<OverflowException>(() =>
            new LongValue("test", (long)INT32_MAX + 1));
        Assert.Contains("32-bit range", ex.Message);
        Assert.Contains("LLongValue", ex.Message);
    }

    [Fact]
    public void LongValue_RejectsInt32MinMinusOne()
    {
        var ex = Assert.Throws<OverflowException>(() =>
            new LongValue("test", (long)INT32_MIN - 1));
        Assert.Contains("32-bit range", ex.Message);
    }

    [Fact]
    public void LongValue_RejectsLargePositiveValue()
    {
        Assert.Throws<OverflowException>(() =>
            new LongValue("test", 5000000000L));
    }

    [Fact]
    public void LongValue_RejectsLargeNegativeValue()
    {
        Assert.Throws<OverflowException>(() =>
            new LongValue("test", -5000000000L));
    }

    // ========================================================================
    // ULongValue (type 7) Tests - Unsigned 32-bit Range
    // ========================================================================

    [Fact]
    public void ULongValue_AcceptsValidValue()
    {
        var ulv = new ULongValue("test", 1000000UL);
        Assert.Equal(1000000L, ulv.ToLong());
    }

    [Fact]
    public void ULongValue_AcceptsZero()
    {
        var ulv = new ULongValue("test", 0UL);
        Assert.Equal(0L, ulv.ToLong());
    }

    [Fact]
    public void ULongValue_AcceptsUInt32Max()
    {
        var ulv = new ULongValue("test", UINT32_MAX);
        Assert.Equal(UINT32_MAX, (uint)ulv.ToLong());
    }

    [Fact]
    public void ULongValue_RejectsUInt32MaxPlusOne()
    {
        var ex = Assert.Throws<OverflowException>(() =>
            new ULongValue("test", (ulong)UINT32_MAX + 1));
        Assert.Contains("32-bit range", ex.Message);
        Assert.Contains("ULLongValue", ex.Message);
    }

    [Fact]
    public void ULongValue_RejectsLargeValue()
    {
        Assert.Throws<OverflowException>(() =>
            new ULongValue("test", 10000000000UL));
    }

    // ========================================================================
    // Serialization Tests - Data Size Verification
    // ========================================================================

    [Fact]
    public void LongValue_SerializesAs4Bytes()
    {
        var lv = new LongValue("test", 12345L);
        var data = lv.Serialize();
        Assert.Equal(4, data.Length);
    }

    [Fact]
    public void ULongValue_SerializesAs4Bytes()
    {
        var ulv = new ULongValue("test", 12345UL);
        var data = ulv.Serialize();
        Assert.Equal(4, data.Length);
    }

    [Fact]
    public void LongValue_SerializationRoundtrip()
    {
        var original = new LongValue("test", -12345L);
        var data = original.Serialize();
        var restored = BitConverter.ToInt32(data, 0);
        Assert.Equal(-12345, restored);
    }

    [Fact]
    public void ULongValue_SerializationRoundtrip()
    {
        var original = new ULongValue("test", 12345UL);
        var data = original.Serialize();
        var restored = BitConverter.ToUInt32(data, 0);
        Assert.Equal(12345U, restored);
    }

    // ========================================================================
    // Size Method Tests
    // ========================================================================

    [Fact]
    public void LongValue_SizeReturns4()
    {
        var lv = new LongValue("test", 100L);
        Assert.Equal(4, lv.Size());
    }

    [Fact]
    public void ULongValue_SizeReturns4()
    {
        var ulv = new ULongValue("test", 100UL);
        Assert.Equal(4, ulv.Size());
    }

    // ========================================================================
    // Error Message Validation Tests
    // ========================================================================

    [Fact]
    public void LongValue_ErrorMessageIsDescriptive()
    {
        var ex = Assert.Throws<OverflowException>(() =>
            new LongValue("test", 5000000000L));

        Assert.Contains("LongValue", ex.Message);
        Assert.Contains("32-bit", ex.Message);
        Assert.Contains("LLongValue", ex.Message);
        Assert.Contains("5000000000", ex.Message);
    }

    [Fact]
    public void ULongValue_ErrorMessageIsDescriptive()
    {
        var ex = Assert.Throws<OverflowException>(() =>
            new ULongValue("test", 10000000000UL));

        Assert.Contains("ULongValue", ex.Message);
        Assert.Contains("32-bit", ex.Message);
        Assert.Contains("ULLongValue", ex.Message);
        Assert.Contains("10000000000", ex.Message);
    }

    // ========================================================================
    // Type Conversion Tests
    // ========================================================================

    [Fact]
    public void LongValue_ConvertsToInt()
    {
        var lv = new LongValue("test", 12345L);
        Assert.Equal(12345, lv.ToInt());
    }

    [Fact]
    public void ULongValue_ConvertsToInt()
    {
        var ulv = new ULongValue("test", 12345UL);
        Assert.Equal(12345, ulv.ToInt());
    }

    [Fact]
    public void LongValue_ConvertsToFloat()
    {
        var lv = new LongValue("test", 12345L);
        Assert.Equal(12345.0f, lv.ToFloat());
    }

    [Fact]
    public void ULongValue_ConvertsToDouble()
    {
        var ulv = new ULongValue("test", 12345UL);
        Assert.Equal(12345.0, ulv.ToDouble());
    }

    [Fact]
    public void LongValue_ConvertsToBoolean()
    {
        var lv1 = new LongValue("test1", 100L);
        var lv2 = new LongValue("test2", 0L);
        Assert.True(lv1.ToBoolean());
        Assert.False(lv2.ToBoolean());
    }

    [Fact]
    public void LongValue_ConvertsToString()
    {
        var lv = new LongValue("test", 12345L);
        Assert.Equal("12345", lv.ToString());
    }

    // ========================================================================
    // Platform Independence Tests
    // ========================================================================

    [Fact]
    public void LongValue_UsesLittleEndian()
    {
        var lv = new LongValue("test", 0x12345678);
        var data = lv.Serialize();

        // Little-endian: least significant byte first
        // 0x12345678 -> 78 56 34 12
        Assert.Equal(0x78, data[0]);
        Assert.Equal(0x56, data[1]);
        Assert.Equal(0x34, data[2]);
        Assert.Equal(0x12, data[3]);
    }

    [Fact]
    public void ULongValue_UsesLittleEndian()
    {
        var ulv = new ULongValue("test", 0x12345678U);
        var data = ulv.Serialize();

        // Little-endian: least significant byte first
        Assert.Equal(0x78, data[0]);
        Assert.Equal(0x56, data[1]);
        Assert.Equal(0x34, data[2]);
        Assert.Equal(0x12, data[3]);
    }

    // ========================================================================
    // Boundary Value Tests
    // ========================================================================

    [Theory]
    [InlineData(-2147483648L)]  // INT32_MIN
    [InlineData(-1000000L)]
    [InlineData(-1L)]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(1000000L)]
    [InlineData(2147483647L)]   // INT32_MAX
    public void LongValue_AcceptsValidRangeValues(long value)
    {
        var lv = new LongValue("test", value);
        Assert.Equal(value, lv.ToLong());
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(1000000UL)]
    [InlineData(4294967295UL)]  // UINT32_MAX
    public void ULongValue_AcceptsValidRangeValues(ulong value)
    {
        var ulv = new ULongValue("test", value);
        Assert.Equal((long)value, ulv.ToLong());
    }

    [Theory]
    [InlineData(-2147483649L)]  // INT32_MIN - 1
    [InlineData(2147483648L)]   // INT32_MAX + 1
    [InlineData(-5000000000L)]
    [InlineData(5000000000L)]
    public void LongValue_RejectsOutOfRangeValues(long value)
    {
        Assert.Throws<OverflowException>(() => new LongValue("test", value));
    }

    [Theory]
    [InlineData(4294967296UL)]  // UINT32_MAX + 1
    [InlineData(10000000000UL)]
    public void ULongValue_RejectsOutOfRangeValues(ulong value)
    {
        Assert.Throws<OverflowException>(() => new ULongValue("test", value));
    }
}
