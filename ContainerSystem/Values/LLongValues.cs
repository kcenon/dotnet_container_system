/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;
using ContainerSystem.Core;

namespace ContainerSystem.Values;

/// <summary>
/// Long long integer value (64-bit signed).
/// Equivalent to C++ long long type.
/// In .NET, this is the same as long (Int64).
/// </summary>
public class LLongValue : Value
{
    private long _value;

    public LLongValue(string name, long value) : base(name, ValueTypes.LLongValue)
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
/// Unsigned long long integer value (64-bit unsigned).
/// Equivalent to C++ unsigned long long type.
/// In .NET, this is the same as ulong (UInt64).
/// </summary>
public class ULLongValue : Value
{
    private ulong _value;

    public ULLongValue(string name, ulong value) : base(name, ValueTypes.ULLongValue)
    {
        _value = value;
    }

    public override string Data() => _value.ToString();
    public override int Size() => sizeof(ulong);
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override int ToInt() => (int)_value;
    public override long ToLong() => (long)_value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString();
    public override bool ToBoolean() => _value != 0;
}
