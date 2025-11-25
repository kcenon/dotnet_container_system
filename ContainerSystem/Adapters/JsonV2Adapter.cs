/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.

JSON v2.0 Adapter for cross-language compatibility

This adapter implements the unified JSON v2.0 format for data interchange
between C++, Python, and .NET container system implementations.

Unified JSON v2.0 Format:
{
  "container": {
    "version": "2.0",
    "metadata": {
      "message_type": "user_profile",
      "protocol_version": "1.0.0.0",
      "source": {
        "id": "client",
        "sub_id": "session"
      },
      "target": {
        "id": "server",
        "sub_id": "handler"
      }
    },
    "values": [
      {
        "name": "username",
        "type": 13,
        "type_name": "string",
        "data": "john_doe"
      }
    ]
  }
}
***************************************************************************/

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ContainerSystem.Core;
using ContainerSystem.Values;

namespace ContainerSystem.Adapters;

/// <summary>
/// Adapter for unified JSON v2.0 format compatible across C++, Python, and .NET.
///
/// This adapter provides methods to:
/// - Convert ValueContainer to unified JSON v2.0 format
/// - Parse JSON v2.0 format into ValueContainer
/// - Convert between different JSON formats (C++ nested, Python/.NET flat, v2.0 unified)
/// - Handle backward compatibility with legacy formats
/// </summary>
public static class JsonV2Adapter
{
    // JSON format version constants
    private const string V2_FORMAT_VERSION = "2.0";
    private const string V1_FORMAT_VERSION = "1.0";

    // Type name mapping for human-readable type names
    // Note: Order matches C++ value_types enum (string=12, bytes=13)
    private static readonly Dictionary<ValueTypes, string> TypeNameMap = new()
    {
        { ValueTypes.NullValue, "null" },
        { ValueTypes.BoolValue, "bool" },
        { ValueTypes.ShortValue, "short" },
        { ValueTypes.UShortValue, "ushort" },
        { ValueTypes.IntValue, "int" },
        { ValueTypes.UIntValue, "uint" },
        { ValueTypes.LongValue, "long" },
        { ValueTypes.ULongValue, "ulong" },
        { ValueTypes.LLongValue, "llong" },
        { ValueTypes.ULLongValue, "ullong" },
        { ValueTypes.FloatValue, "float" },
        { ValueTypes.DoubleValue, "double" },
        { ValueTypes.StringValue, "string" },   // 12 - matches C++ string_value
        { ValueTypes.BytesValue, "bytes" },     // 13 - matches C++ bytes_value
        { ValueTypes.ContainerValue, "container" },
        { ValueTypes.ArrayValue, "array" }
    };

