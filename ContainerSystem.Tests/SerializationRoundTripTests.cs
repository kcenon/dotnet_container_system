/***************************************************************************
BSD 3-Clause License

Copyright (c) 2025, kcenon
All rights reserved.
***************************************************************************/

using Xunit;
using ContainerSystem.Core;
using ContainerSystem.Values;

namespace ContainerSystem.Tests;

/// <summary>
/// Integration tests for serialization round-trip operations.
/// Ensures data integrity across serialize/deserialize cycles.
/// </summary>
public class SerializationRoundTripTests
{
    [Fact]
    public void ArrayValue_Empty_RoundTripSucceeds()
    {
        // Arrange
        var original = new ArrayValue("empty_array");

        // Act
        var serialized = original.Serialize();
        var deserialized = ArrayValue.Deserialize("empty_array", serialized);

        // Assert
        Assert.Equal(original.Count, deserialized.Count);
        Assert.Equal(0, deserialized.Count);
    }

    [Fact]
    public void ArrayValue_WithIntegers_RoundTripSucceeds()
    {
        // Arrange
        var original = new ArrayValue("numbers");
        original.Append(new IntValue("num1", 10));
        original.Append(new IntValue("num2", 20));
        original.Append(new IntValue("num3", 30));

        // Act
        var serialized = original.Serialize();
        var deserialized = ArrayValue.Deserialize("numbers", serialized);

        // Assert
        Assert.Equal(3, deserialized.Count);
        Assert.Equal(10, deserialized[0].ToInt());
        Assert.Equal(20, deserialized[1].ToInt());
        Assert.Equal(30, deserialized[2].ToInt());
    }

    [Fact]
    public void ArrayValue_MixedTypes_RoundTripSucceeds()
    {
        // Arrange
        var original = new ArrayValue("mixed");
        original.Append(new IntValue("age", 25));
        original.Append(new StringValue("name", "Alice"));
        original.Append(new DoubleValue("score", 95.5));
        original.Append(new BoolValue("active", true));

        // Act
        var serialized = original.Serialize();
        var deserialized = ArrayValue.Deserialize("mixed", serialized);

        // Assert
        Assert.Equal(4, deserialized.Count);
        Assert.Equal(25, deserialized[0].ToInt());
        Assert.Equal("Alice", deserialized[1].ToString());
        Assert.Equal(95.5, deserialized[2].ToDouble(), 2);
        Assert.True(deserialized[3].ToBoolean());
    }

    [Fact]
    public void ArrayValue_Nested_RoundTripSucceeds()
    {
        // Arrange
        var inner = new ArrayValue("inner");
        inner.Append(new IntValue("x", 1));
        inner.Append(new IntValue("y", 2));

        var original = new ArrayValue("outer");
        original.Append(new StringValue("label", "coordinates"));
        original.Append(inner);

        // Act
        var serialized = original.Serialize();
        var deserialized = ArrayValue.Deserialize("outer", serialized);

        // Assert
        Assert.Equal(2, deserialized.Count);
        Assert.Equal("coordinates", deserialized[0].ToString());

        var deserializedInner = deserialized[1] as ArrayValue;
        Assert.NotNull(deserializedInner);
        Assert.Equal(2, deserializedInner.Count);
        Assert.Equal(1, deserializedInner[0].ToInt());
        Assert.Equal(2, deserializedInner[1].ToInt());
    }

    [Fact]
    public void ContainerValue_Empty_RoundTripSucceeds()
    {
        // Arrange
        var original = new ContainerValue("empty_container");

        // Act
        var serialized = original.Serialize();
        var deserialized = ValueFactory.Deserialize(
            ValueFactory.SerializeWithHeader(original)) as ContainerValue;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(0, deserialized.ChildCount);
    }

    [Fact]
    public void ContainerValue_WithValues_RoundTripSucceeds()
    {
        // Arrange
        var original = new ContainerValue("user");
        original.Add(new StringValue("username", "john_doe"));
        original.Add(new IntValue("age", 30));
        original.Add(new BoolValue("active", true));

        // Act
        var serialized = ValueFactory.SerializeWithHeader(original);
        var deserialized = ValueFactory.Deserialize(serialized) as ContainerValue;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(3, deserialized.ChildCount);

        var username = deserialized.GetValue("username");
        Assert.NotNull(username);
        Assert.Equal("john_doe", username.ToString());

        var age = deserialized.GetValue("age");
        Assert.NotNull(age);
        Assert.Equal(30, age.ToInt());

        var active = deserialized.GetValue("active");
        Assert.NotNull(active);
        Assert.True(active.ToBoolean());
    }

    [Fact]
    public void ContainerValue_Nested_RoundTripSucceeds()
    {
        // Arrange
        var address = new ContainerValue("address");
        address.Add(new StringValue("street", "123 Main St"));
        address.Add(new StringValue("city", "New York"));
        address.Add(new IntValue("zip", 10001));

        var original = new ContainerValue("person");
        original.Add(new StringValue("name", "Alice"));
        original.Add(address);

        // Act
        var serialized = ValueFactory.SerializeWithHeader(original);
        var deserialized = ValueFactory.Deserialize(serialized) as ContainerValue;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.ChildCount);

        var name = deserialized.GetValue("name");
        Assert.Equal("Alice", name?.ToString());

