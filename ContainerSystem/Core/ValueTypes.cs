/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
***************************************************************************/

namespace ContainerSystem.Core;

/// <summary>
/// Enumeration of available value types in the container system.
/// Equivalent to C++ value_types enum.
/// </summary>
public enum ValueTypes
{
    /// <summary>Null value (no data)</summary>
    NullValue = 0,

    /// <summary>Boolean value (true/false)</summary>
    BoolValue = 1,

    /// <summary>Short integer (16-bit signed)</summary>
    ShortValue = 2,

    /// <summary>Unsigned short integer (16-bit unsigned)</summary>
    UShortValue = 3,

    /// <summary>Integer (32-bit signed)</summary>
    IntValue = 4,

    /// <summary>Unsigned integer (32-bit unsigned)</summary>
    UIntValue = 5,

    /// <summary>Long integer (64-bit signed)</summary>
    LongValue = 6,

    /// <summary>Unsigned long integer (64-bit unsigned)</summary>
    ULongValue = 7,

    /// <summary>Long long integer (64-bit signed, same as Long in .NET)</summary>
    LLongValue = 8,

    /// <summary>Unsigned long long integer (64-bit unsigned)</summary>
    ULLongValue = 9,

    /// <summary>Floating point (32-bit)</summary>
    FloatValue = 10,

    /// <summary>Double precision floating point (64-bit)</summary>
    DoubleValue = 11,

    /// <summary>Binary data (byte array)</summary>
    BytesValue = 12,

    /// <summary>String data (UTF-8)</summary>
    StringValue = 13,

    /// <summary>Nested container (composite value)</summary>
    ContainerValue = 14,

    /// <summary>Array/list of values</summary>
    ArrayValue = 15
}

/// <summary>
/// Extension methods for ValueTypes enum.
/// </summary>
public static class ValueTypesExtensions
{
    private static readonly Dictionary<string, ValueTypes> StringToTypeMap = new()
    {
        { "0", ValueTypes.NullValue },
        { "1", ValueTypes.BoolValue },
        { "2", ValueTypes.ShortValue },
        { "3", ValueTypes.UShortValue },
        { "4", ValueTypes.IntValue },
        { "5", ValueTypes.UIntValue },
        { "6", ValueTypes.LongValue },
        { "7", ValueTypes.ULongValue },
        { "8", ValueTypes.LLongValue },
        { "9", ValueTypes.ULLongValue },
        { "10", ValueTypes.FloatValue },
        { "11", ValueTypes.DoubleValue },
        { "12", ValueTypes.BytesValue },
        { "13", ValueTypes.StringValue },
        { "14", ValueTypes.ContainerValue },
        { "15", ValueTypes.ArrayValue }
    };

    private static readonly Dictionary<ValueTypes, string> TypeToStringMap = new()
    {
        { ValueTypes.NullValue, "0" },
        { ValueTypes.BoolValue, "1" },
        { ValueTypes.ShortValue, "2" },
        { ValueTypes.UShortValue, "3" },
        { ValueTypes.IntValue, "4" },
        { ValueTypes.UIntValue, "5" },
        { ValueTypes.LongValue, "6" },
        { ValueTypes.ULongValue, "7" },
        { ValueTypes.LLongValue, "8" },
        { ValueTypes.ULLongValue, "9" },
        { ValueTypes.FloatValue, "10" },
        { ValueTypes.DoubleValue, "11" },
        { ValueTypes.BytesValue, "12" },
        { ValueTypes.StringValue, "13" },
        { ValueTypes.ContainerValue, "14" },
        { ValueTypes.ArrayValue, "15" }
    };

    /// <summary>
    /// Convert ValueTypes enum to string representation.
    /// Equivalent to C++ convert_value_type(const value_types&).
    /// </summary>
    /// <param name="type">The value type to convert</param>
    /// <returns>String representation (e.g., "4" for IntValue)</returns>
    public static string ToTypeString(this ValueTypes type)
    {
        return TypeToStringMap.TryGetValue(type, out var str) ? str : "0";
    }

    /// <summary>
    /// Convert string representation to ValueTypes enum.
    /// Equivalent to C++ convert_value_type(const std::string&).
    /// </summary>
    /// <param name="typeString">String representation (e.g., "4")</param>
    /// <returns>Corresponding ValueTypes enum</returns>
    public static ValueTypes FromTypeString(string typeString)
    {
        return StringToTypeMap.TryGetValue(typeString, out var type) ? type : ValueTypes.NullValue;
    }
}
