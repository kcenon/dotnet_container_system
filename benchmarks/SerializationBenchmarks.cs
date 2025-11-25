// BSD 3-Clause License
// Copyright (c) 2025, kcenon (kcenon@naver.com)

using BenchmarkDotNet.Attributes;
using ContainerSystem.Core;
using ContainerSystem.Values;
using ContainerSystem.Adapters;

namespace ContainerSystem.Benchmarks;

/// <summary>
/// Serialization performance benchmarks
/// </summary>
[MemoryDiagnoser]
[RankColumn]
public class SerializationBenchmarks
{
    private ValueContainer _smallContainer = null!;
    private ValueContainer _mediumContainer = null!;
    private ValueContainer _largeContainer = null!;
    private string _smallJson = null!;
    private string _mediumJson = null!;
    private byte[] _smallBinary = null!;
    private byte[] _mediumBinary = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Small container (4 values)
        _smallContainer = new ValueContainer();
        _smallContainer.MessageType = "small";
        _smallContainer.Add(new StringValue("name", "test"));
        _smallContainer.Add(new IntValue("count", 100));
        _smallContainer.Add(new DoubleValue("price", 99.99));
        _smallContainer.Add(new BoolValue("active", true));

        // Medium container (20 values)
        _mediumContainer = new ValueContainer();
        _mediumContainer.MessageType = "medium";
        _mediumContainer.SetSource("client", "session_123");
        _mediumContainer.SetTarget("server", "handler");
        for (int i = 0; i < 20; i++)
        {
            _mediumContainer.Add(new IntValue($"value_{i}", i));
        }

        // Large container (100 values)
        _largeContainer = new ValueContainer();
        _largeContainer.MessageType = "large";
        for (int i = 0; i < 100; i++)
        {
            _largeContainer.Add(new StringValue($"key_{i}", $"value_{i}"));
        }

        // Pre-serialize for deserialization benchmarks
        _smallJson = _smallContainer.Serialize();
        _mediumJson = _mediumContainer.Serialize();
        _smallBinary = _smallContainer.Store.Serialize();
        _mediumBinary = _mediumContainer.Store.Serialize();
    }

    // JSON Serialization
    [Benchmark]
    public string JsonSerialize_Small() => _smallContainer.Serialize();

    [Benchmark]
    public string JsonSerialize_Medium() => _mediumContainer.Serialize();

    [Benchmark]
    public string JsonSerialize_Large() => _largeContainer.Serialize();

    // JSON Deserialization
    [Benchmark]
    public ValueContainer JsonDeserialize_Small() => new ValueContainer(_smallJson);

    [Benchmark]
    public ValueContainer JsonDeserialize_Medium() => new ValueContainer(_mediumJson);

    // Binary Serialization
    [Benchmark]
    public byte[] BinarySerialize_Small() => _smallContainer.Store.Serialize();

    [Benchmark]
    public byte[] BinarySerialize_Medium() => _mediumContainer.Store.Serialize();

    // Binary Deserialization
    [Benchmark]
    public void BinaryDeserialize_Small()
    {
        var store = new ValueStore();
        store.Deserialize(_smallBinary);
    }

    [Benchmark]
    public void BinaryDeserialize_Medium()
    {
        var store = new ValueStore();
        store.Deserialize(_mediumBinary);
    }

    // JSON v2.0 Adapter
    [Benchmark]
    public string JsonV2Serialize_Small() => JsonV2Adapter.ToJson(_smallContainer);

    [Benchmark]
    public string JsonV2Serialize_Medium() => JsonV2Adapter.ToJson(_mediumContainer);
}
