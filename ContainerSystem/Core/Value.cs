/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using System.Text;

namespace ContainerSystem.Core;

/// <summary>
/// Base class for all values stored in the container system.
/// Equivalent to C++ value class.
/// </summary>
public abstract class Value : IValue
{
    private string _name;
    private ValueTypes _type;
    private Value? _parent;
    private readonly List<Value> _children;

    protected Value()
    {
        _name = string.Empty;
        _type = ValueTypes.NullValue;
        _children = new List<Value>();
    }

    protected Value(string name, ValueTypes type)
    {
        _name = name;
        _type = type;
        _children = new List<Value>();
    }

    protected Value(string name, List<Value>? children = null)
    {
        _name = name;
        _type = ValueTypes.ContainerValue;
        _children = children ?? new List<Value>();
    }

    /// <summary>
    /// Gets or sets the name of this value.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Gets the type of this value.
    /// </summary>
    public ValueTypes Type => _type;

    /// <summary>
    /// Gets the parent value (if nested).
    /// </summary>
    public Value? Parent => _parent;

    /// <summary>
    /// Sets the parent value.
    /// </summary>
    public void SetParent(Value parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Gets the number of child values.
    /// </summary>
    public int ChildCount => _children.Count;

    /// <summary>
    /// Gets all child values.
    /// </summary>
    /// <param name="onlyContainer">If true, only return container-type children</param>
    /// <returns>List of child values</returns>
    public List<Value> Children(bool onlyContainer = false)
    {
        if (!onlyContainer)
            return new List<Value>(_children);

        return _children.Where(c => c.Type == ValueTypes.ContainerValue).ToList();
    }

    /// <summary>
    /// Gets all values with the specified key/name.
    /// </summary>
    /// <param name="key">The name to search for</param>
    /// <returns>List of matching values</returns>
    public List<Value> ValueArray(string key)
    {
        return _children.Where(c => c.Name == key).ToList();
    }

    /// <summary>
    /// Adds a child value.
    /// </summary>
    /// <param name="child">The value to add</param>
    public void AddChild(Value child)
    {
        child.SetParent(this);
        _children.Add(child);
    }

    /// <summary>
    /// Checks if this value is null.
    /// </summary>
    public bool IsNull() => _type == ValueTypes.NullValue;

    /// <summary>
    /// Checks if this value is binary data.
    /// </summary>
    public bool IsBytes() => _type == ValueTypes.BytesValue;

    /// <summary>
    /// Checks if this value is boolean.
    /// </summary>
    public bool IsBoolean() => _type == ValueTypes.BoolValue;

    /// <summary>
    /// Checks if this value is numeric.
    /// </summary>
    public bool IsNumeric() => _type switch
    {
        ValueTypes.ShortValue => true,
        ValueTypes.UShortValue => true,
        ValueTypes.IntValue => true,
        ValueTypes.UIntValue => true,
        ValueTypes.LongValue => true,
        ValueTypes.ULongValue => true,
        ValueTypes.LLongValue => true,
        ValueTypes.ULLongValue => true,
        ValueTypes.FloatValue => true,
        ValueTypes.DoubleValue => true,
        _ => false
    };

    /// <summary>
    /// Checks if this value is a string.
    /// </summary>
    public bool IsString() => _type == ValueTypes.StringValue;

    /// <summary>
    /// Checks if this value is a container.
    /// </summary>
    public bool IsContainer() => _type == ValueTypes.ContainerValue;

    // Abstract conversion methods to be implemented by derived classes
    public virtual bool ToBoolean()
    {
        ThrowIfNull();
        return false;
    }

    public virtual short ToShort()
    {
        ThrowIfNull();
        return 0;
    }

    public virtual ushort ToUShort()
    {
        ThrowIfNull();
        return 0;
    }

    public virtual int ToInt()
    {
        ThrowIfNull();
        return 0;
    }

    public virtual uint ToUInt()
    {
        ThrowIfNull();
        return 0;
    }

    public virtual long ToLong()
    {
        ThrowIfNull();
        return 0;
    }

    public virtual ulong ToULong()
    {
        ThrowIfNull();
        return 0;
    }

    public virtual float ToFloat()
    {
        ThrowIfNull();
        return 0.0f;
    }

    public virtual double ToDouble()
    {
        ThrowIfNull();
        return 0.0;
    }

    public virtual string ToString()
    {
        ThrowIfNull();
        return string.Empty;
    }

    public virtual byte[] ToBytes()
    {
        ThrowIfNull();
        return Array.Empty<byte>();
    }

    /// <summary>
    /// Gets the raw data as string.
    /// </summary>
    public abstract string Data();

    /// <summary>
    /// Gets the size of the data in bytes.
    /// </summary>
    public abstract int Size();

    /// <summary>
    /// Serializes this value to a byte array.
    /// </summary>
    public abstract byte[] Serialize();

    /// <summary>
    /// Serializes this value to JSON format.
    /// </summary>
    public virtual string ToJson()
    {
        var sb = new StringBuilder();
        sb.Append('{');
        sb.Append($"\"name\":\"{Name}\",");
        sb.Append($"\"type\":{(int)Type},");
        sb.Append($"\"data\":\"{Data()}\"");
        sb.Append('}');
        return sb.ToString();
    }

    /// <summary>
    /// Serializes this value to XML format.
    /// </summary>
    public virtual string ToXml()
    {
        var sb = new StringBuilder();
        sb.Append($"<value name=\"{Name}\" type=\"{(int)Type}\">");
        sb.Append(Data());
        sb.Append("</value>");
        return sb.ToString();
    }

    private void ThrowIfNull()
    {
        if (IsNull())
            throw new InvalidOperationException("Cannot convert null_value to target type.");
    }
}
