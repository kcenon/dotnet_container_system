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
/// </summary>
public class IntValue : Value
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
}

/// <summary>
/// Long integer value (64-bit signed).
/// </summary>
public class LongValue : Value
{
    private long _value;

    public LongValue(string name, long value) : base(name, ValueTypes.LongValue)
    {
        _value = value;
    }

    public override string Data() => _value.ToString();
    public override int Size() => sizeof(long);
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override int ToInt() => (int)_value;
    public override long ToLong() => _value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString();
    public override bool ToBoolean() => _value != 0;
}

/// <summary>
/// Double precision floating point value (64-bit).
/// </summary>
public class DoubleValue : Value
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
}

/// <summary>
/// Float value (32-bit).
/// </summary>
public class FloatValue : Value
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