    private static readonly Dictionary<string, ValueTypes> ReverseTypeNameMap =
        TypeNameMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    /// <summary>
    /// Convert ValueContainer to unified JSON v2.0 format.
    /// </summary>
    /// <param name="container">ValueContainer to convert</param>
    /// <param name="pretty">If true, format with indentation for readability</param>
    /// <returns>JSON string in v2.0 unified format</returns>
    public static string ToV2Json(ValueContainer container, bool pretty = false)
    {
        var v2Data = new JsonObject
        {
            ["container"] = new JsonObject
            {
                ["version"] = V2_FORMAT_VERSION,
                ["metadata"] = new JsonObject
                {
                    ["message_type"] = container.MessageType,
                    ["protocol_version"] = container.Version,
                    ["source"] = new JsonObject
                    {
                        ["id"] = container.SourceId,
                        ["sub_id"] = container.SourceSubId
                    },
                    ["target"] = new JsonObject
                    {
                        ["id"] = container.TargetId,
                        ["sub_id"] = container.TargetSubId
                    }
                },
                ["values"] = new JsonArray()
            }
        };

        var valuesArray = v2Data["container"]!["values"]!.AsArray();
        foreach (var value in container.Units)
        {
            valuesArray.Add(ValueToV2Dict(value));
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = pretty,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return v2Data.ToJsonString(options);
    }

    /// <summary>
    /// Parse JSON v2.0 format into ValueContainer.
    /// </summary>
    /// <param name="jsonStr">JSON string in v2.0 format</param>
    /// <returns>Parsed ValueContainer</returns>
    /// <exception cref="ArgumentException">If JSON format is invalid</exception>
    public static ValueContainer FromV2Json(string jsonStr)
    {
        var root = JsonNode.Parse(jsonStr);
        if (root == null)
            throw new ArgumentException("Invalid JSON: root is null");

        var containerNode = root["container"];
        if (containerNode == null)
            throw new ArgumentException("Invalid JSON v2.0: missing 'container' field");

        // Check version
        var version = containerNode["version"]?.GetValue<string>();
        if (version != V2_FORMAT_VERSION)
            throw new ArgumentException($"Unsupported JSON version: {version} (expected {V2_FORMAT_VERSION})");

        // Parse metadata
        var metadata = containerNode["metadata"];
        if (metadata == null)
            throw new ArgumentException("Invalid JSON v2.0: missing 'metadata' field");

        var messageType = metadata["message_type"]?.GetValue<string>() ?? string.Empty;
        var protocolVersion = metadata["protocol_version"]?.GetValue<string>() ?? "1.0.0.0";

        var source = metadata["source"];
        var sourceId = source?["id"]?.GetValue<string>() ?? string.Empty;
        var sourceSubId = source?["sub_id"]?.GetValue<string>() ?? string.Empty;

        var target = metadata["target"];
        var targetId = target?["id"]?.GetValue<string>() ?? string.Empty;
        var targetSubId = target?["sub_id"]?.GetValue<string>() ?? string.Empty;

        // Create container
        var container = new ValueContainer(
            sourceId: sourceId,
            sourceSubId: sourceSubId,
            targetId: targetId,
            targetSubId: targetSubId,
            messageType: messageType,
            version: protocolVersion
        );

        // Parse values
        var values = containerNode["values"]?.AsArray();
        if (values != null)
        {
            foreach (var valueNode in values)
            {
                if (valueNode != null)
                {
                    var value = V2DictToValue(valueNode);
                    if (value != null)
                        container.Add(value);
                }
            }
        }

        return container;
    }

    /// <summary>
    /// Convert C++ nested JSON format to ValueContainer.
    /// </summary>
    /// <param name="jsonStr">JSON string in C++ format</param>
    /// <returns>Parsed ValueContainer</returns>
    public static ValueContainer FromCppJson(string jsonStr)
    {
        var root = JsonNode.Parse(jsonStr);
        if (root == null)
            throw new ArgumentException("Invalid JSON: root is null");

        // Parse header
        var header = root["header"];
        if (header == null)
            throw new ArgumentException("Invalid C++ JSON: missing 'header' field");

        var messageType = header["message_type"]?.GetValue<string>() ?? string.Empty;
        var version = header["version"]?.GetValue<string>() ?? "1.0.0.0";
        var sourceId = header["source_id"]?.GetValue<string>() ?? string.Empty;
        var sourceSubId = header["source_sub_id"]?.GetValue<string>() ?? string.Empty;
        var targetId = header["target_id"]?.GetValue<string>() ?? string.Empty;
        var targetSubId = header["target_sub_id"]?.GetValue<string>() ?? string.Empty;

        var container = new ValueContainer(
            sourceId: sourceId,
            sourceSubId: sourceSubId,
            targetId: targetId,
            targetSubId: targetSubId,
            messageType: messageType,
            version: version
        );

        // Parse values (C++ format: values is an object with keys)
        var values = root["values"]?.AsObject();
        if (values != null)
        {
            foreach (var kvp in values)
            {
                var name = kvp.Key;
                var valueNode = kvp.Value;
                if (valueNode != null)
                {
                    var value = CppValueToValue(name, valueNode);
                    if (value != null)
                        container.Add(value);
                }
            }
        }

        return container;
    }

    /// <summary>
    /// Convert ValueContainer to C++ nested JSON format.
    /// </summary>
    /// <param name="container">ValueContainer to convert</param>
    /// <param name="pretty">If true, format with indentation</param>
    /// <returns>JSON string in C++ nested format</returns>
    public static string ToCppJson(ValueContainer container, bool pretty = false)
    {
        var cppData = new JsonObject
        {
            ["header"] = new JsonObject
            {
                ["message_type"] = container.MessageType,
                ["version"] = container.Version,
                ["source_id"] = container.SourceId,
                ["source_sub_id"] = container.SourceSubId,
                ["target_id"] = container.TargetId,
                ["target_sub_id"] = container.TargetSubId
            },
            ["values"] = new JsonObject()
        };

        var valuesObj = cppData["values"]!.AsObject();
        foreach (var value in container.Units)
        {
            var valueData = new JsonObject
            {
                ["type"] = (int)value.Type,
                ["data"] = GetValueData(value)
            };
            valuesObj[value.Name] = valueData;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = pretty,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return cppData.ToJsonString(options);
    }

    /// <summary>
    /// Detect JSON format type automatically.
    /// </summary>
    /// <param name="jsonStr">JSON string to detect</param>
    /// <returns>Format type: "v2.0", "cpp", "python", "unknown", or "invalid"</returns>
    public static string DetectFormat(string jsonStr)
    {
        try
        {
            var root = JsonNode.Parse(jsonStr);
            if (root == null)
                return "invalid";

            // Check for v2.0 format
            if (root["container"] != null && root["container"]!["version"] != null)
            {
                var version = root["container"]!["version"]!.GetValue<string>();
                if (version == V2_FORMAT_VERSION)
                    return "v2.0";
            }

            // Check for C++ format (nested header object)
            if (root["header"] != null && root["values"] is JsonObject)
                return "cpp";

            // Check for Python/.NET format (flat with values array)
            if (root["message_type"] != null && root["values"] is JsonArray)
                return "python";

            return "unknown";
        }
        catch
        {
            return "invalid";
        }
    }

    /// <summary>
    /// Convert between different JSON formats automatically.
    /// </summary>
    /// <param name="jsonStr">Input JSON string</param>
    /// <param name="targetFormat">Target format: "v2.0", "cpp", or "python"</param>
    /// <param name="pretty">Format output with indentation</param>
    /// <returns>JSON string in target format</returns>
    public static string ConvertFormat(string jsonStr, string targetFormat, bool pretty = false)
    {
        var sourceFormat = DetectFormat(jsonStr);

        // Parse to container based on source format
        ValueContainer container = sourceFormat switch
        {
            "v2.0" => FromV2Json(jsonStr),
            "cpp" => FromCppJson(jsonStr),
            "python" => new ValueContainer(dataString: jsonStr),
            _ => throw new ArgumentException($"Unsupported source format: {sourceFormat}")
        };

        // Convert to target format
        return targetFormat.ToLower() switch
        {
            "v2.0" => ToV2Json(container, pretty),
            "cpp" => ToCppJson(container, pretty),
            "python" => container.ToJson(),
            _ => throw new ArgumentException($"Unsupported target format: {targetFormat}")
        };
    }

    // Private helper methods

    private static JsonObject ValueToV2Dict(Value value)
    {
        var v2Value = new JsonObject
        {
            ["name"] = value.Name,
            ["type"] = (int)value.Type,
            ["type_name"] = TypeNameMap.GetValueOrDefault(value.Type, "unknown")
        };

        // Handle different value types
        if (value is ContainerValue containerValue)
        {
            var childrenArray = new JsonArray();
            foreach (var child in containerValue.Children())
            {
                childrenArray.Add(ValueToV2Dict(child));
            }
            v2Value["children"] = childrenArray;
            v2Value["child_count"] = containerValue.ChildCount;
        }
        else if (value is BytesValue)
        {
            // Base64 encode binary data
            var bytes = value.ToBytes();
            v2Value["data"] = Convert.ToBase64String(bytes);
            v2Value["encoding"] = "base64";
        }
        else
        {
            v2Value["data"] = GetValueData(value);
        }

        return v2Value;
    }

    private static Value? V2DictToValue(JsonNode valueNode)
    {
        var name = valueNode["name"]?.GetValue<string>() ?? string.Empty;
        var typeId = valueNode["type"]?.GetValue<int>() ?? 0;
        var valueType = (ValueTypes)typeId;

        // Handle container type
        if (valueType == ValueTypes.ContainerValue)
        {
            var container = new ContainerValue(name);
            var children = valueNode["children"]?.AsArray();
            if (children != null)
            {
                foreach (var childNode in children)
                {
                    if (childNode != null)
                    {
                        var child = V2DictToValue(childNode);
                        if (child != null)
                            container.Add(child);
                    }
                }
            }
            return container;
        }

        // Handle binary data
        if (valueType == ValueTypes.BytesValue)
        {
            var encoding = valueNode["encoding"]?.GetValue<string>();
            var dataStr = valueNode["data"]?.GetValue<string>() ?? string.Empty;

            if (encoding == "base64")
            {
                var bytes = Convert.FromBase64String(dataStr);
                return new BytesValue(name, bytes);
            }
        }

        // Handle other types
        var data = valueNode["data"];
        if (data == null)
            return null;

        return valueType switch
        {
            ValueTypes.BoolValue => new BoolValue(name, data.GetValue<bool>()),
            ValueTypes.ShortValue => new ShortValue(name, data.GetValue<short>()),
            ValueTypes.UShortValue => new UShortValue(name, data.GetValue<ushort>()),
            ValueTypes.IntValue => new IntValue(name, data.GetValue<int>()),
            ValueTypes.UIntValue => new UIntValue(name, data.GetValue<uint>()),
            ValueTypes.LongValue => new LongValue(name, data.GetValue<long>()),
            ValueTypes.ULongValue => new ULongValue(name, data.GetValue<ulong>()),
            ValueTypes.LLongValue => new LLongValue(name, data.GetValue<long>()),
            ValueTypes.ULLongValue => new ULLongValue(name, data.GetValue<ulong>()),
            ValueTypes.FloatValue => new FloatValue(name, data.GetValue<float>()),
            ValueTypes.DoubleValue => new DoubleValue(name, data.GetValue<double>()),
            ValueTypes.StringValue => new StringValue(name, data.GetValue<string>()),
            _ => null
        };
    }

    private static Value? CppValueToValue(string name, JsonNode valueNode)
    {
        var typeId = valueNode["type"]?.GetValue<int>() ?? 0;
        var valueType = (ValueTypes)typeId;
        var dataStr = valueNode["data"]?.GetValue<string>() ?? string.Empty;

        return valueType switch
        {
            ValueTypes.BoolValue => new BoolValue(name, bool.Parse(dataStr)),
            ValueTypes.ShortValue => new ShortValue(name, short.Parse(dataStr)),
            ValueTypes.UShortValue => new UShortValue(name, ushort.Parse(dataStr)),
            ValueTypes.IntValue => new IntValue(name, int.Parse(dataStr)),
            ValueTypes.UIntValue => new UIntValue(name, uint.Parse(dataStr)),
            ValueTypes.LongValue => new LongValue(name, long.Parse(dataStr)),
            ValueTypes.ULongValue => new ULongValue(name, ulong.Parse(dataStr)),
            ValueTypes.LLongValue => new LLongValue(name, long.Parse(dataStr)),
            ValueTypes.ULLongValue => new ULLongValue(name, ulong.Parse(dataStr)),
            ValueTypes.FloatValue => new FloatValue(name, float.Parse(dataStr)),
            ValueTypes.DoubleValue => new DoubleValue(name, double.Parse(dataStr)),
            ValueTypes.StringValue => new StringValue(name, dataStr),
            ValueTypes.BytesValue => new BytesValue(name, Convert.FromBase64String(dataStr)),
            _ => null
        };
    }

    private static JsonNode GetValueData(Value value)
    {
        return value.Type switch
        {
            ValueTypes.BoolValue => JsonValue.Create(value.ToBoolean()),
            ValueTypes.ShortValue => JsonValue.Create(value.ToShort()),
            ValueTypes.UShortValue => JsonValue.Create(value.ToUShort()),
            ValueTypes.IntValue => JsonValue.Create(value.ToInt()),
            ValueTypes.UIntValue => JsonValue.Create(value.ToUInt()),
            ValueTypes.LongValue => JsonValue.Create(value.ToLong()),
            ValueTypes.ULongValue => JsonValue.Create(value.ToULong()),
            ValueTypes.LLongValue => JsonValue.Create(value.ToLong()),
            ValueTypes.ULLongValue => JsonValue.Create(value.ToULong()),
            ValueTypes.FloatValue => JsonValue.Create(value.ToFloat()),
            ValueTypes.DoubleValue => JsonValue.Create(value.ToDouble()),
            ValueTypes.StringValue => JsonValue.Create(value.ToString()),
            ValueTypes.BytesValue => JsonValue.Create(Convert.ToBase64String(value.ToBytes())),
            _ => JsonValue.Create(value.Data())
        };
    }
}
