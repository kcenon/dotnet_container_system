/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;
using ContainerSystem.Core;

namespace ContainerSystem.Values;

/// <summary>
/// Short integer value (16-bit signed).
/// Equivalent to C++ short type.
/// </summary>
public class ShortValue : Value
{
    private short _value;

    public ShortValue(string name, short value) : base(name, ValueTypes.ShortValue)
    {
        _value = value;
    }

    public override string Data() => _value.ToString();
    public override int Size() => sizeof(short);
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override short ToShort() => _value;
    public override int ToInt() => _value;
    public override long ToLong() => _value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString();
    public override bool ToBoolean() => _value != 0;
}

/// <summary>
/// Unsigned short integer value (16-bit unsigned).
/// Equivalent to C++ unsigned short type.
/// </summary>
public class UShortValue : Value
{
    private ushort _value;

    public UShortValue(string name, ushort value) : base(name, ValueTypes.UShortValue)
    {
        _value = value;
    }

    public override string Data() => _value.ToString();
    public override int Size() => sizeof(ushort);
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override ushort ToUShort() => _value;
    public override int ToInt() => _value;
    public override long ToLong() => _value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString();
    public override bool ToBoolean() => _value != 0;
}
