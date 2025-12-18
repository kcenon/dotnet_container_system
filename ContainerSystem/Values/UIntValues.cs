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
    public override uint ToUInt() => _value;
    public override int ToInt() => (int)_value;
    public override long ToLong() => _value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString();
    public override bool ToBoolean() => _value != 0;
}

/// <summary>
/// Unsigned long integer value (type 7) - 32-bit unsigned range.
/// </summary>
/// <remarks>
/// <para>
/// <b>Cross-Language Compatibility Warning:</b> This type exists for cross-language compatibility
/// with C++ where 'unsigned long' is typically 32-bit. In C#, this wraps a <see cref="ulong"/> but enforces
/// <see cref="uint"/> (UInt32) limits during construction and serialization.
/// </para>
/// <para>
/// <b>Policy:</b> Enforces 32-bit range [0, 2^32-1]. Values exceeding this range will throw
/// <see cref="OverflowException"/>. Use <see cref="ULLongValue"/> for true 64-bit values.
/// </para>
/// <para>
/// Always serializes as 4 bytes (uint32) regardless of platform.
/// </para>
/// </remarks>
public class ULongValue : Value
{
    // 32-bit unsigned range constant
    private const uint UINT32_MAX = uint.MaxValue;  // 4294967295

    private uint _value;

    public ULongValue(string name, ulong value) : base(name, ValueTypes.ULongValue)
    {
        // Enforce strict 32-bit range policy
        if (value > UINT32_MAX)
        {
            throw new OverflowException(
                $"ULongValue: value {value} exceeds 32-bit range " +
                $"[0, {UINT32_MAX}]. " +
                "Use ULLongValue for 64-bit values."
            );
        }
        _value = (uint)value;
    }

    public override string Data() => _value.ToString();
    public override int Size() => sizeof(uint);  // Always 4 bytes
    public override byte[] Serialize() => BitConverter.GetBytes(_value);
    public override ulong ToULong() => _value;
    public override int ToInt() => (int)_value;
    public override long ToLong() => _value;
    public override float ToFloat() => _value;
    public override double ToDouble() => _value;
    public override string ToString() => _value.ToString();
    public override bool ToBoolean() => _value != 0;
}
