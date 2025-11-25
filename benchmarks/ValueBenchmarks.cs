// BSD 3-Clause License
// Copyright (c) 2025, kcenon (kcenon@naver.com)

using BenchmarkDotNet.Attributes;
using ContainerSystem.Core;
using ContainerSystem.Values;

namespace ContainerSystem.Benchmarks;

/// <summary>
/// Value type operations performance benchmarks
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class ValueBenchmarks
{
    private IntValue _intValue = null!;
    private LLongValue _longValue = null!;
    private DoubleValue _doubleValue = null!;
    private StringValue _stringValue = null!;
    private BytesValue _bytesValue = null!;

    [GlobalSetup]
    public void Setup()
    {
        _intValue = new IntValue("int", 42);
        _longValue = new LLongValue("long", 1234567890L);
        _doubleValue = new DoubleValue("double", 3.14159265359);
        _stringValue = new StringValue("string", "Hello, World!");
        _bytesValue = new BytesValue("bytes", new byte[100]);
    }

    // Value creation
    [Benchmark]
    public IntValue CreateIntValue() => new IntValue("x", 42);

    [Benchmark]
    public LLongValue CreateLLongValue() => new LLongValue("x", 1234567890L);

    [Benchmark]
    public DoubleValue CreateDoubleValue() => new DoubleValue("x", 3.14159);

    [Benchmark]
    public StringValue CreateStringValue() => new StringValue("x", "test value");

    [Benchmark]
    public BytesValue CreateBytesValue() => new BytesValue("x", new byte[100]);

    // Type conversions
    [Benchmark]
    public int IntToInt() => _intValue.ToInt();

    [Benchmark]
    public long IntToLong() => _intValue.ToLong();

    [Benchmark]
    public double IntToDouble() => _intValue.ToDouble();

    [Benchmark]
    public string IntToString() => _intValue.ToString();

    [Benchmark]
    public double DoubleToDouble() => _doubleValue.ToDouble();

    [Benchmark]
    public string StringToString() => _stringValue.ToString();

    [Benchmark]
    public byte[] BytesToBytes() => _bytesValue.ToBytes();

    // Type checking
    [Benchmark]
    public bool IsNumeric() => _intValue.IsNumeric();

    [Benchmark]
    public bool IsString() => _stringValue.IsString();

    // Serialization
    [Benchmark]
    public byte[] SerializeInt() => _intValue.Serialize();

    [Benchmark]
    public byte[] SerializeLong() => _longValue.Serialize();

    [Benchmark]
    public byte[] SerializeString() => _stringValue.Serialize();

    [Benchmark]
    public string IntToJson() => _intValue.ToJson();

    [Benchmark]
    public string StringToJson() => _stringValue.ToJson();
}
