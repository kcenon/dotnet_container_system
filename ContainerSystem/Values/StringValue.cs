/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;
using ContainerSystem.Core;

namespace ContainerSystem.Values;

/// <summary>
/// String value implementation.
/// Equivalent to C++ string_value class.
/// </summary>
public class StringValue : Value
{
    private string _value;

    public StringValue() : base(string.Empty, ValueTypes.StringValue)
    {
        _value = string.Empty;
    }

    public StringValue(string name, string value) : base(name, ValueTypes.StringValue)
    {
        _value = value;
    }

    public override string Data() => _value;

    public override int Size() => Encoding.UTF8.GetByteCount(_value);

    public override byte[] Serialize()
    {
        return Encoding.UTF8.GetBytes(_value);
    }

    public override string ToString() => _value;

    public override bool ToBoolean() => bool.TryParse(_value, out var result) && result;

    public override short ToShort() => short.TryParse(_value, out var result) ? result : (short)0;

    public override ushort ToUShort() => ushort.TryParse(_value, out var result) ? result : (ushort)0;

    public override int ToInt() => int.TryParse(_value, out var result) ? result : 0;

    public override uint ToUInt() => uint.TryParse(_value, out var result) ? result : 0;

    public override long ToLong() => long.TryParse(_value, out var result) ? result : 0L;

    public override ulong ToULong() => ulong.TryParse(_value, out var result) ? result : 0UL;

    public override float ToFloat() => float.TryParse(_value, out var result) ? result : 0.0f;

    public override double ToDouble() => double.TryParse(_value, out var result) ? result : 0.0;
}
