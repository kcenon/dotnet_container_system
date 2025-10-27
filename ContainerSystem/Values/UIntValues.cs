/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;
using ContainerSystem.Core;

namespace ContainerSystem.Values;

/// <summary>
/// Unsigned integer value (32-bit unsigned).
/// Equivalent to C++ unsigned int type.
/// </summary>
public class UIntValue : Value
{
    private uint _value;

    public UIntValue(string name, uint value) : base(name, ValueTypes.UIntValue)
    {
        _value = value;
    }

    public override string Data() => _value.ToString();
    public override int Size() => sizeof(uint);
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override int ToInt() => (int)_value;
    public override long ToLong() => _value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString();
    public override bool ToBoolean() => _value != 0;
}

/// <summary>
/// Unsigned long integer value (64-bit unsigned).
/// Equivalent to C++ unsigned long type.
/// </summary>
public class ULongValue : Value
{
    private ulong _value;

    public ULongValue(string name, ulong value) : base(name, ValueTypes.ULongValue)
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
