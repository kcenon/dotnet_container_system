// BSD 3-Clause License
// Copyright (c) 2025, kcenon (kcenon@naver.com)

using BenchmarkDotNet.Attributes;
using ContainerSystem.Core;
using ContainerSystem.Values;

namespace ContainerSystem.Benchmarks;

/// <summary>
/// Container operations performance benchmarks
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class ContainerBenchmarks
{
    private ValueContainer _container = null!;

    [GlobalSetup]
    public void Setup()
    {
        _container = new ValueContainer();
        _container.MessageType = "benchmark";
        for (int i = 0; i < 100; i++)
        {
            _container.Add(new IntValue($"value_{i}", i));
        }
    }

    [Benchmark]
    public ValueContainer CreateContainer()
    {
        return new ValueContainer();
    }

    [Benchmark]
    public ValueContainer CreateContainerWithMetadata()
    {
        var c = new ValueContainer();
        c.MessageType = "test";
        c.SetSource("client", "session");
        c.SetTarget("server", "handler");
        return c;
    }

    [Benchmark]
    public void AddIntValue()
    {
        var c = new ValueContainer();
        c.Add(new IntValue("x", 42));
    }

    [Benchmark]
    public void AddStringValue()
    {
        var c = new ValueContainer();
        c.Add(new StringValue("name", "test value"));
    }

    [Benchmark]
    public void AddMultipleValues()
    {
        var c = new ValueContainer();
        c.Add(new IntValue("int", 1));
        c.Add(new DoubleValue("double", 1.5));
        c.Add(new StringValue("string", "test"));
        c.Add(new BoolValue("bool", true));
    }

    [Benchmark]
    public Value? GetValue()
    {
        return _container.GetValue("value_50");
    }

    [Benchmark]
    public List<Value> GetAllValues()
    {
        return _container.Values();
    }

    [Benchmark]
    public int CountValues()
    {
        return _container.Count;
    }
}
