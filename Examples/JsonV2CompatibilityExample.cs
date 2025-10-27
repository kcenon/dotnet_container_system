/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.

JSON v2.0 Cross-Language Compatibility Example

This example demonstrates how to use JsonV2Adapter for data interchange
between C++, Python, and .NET container system implementations.
***************************************************************************/

using System;
using ContainerSystem.Core;
using ContainerSystem.Values;
using ContainerSystem.Adapters;

namespace ContainerSystem.Examples;

/// <summary>
/// Demonstrates JSON v2.0 cross-language compatibility features.
/// </summary>
public static class JsonV2CompatibilityExample
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=" + new string('=', 79));
        Console.WriteLine("JSON v2.0 Cross-Language Compatibility Examples");
        Console.WriteLine("=" + new string('=', 79));
        Console.WriteLine();
        Console.WriteLine("These examples demonstrate JSON v2.0 adapter usage for data interchange");
        Console.WriteLine("between C++, Python, and .NET container system implementations.");
        Console.WriteLine("=" + new string('=', 79));

        try
        {
            Example1_BasicConversion();
            Example2_NestedContainers();
            Example3_BinaryData();
            Example4_CppFormatConversion();
            Example5_FormatDetection();
            Example6_CrossLanguageWorkflow();
            Example7_AllValueTypes();

            Console.WriteLine();
            Console.WriteLine("=" + new string('=', 79));
            Console.WriteLine("All examples completed successfully!");
            Console.WriteLine("=" + new string('=', 79));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nExample failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// Example 1: Basic container to v2.0 JSON conversion.
    /// </summary>
    private static void Example1_BasicConversion()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("Example 1: Basic .NET Container to JSON v2.0");
        Console.WriteLine(new string('=', 80));

        // Create a container with user data
        var container = new ValueContainer(
            sourceId: "dotnet_client",
            sourceSubId: "session_001",
            targetId: "cpp_server",
            targetSubId: "handler_main",
            messageType: "user_profile"
        );

        container.Add(new IntValue("user_id", 12345));
        container.Add(new StringValue("username", "john_doe"));
        container.Add(new StringValue("email", "john@example.com"));
        container.Add(new DoubleValue("balance", 1500.75));
        container.Add(new BoolValue("is_active", true));

        // Convert to v2.0 JSON
        var v2Json = JsonV2Adapter.ToV2Json(container, pretty: true);
        Console.WriteLine("\nJSON v2.0 Output:");
        Console.WriteLine(v2Json);

        // Parse back from v2.0 JSON
        var restored = JsonV2Adapter.FromV2Json(v2Json);
        Console.WriteLine("\nRestored container:");
        Console.WriteLine($"  Message type: {restored.MessageType}");
        Console.WriteLine($"  Source: {restored.SourceId}.{restored.SourceSubId}");
        Console.WriteLine($"  Target: {restored.TargetId}.{restored.TargetSubId}");
        Console.WriteLine($"  Values count: {restored.Units.Count}");

        // Verify values
        var userId = restored.GetValue("user_id");
        var username = restored.GetValue("username");
        Console.WriteLine($"\n  user_id: {userId?.ToInt() ?? 0}");
        Console.WriteLine($"  username: {username?.ToString() ?? "N/A"}");
    }

    /// <summary>
    /// Example 2: Nested containers with v2.0 format.
    /// </summary>
    private static void Example2_NestedContainers()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("Example 2: Nested Containers in JSON v2.0");
        Console.WriteLine(new string('=', 80));

        // Create main container
        var main = new ValueContainer(
            sourceId: "dotnet_app",
            sourceSubId: "",
            targetId: "python_service",
            targetSubId: "",
            messageType: "customer_data"
        );

        main.Add(new IntValue("customer_id", 9876));
        main.Add(new StringValue("name", "Alice Johnson"));

        // Create nested address container
        var address = new ContainerValue("address");
        address.Add(new StringValue("street", "123 Main St"));
        address.Add(new StringValue("city", "Seattle"));
        address.Add(new StringValue("state", "WA"));
        address.Add(new StringValue("zip", "98101"));
        main.Add(address);

        // Create nested contact container
        var contact = new ContainerValue("contact");
        contact.Add(new StringValue("phone", "+1-206-555-0123"));
        contact.Add(new StringValue("email", "alice@example.com"));
        main.Add(contact);

        // Convert to v2.0 JSON
        var v2Json = JsonV2Adapter.ToV2Json(main, pretty: true);
        Console.WriteLine("\nJSON v2.0 with nested containers:");
        Console.WriteLine(v2Json);

        // Parse and verify
        var restored = JsonV2Adapter.FromV2Json(v2Json);
        var addressVal = restored.GetValue("address") as ContainerValue;
        if (addressVal != null)
        {
            Console.WriteLine($"\nAddress container has {addressVal.ChildCount} children:");
            var city = addressVal.GetValue("city");
            Console.WriteLine($"  City: {city?.ToString() ?? "N/A"}");
        }
    }

    /// <summary>
    /// Example 3: Binary data with base64 encoding.
    /// </summary>
    private static void Example3_BinaryData()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("Example 3: Binary Data with Base64 Encoding");
        Console.WriteLine(new string('=', 80));

        // Create container with binary data
        var container = new ValueContainer(
            sourceId: "dotnet_app",
            sourceSubId: "",
            targetId: "",
            targetSubId: "",
            messageType: "image_data"
        );

        container.Add(new StringValue("filename", "avatar.png"));
        container.Add(new IntValue("size", 2048));

        // Add binary data (simulating image bytes)
        var binaryData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        container.Add(new BytesValue("image_bytes", binaryData));

        // Convert to v2.0 JSON
        var v2Json = JsonV2Adapter.ToV2Json(container, pretty: true);
        Console.WriteLine("\nJSON v2.0 with binary data:");
        Console.WriteLine(v2Json);

        // Parse and verify binary data
        var restored = JsonV2Adapter.FromV2Json(v2Json);
        var imageVal = restored.GetValue("image_bytes");
        if (imageVal != null)
        {
            var restoredBytes = imageVal.ToBytes();
            Console.WriteLine("\nRestored binary data:");
            Console.WriteLine($"  Original: {BitConverter.ToString(binaryData)}");
            Console.WriteLine($"  Restored: {BitConverter.ToString(restoredBytes)}");
            Console.WriteLine($"  Match: {CompareBytes(binaryData, restoredBytes)}");
        }
    }

    /// <summary>
    /// Example 4: Convert between C++ and .NET formats.
    /// </summary>
    private static void Example4_CppFormatConversion()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("Example 4: C++ Format Conversion");
        Console.WriteLine(new string('=', 80));

        // Simulate C++ JSON format (nested with header object)
        var cppJson = @"{
  ""header"": {
    ""target_id"": ""dotnet_service"",
    ""target_sub_id"": ""handler"",
    ""source_id"": ""cpp_client"",
    ""source_sub_id"": ""main"",
    ""message_type"": ""request"",
    ""version"": ""1.0.0.0""
  },
  ""values"": {
    ""request_id"": {
      ""type"": 4,
      ""data"": ""42""
    },
    ""action"": {
      ""type"": 13,
      ""data"": ""get_user""
    },
    ""timeout"": {
      ""type"": 4,
      ""data"": ""30""
    }
  }
}";

        Console.WriteLine("\nOriginal C++ JSON format:");
        Console.WriteLine(cppJson);

        // Parse C++ format
        var container = JsonV2Adapter.FromCppJson(cppJson);
        Console.WriteLine($"\nParsed container:");
        Console.WriteLine($"  Message type: {container.MessageType}");
        Console.WriteLine($"  Source: {container.SourceId}");
        Console.WriteLine($"  Target: {container.TargetId}");
        Console.WriteLine($"  Values: {container.Units.Count}");

        // Convert to v2.0 format
        var v2Json = JsonV2Adapter.ToV2Json(container, pretty: true);
        Console.WriteLine("\nConverted to JSON v2.0:");
        Console.WriteLine(v2Json);

        // Convert to .NET format
        var dotnetJson = container.ToJson();
        Console.WriteLine("\nConverted to .NET flat format:");
        Console.WriteLine(dotnetJson);
    }

    /// <summary>
    /// Example 5: Automatic format detection and conversion.
    /// </summary>
    private static void Example5_FormatDetection()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("Example 5: Format Detection and Auto-Conversion");
        Console.WriteLine(new string('=', 80));

        // Test different formats
        var testFormats = new[]
        {
            ("v2.0", @"{
                ""container"": {
                    ""version"": ""2.0"",
                    ""metadata"": {
                        ""message_type"": ""test"",
                        ""protocol_version"": ""1.0.0.0"",
                        ""source"": {""id"": ""src"", ""sub_id"": """"},
                        ""target"": {""id"": ""tgt"", ""sub_id"": """"}
                    },
                    ""values"": [
                        {""name"": ""key"", ""type"": 13, ""type_name"": ""string"", ""data"": ""value""}
                    ]
                }
            }"),
            ("cpp", @"{
                ""header"": {
                    ""message_type"": ""test"",
                    ""source_id"": ""src"",
                    ""target_id"": ""tgt""
                },
                ""values"": {
                    ""key"": {""type"": 13, ""data"": ""value""}
                }
            }"),
            ("python", @"{
                ""message_type"": ""test"",
                ""source_id"": ""src"",
                ""target_id"": ""tgt"",
                ""values"": [
                    {""name"": ""key"", ""type"": 13, ""data"": ""value""}
                ]
            }")
        };

        foreach (var (formatName, jsonStr) in testFormats)
        {
            var detected = JsonV2Adapter.DetectFormat(jsonStr);
            Console.WriteLine($"\n{formatName} format detected as: {detected}");

            // Convert to v2.0
            try
            {
                var v2Json = JsonV2Adapter.ConvertFormat(jsonStr, "v2.0");
                Console.WriteLine("  ✓ Successfully converted to v2.0");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Conversion failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Example 6: Complete cross-language data exchange workflow.
    /// </summary>
    private static void Example6_CrossLanguageWorkflow()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("Example 6: Cross-Language Data Exchange Workflow");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\nScenario: .NET -> C++ -> Python -> .NET");
        Console.WriteLine(new string('-', 80));

        // Step 1: .NET creates request
        Console.WriteLine("\n[Step 1] .NET creates request:");
        var dotnetRequest = new ValueContainer(
            sourceId: "dotnet_client",
            sourceSubId: "",
            targetId: "cpp_server",
            targetSubId: "",
            messageType: "data_request"
        );
        dotnetRequest.Add(new IntValue("request_id", 1001));
        dotnetRequest.Add(new StringValue("query", "SELECT * FROM users"));
        Console.WriteLine($"  Created container with {dotnetRequest.Units.Count} values");

        // Convert to v2.0 for transmission
        var v2Json = JsonV2Adapter.ToV2Json(dotnetRequest);
        Console.WriteLine($"  Serialized to v2.0 JSON ({v2Json.Length} bytes)");

        // Step 2: C++ receives and processes
        Console.WriteLine("\n[Step 2] C++ receives and processes (simulated):");
        var cppReceived = JsonV2Adapter.FromV2Json(v2Json);
        Console.WriteLine($"  C++ parsed container: {cppReceived.MessageType}");
        var queryValue = cppReceived.GetValue("query");
        Console.WriteLine($"  Processing query: {queryValue?.ToString()}");

        // C++ creates response
        var cppResponse = new ValueContainer(
            sourceId: "cpp_server",
            sourceSubId: "",
            targetId: "python_middleware",
            targetSubId: "",
            messageType: "data_response"
        );
        cppResponse.Add(new IntValue("request_id", 1001));
        cppResponse.Add(new IntValue("row_count", 42));
        cppResponse.Add(new StringValue("status", "success"));

        // Convert to C++ JSON format
        var cppJson = JsonV2Adapter.ToCppJson(cppResponse);
        Console.WriteLine("  C++ created response in C++ JSON format");

        // Step 3: Python receives via v2.0
        Console.WriteLine("\n[Step 3] Python receives and enriches (simulated):");
        var v2FromCpp = JsonV2Adapter.ConvertFormat(cppJson, "v2.0");
        var pythonReceived = JsonV2Adapter.FromV2Json(v2FromCpp);
        Console.WriteLine($"  Python parsed container: {pythonReceived.MessageType}");

        // Python adds processing info
        pythonReceived.Add(new StringValue("processed_by", "python_middleware"));
        pythonReceived.Add(new DoubleValue("processing_time_ms", 15.3));
        pythonReceived.SetTarget("dotnet_client", "");

        // Step 4: .NET receives final result
        Console.WriteLine("\n[Step 4] .NET receives final result:");
        var finalV2Json = JsonV2Adapter.ToV2Json(pythonReceived);
        var dotnetFinal = JsonV2Adapter.FromV2Json(finalV2Json);
        Console.WriteLine($"  .NET received container with {dotnetFinal.Units.Count} values");

        // Display final result
        Console.WriteLine("\n  Final values:");
        foreach (var value in dotnetFinal.Units)
        {
            Console.WriteLine($"    {value.Name}: {value.Data()} (type: {value.Type})");
        }
    }

    /// <summary>
    /// Example 7: Test all 15 value types in v2.0 format.
    /// </summary>
    private static void Example7_AllValueTypes()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("Example 7: All 15 Value Types in JSON v2.0");
        Console.WriteLine(new string('=', 80));

        var container = new ValueContainer(
            sourceId: "test",
            sourceSubId: "",
            targetId: "",
            targetSubId: "",
            messageType: "all_types_test"
        );

        // Add all 15 types
        container.Add(new StringValue("null_value", "")); // NULL_VALUE representation
        container.Add(new BoolValue("bool_value", true));
        container.Add(new ShortValue("short_value", -32000));
        container.Add(new UShortValue("ushort_value", 65000));
        container.Add(new IntValue("int_value", -2147483648));
        container.Add(new UIntValue("uint_value", 4294967295));
        container.Add(new LongValue("long_value", -9223372036854775808));
        container.Add(new ULongValue("ulong_value", 18446744073709551615));
        container.Add(new LLongValue("llong_value", -9223372036854775808));
        container.Add(new ULLongValue("ullong_value", 18446744073709551615));
        container.Add(new FloatValue("float_value", 3.14159f));
        container.Add(new DoubleValue("double_value", 2.718281828459045));
        container.Add(new BytesValue("bytes_value", new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF }));
        container.Add(new StringValue("string_value", "Hello, World!"));

        var nested = new ContainerValue("container_value");
        nested.Add(new IntValue("nested_int", 42));
        nested.Add(new StringValue("nested_string", "nested"));
        container.Add(nested);

        // Convert to v2.0
        var v2Json = JsonV2Adapter.ToV2Json(container, pretty: true);
        Console.WriteLine("\nJSON v2.0 with all 15 value types:");
        Console.WriteLine(v2Json);

        // Parse and verify
        var restored = JsonV2Adapter.FromV2Json(v2Json);
        Console.WriteLine($"\nRestored container has {restored.Units.Count} values (all 15 types)");

        // Verify critical values
        var testCases = new[]
        {
            ("short_value", -32000),
            ("uint_value", (int)4294967295),
            ("string_value", "Hello, World!")
        };

        Console.WriteLine("\nVerification:");
        foreach (var (name, expected) in testCases)
        {
            var value = restored.GetValue(name);
            if (value != null)
            {
                var actual = name == "string_value" ? value.ToString() : (object)value.ToInt();
                var match = actual.Equals(expected) ? "✓" : "✗";
                Console.WriteLine($"  {match} {name}: {actual}");
            }
        }
    }

    // Helper method to compare byte arrays
    private static bool CompareBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }
}
