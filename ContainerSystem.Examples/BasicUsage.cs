/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using ContainerSystem.Core;
using ContainerSystem.Values;

namespace ContainerSystem.Examples;

/// <summary>
/// Basic usage example for the container system.
/// Equivalent to C++ samples/basic_usage.cpp
/// </summary>
public static class BasicUsage
{
    public static void Run()
    {
        Console.WriteLine("=== Container System - Basic Usage Example ===");

        // 1. Basic container creation and value setting
        Console.WriteLine("\n1. Basic Container Operations:");

        var container = new ValueContainer();
        container.MessageType = "user_profile";

        // Set various types of values
        container.Add(new StringValue("user_id", "12345"));
        container.Add(new StringValue("username", "john_doe"));
        container.Add(new IntValue("age", 30));
        container.Add(new BoolValue("is_active", true));
        container.Add(new DoubleValue("balance", 1000.50));

        Console.WriteLine($"Container message type: {container.MessageType}");

        // 2. Reading values from container
        Console.WriteLine("\n2. Reading Values:");

        var userId = container.GetValue("user_id");
        if (userId != null)
        {
            Console.WriteLine($"User ID: {userId.ToString()}");
        }

        var username = container.GetValue("username");
        if (username != null)
        {
            Console.WriteLine($"Username: {username.ToString()}");
        }

        var age = container.GetValue("age");
        if (age != null)
        {
            Console.WriteLine($"Age: {age.ToInt()}");
        }

        var isActive = container.GetValue("is_active");
        if (isActive != null)
        {
            Console.WriteLine($"Is Active: {(isActive.ToBoolean() ? "Yes" : "No")}");
        }

        var balance = container.GetValue("balance");
        if (balance != null)
        {
            Console.WriteLine($"Balance: ${balance.ToDouble()}");
        }

        // 3. Multiple values with same name
        Console.WriteLine("\n3. Multiple Values with Same Name:");

        container.Add(new StringValue("tag", "csharp"));
        container.Add(new StringValue("tag", "dotnet"));
        container.Add(new StringValue("tag", "example"));

        var tags = container.ValueArray("tag");
        Console.WriteLine($"User has {tags.Count} tags:");
        foreach (var tag in tags)
        {
            Console.WriteLine($"  - {tag.ToString()}");
        }

        // 4. Container serialization
        Console.WriteLine("\n4. Serialization:");

        string serialized = container.Serialize();
        Console.WriteLine($"Serialized container size: {serialized.Length} bytes");
        Console.WriteLine($"Serialized data: {serialized}");

        // 5. Container deserialization
        Console.WriteLine("\n5. Deserialization:");

        var restoredContainer = new ValueContainer(serialized);
        Console.WriteLine($"Restored container message type: {restoredContainer.MessageType}");

        var restoredUsername = restoredContainer.GetValue("username");
        if (restoredUsername != null)
        {
            Console.WriteLine($"Restored username: {restoredUsername.ToString()}");
        }

        var restoredBalance = restoredContainer.GetValue("balance");
        if (restoredBalance != null)
        {
            Console.WriteLine($"Restored balance: ${restoredBalance.ToDouble()}");
        }

        // 6. Container metadata
        Console.WriteLine("\n6. Container Metadata:");

        container.SetSource("client_app", "user_session_123");
        container.SetTarget("server_api", "profile_handler");

        Console.WriteLine($"Source: {container.SourceId}/{container.SourceSubId}");
        Console.WriteLine($"Target: {container.TargetId}/{container.TargetSubId}");
        Console.WriteLine($"Message type: {container.MessageType}");
        Console.WriteLine($"Version: {container.Version}");

        Console.WriteLine("\n=== Example completed successfully ===");
    }
}