        var deserializedAddress = deserialized.GetValue("address") as ContainerValue;
        Assert.NotNull(deserializedAddress);
        Assert.Equal(3, deserializedAddress.ChildCount);

        var street = deserializedAddress.GetValue("street");
        Assert.Equal("123 Main St", street?.ToString());
    }

    [Fact]
    public void ValueContainer_Complete_RoundTripSucceeds()
    {
        // Arrange
        var original = new ValueContainer(
            sourceId: "client1",
            sourceSubId: "session123",
            targetId: "server1",
            targetSubId: "handler1",
            messageType: "user_data",
            version: "2.0.0"
        );

        original.Add(new StringValue("username", "test_user"));
        original.Add(new IntValue("id", 12345));
        original.Add(new DoubleValue("balance", 1000.50));

        // Act
        var json = original.Serialize();
        var deserialized = new ValueContainer(json);

        // Assert
        Assert.Equal(original.MessageType, deserialized.MessageType);
        Assert.Equal(original.Version, deserialized.Version);
        Assert.Equal(original.SourceId, deserialized.SourceId);
        Assert.Equal(original.SourceSubId, deserialized.SourceSubId);
        Assert.Equal(original.TargetId, deserialized.TargetId);
        Assert.Equal(original.TargetSubId, deserialized.TargetSubId);
        Assert.Equal(original.Count, deserialized.Count);

        var username = deserialized.GetValue("username");
        Assert.Equal("test_user", username?.ToString());

        var id = deserialized.GetValue("id");
        Assert.Equal(12345, id?.ToInt());

        var balance = deserialized.GetValue("balance");
        Assert.Equal(1000.50, balance?.ToDouble(), 2);
    }

    [Fact]
    public void LongValue_BoundaryValues_RoundTripSucceeds()
    {
        // Arrange - 32-bit boundary values
        var minValue = new LongValue("min", int.MinValue);
        var maxValue = new LongValue("max", int.MaxValue);

        // Act
        var minSerialized = ValueFactory.SerializeWithHeader(minValue);
        var maxSerialized = ValueFactory.SerializeWithHeader(maxValue);

        var minDeserialized = ValueFactory.Deserialize(minSerialized);
        var maxDeserialized = ValueFactory.Deserialize(maxSerialized);

        // Assert
        Assert.Equal(int.MinValue, minDeserialized.ToInt());
        Assert.Equal(int.MaxValue, maxDeserialized.ToInt());
    }

    [Fact]
    public void LLongValue_64BitValues_RoundTripSucceeds()
    {
        // Arrange - Values beyond 32-bit range
        var largeValue = new LLongValue("large", 5_000_000_000L);
        var minValue = new LLongValue("min", long.MinValue);
        var maxValue = new LLongValue("max", long.MaxValue);

        // Act & Assert
        var largeSerialized = ValueFactory.SerializeWithHeader(largeValue);
        var largeDeserialized = ValueFactory.Deserialize(largeSerialized);
        Assert.Equal(5_000_000_000L, largeDeserialized.ToLong());

        var minSerialized = ValueFactory.SerializeWithHeader(minValue);
        var minDeserialized = ValueFactory.Deserialize(minSerialized);
        Assert.Equal(long.MinValue, minDeserialized.ToLong());

        var maxSerialized = ValueFactory.SerializeWithHeader(maxValue);
        var maxDeserialized = ValueFactory.Deserialize(maxSerialized);
        Assert.Equal(long.MaxValue, maxDeserialized.ToLong());
    }

    [Fact]
    public void BytesValue_BinaryData_RoundTripSucceeds()
    {
        // Arrange
        byte[] binaryData = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            binaryData[i] = (byte)i;
        }

        var original = new BytesValue("binary", binaryData);

        // Act
        var serialized = ValueFactory.SerializeWithHeader(original);
        var deserialized = ValueFactory.Deserialize(serialized);

        // Assert
        var deserializedBytes = deserialized.ToBytes();
        Assert.Equal(binaryData.Length, deserializedBytes.Length);
        Assert.Equal(binaryData, deserializedBytes);
    }

    [Fact]
    public void ComplexStructure_DeepNesting_RoundTripSucceeds()
    {
        // Arrange - Create a complex nested structure
        var level3 = new ContainerValue("level3");
        level3.Add(new IntValue("depth", 3));

        var level2Array = new ArrayValue("level2_array");
        level2Array.Append(new StringValue("item1", "deep"));
        level2Array.Append(level3);

        var level1 = new ContainerValue("level1");
        level1.Add(new StringValue("name", "root"));
        level1.Add(level2Array);

        // Act
        var serialized = ValueFactory.SerializeWithHeader(level1);
        var deserialized = ValueFactory.Deserialize(serialized) as ContainerValue;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("root", deserialized.GetValue("name")?.ToString());

        var deserializedArray = deserialized.GetValue("level2_array") as ArrayValue;
        Assert.NotNull(deserializedArray);
        Assert.Equal(2, deserializedArray.Count);

        var deserializedLevel3 = deserializedArray[1] as ContainerValue;
        Assert.NotNull(deserializedLevel3);
        Assert.Equal(3, deserializedLevel3.GetValue("depth")?.ToInt());
    }
}
