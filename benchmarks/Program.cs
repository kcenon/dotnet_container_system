// BSD 3-Clause License
// Copyright (c) 2025, kcenon (kcenon@naver.com)

using BenchmarkDotNet.Running;

namespace ContainerSystem.Benchmarks;

internal class Program
{
    private static void Main(string[] args)
    {
        // Run all benchmarks
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

        // Or run specific benchmark
        // var summary = BenchmarkRunner.Run<SerializationBenchmarks>();
    }
}
