/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Globalization;
using System.Text;
using ContainerSystem.Core;

namespace ContainerSystem.Values;

/// <summary>
/// Integer value (32-bit signed).
/// Supports zero-copy serialization via Span&lt;T&gt;.
/// </summary>
public class IntValue : Value, IValueSpan
{
    private int _value;

    public IntValue(string name, int value) : base(name, ValueTypes.IntValue)
    {
        _value = value;
    }

    public override string Data() => _value.ToString();
    public override int Size() => sizeof(int);
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override int ToInt() => _value;
    public override long ToLong() => _value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString();
    public override bool ToBoolean() => _value != 0;

    // IValueSpan implementation for zero-copy serialization
    public bool TrySerialize(Span<byte> destination, out int bytesWritten)
    {
        if (destination.Length < sizeof(int))
        {
            bytesWritten = 0;
            return false;
        }

        BitConverter.TryWriteBytes(destination, _value);
        bytesWritten = sizeof(int);
        return true;
    }

    public ReadOnlySpan<byte> AsSpan()
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BitConverter.TryWriteBytes(buffer, _value);
        return buffer;
    }
}

/// <summary>
/// Long integer value (type 6) - 32-bit signed range.
/// Policy: Enforces 32-bit range [-2^31, 2^31-1].
/// Values exceeding this range should use LLongValue.
/// Always serializes as 4 bytes (int32) regardless of platform.
/// Supports zero-copy serialization via Span&lt;T&gt;.
/// </summary>
public class LongValue : Value, IValueSpan
{
    // 32-bit signed range constants
    private const int INT32_MIN = int.MinValue;  // -2147483648
    private const int INT32_MAX = int.MaxValue;   // 2147483647

    private int _value;

    public LongValue(string name, long value) : base(name, ValueTypes.LongValue)
    {
        // Enforce strict 32-bit range policy
        if (value < INT32_MIN || value > INT32_MAX)
        {
            throw new OverflowException(
                $"LongValue: value {value} exceeds 32-bit range " +
                $"[{INT32_MIN}, {INT32_MAX}]. " +
                "Use LLongValue for 64-bit values."
            );
        }
        _value = (int)value;
    }

    public override string Data() => _value.ToString();
    public override int Size() => sizeof(int);  // Always 4 bytes
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override int ToInt() => _value;
    public override long ToLong() => _value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString();
    public override bool ToBoolean() => _value != 0;

    // IValueSpan implementation for zero-copy serialization
    public bool TrySerialize(Span<byte> destination, out int bytesWritten)
    {
        if (destination.Length < sizeof(int))
        {
            bytesWritten = 0;
            return false;
        }

        BitConverter.TryWriteBytes(destination, _value);
        bytesWritten = sizeof(int);
        return true;
    }

    public ReadOnlySpan<byte> AsSpan()
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BitConverter.TryWriteBytes(buffer, _value);
        return buffer;
    }
}

/// <summary>
/// Double precision floating point value (64-bit).
/// Supports zero-copy serialization via Span&lt;T&gt;.
/// </summary>
public class DoubleValue : Value, IValueSpan
{
    private double _value;

    public DoubleValue(string name, double value) : base(name, ValueTypes.DoubleValue)
    {
        _value = value;
    }

    public override string Data() => _value.ToString(CultureInfo.InvariantCulture);
    public override int Size() => sizeof(double);
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override int ToInt() => (int)_value;
    public override long ToLong() => (long)_value;
    public override float ToFloat() => (float)_value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
    public override bool ToBoolean() => Math.Abs(_value) > 0.0;

    // IValueSpan implementation for zero-copy serialization
    public bool TrySerialize(Span<byte> destination, out int bytesWritten)
    {
        if (destination.Length < sizeof(double))
        {
            bytesWritten = 0;
            return false;
        }

        BitConverter.TryWriteBytes(destination, _value);
        bytesWritten = sizeof(double);
        return true;
    }

    public ReadOnlySpan<byte> AsSpan()
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        BitConverter.TryWriteBytes(buffer, _value);
        return buffer;
    }
}

/// <summary>
/// Float value (32-bit).
/// Supports zero-copy serialization via Span&lt;T&gt;.
/// </summary>
public class FloatValue : Value, IValueSpan
{
    private float _value;

    public FloatValue(string name, float value) : base(name, ValueTypes.FloatValue)
    {
        _value = value;
    }

    public override string Data() => _value.ToString(CultureInfo.InvariantCulture);
    public override int Size() => sizeof(float);
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override int ToInt() => (int)_value;
    public override long ToLong() => (long)_value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
    public override bool ToBoolean() => Math.Abs(_value) > 0.0f;

    // IValueSpan implementation for zero-copy serialization
    public bool TrySerialize(Span<byte> destination, out int bytesWritten)
    {
        if (destination.Length < sizeof(float))
        {
            bytesWritten = 0;
            return false;
        }

        BitConverter.TryWriteBytes(destination, _value);
        bytesWritten = sizeof(float);
        return true;
    }

    public ReadOnlySpan<byte> AsSpan()
    {
        Span<byte> buffer = stackalloc byte[sizeof(float)];
        BitConverter.TryWriteBytes(buffer, _value);
        return buffer;
    }
}

/// <summary>
/// Boolean value.
/// </summary>
public class BoolValue : Value
{
    private bool _value;

    public BoolValue(string name, bool value) : base(name, ValueTypes.BoolValue)
    {
        _value = value;
    }

    public override string Data() => _value ? "1" : "0";
    public override int Size() => sizeof(bool);
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override bool ToBoolean() => _value;
    public override int ToInt() => _value ? 1 : 0;
    public override long ToLong() => _value ? 1L : 0L;
    public override float ToFloat() => _value ? 1.0f : 0.0f;
    public override double ToDouble() => _value ? 1.0 : 0.0;
    public override string ToString() => _value.ToString();
}

/// <summary>
/// Bytes value (binary data).
/// </summary>
public class BytesValue : Value
{
    private byte[] _value;

    public BytesValue(string name, byte[] value) : base(name, ValueTypes.BytesValue)
    {
        _value = value;
    }

    public override string Data() => Convert.ToBase64String(_value);
    public override int Size() => _value.Length;
    public override byte[] Serialize() => _value;
    public override byte[] ToBytes() => _value;
    public override string ToString() => Convert.ToBase64String(_value);
}
