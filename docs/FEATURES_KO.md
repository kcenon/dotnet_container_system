# 기능

> **언어:** [English](FEATURES.md) | **한국어**

.NET Container System의 전체 기능 문서입니다.

---

## 개요

.NET Container System은 크로스 언어 호환성을 갖춘 메시지 직렬화 및 데이터 저장을 위한 타입 안전, 고성능 컨테이너 프레임워크를 제공합니다.

---

## 핵심 기능

### 1. 타입 안전한 값 시스템

컴파일 타임 타입 검사가 가능한 16개의 고유 값 타입:

```csharp
// Null
container.Add(new NullValue("empty"));

// Boolean
container.Add(new BoolValue("active", true));

// 정수 (16비트)
container.Add(new ShortValue("small", 100));
container.Add(new UShortValue("unsigned_small", 200));

// 정수 (32비트)
container.Add(new IntValue("count", 1000));
container.Add(new UIntValue("unsigned_count", 2000));

// 정수 (32비트 범위, C++ 호환)
container.Add(new LongValue("medium", 100000));
container.Add(new ULongValue("unsigned_medium", 200000));

// 정수 (64비트)
container.Add(new LLongValue("large", 5000000000L));
container.Add(new ULLongValue("unsigned_large", 10000000000UL));

// 부동소수점
container.Add(new FloatValue("ratio", 0.5f));
container.Add(new DoubleValue("precise", 3.14159265359));

// 문자열
container.Add(new StringValue("name", "Hello World"));

// 바이너리
container.Add(new BytesValue("data", new byte[] { 1, 2, 3 }));

// 중첩 컨테이너
container.Add(new ContainerValue("nested", innerContainer));

// 배열
container.Add(new ArrayValue("list"));
```

### 2. 메시지 컨테이너

라우팅 메타데이터가 포함된 고수준 컨테이너:

```csharp
var container = new ValueContainer();

// 메시지 타입
container.MessageType = "user_profile";

// 소스 식별
container.SetSource("client_app", "session_123");

// 대상 식별
container.SetTarget("user_service", "profile_handler");

// 버전 추적
container.Version = "1.0.0";
```

### 3. 다중 직렬화 포맷

#### JSON 직렬화

```csharp
// JSON으로 직렬화
string json = container.Serialize();

// JSON에서 역직렬화
var restored = new ValueContainer(json);
```

#### Binary 직렬화

```csharp
// 바이너리로 직렬화
byte[] binary = container.Store.Serialize();

// 바이너리에서 역직렬화
container.Store.Deserialize(binary);
```

#### XML 직렬화

```csharp
// XML로 직렬화
string xml = container.ToXml();
```

#### JSON v2.0 (크로스 언어)

```csharp
using ContainerSystem.Adapters;

// JSON v2.0 포맷으로
string jsonV2 = JsonV2Adapter.ToJson(container);

// JSON v2.0 포맷에서
var restored = JsonV2Adapter.FromJson(jsonV2);
```

### 4. 스레드 안전성

내장된 스레드 안전 연산:

```csharp
// 스레드 안전한 컨테이너 연산
var container = new ValueContainer();

Parallel.For(0, 1000, i =>
{
    container.Add(new IntValue($"item_{i}", i));
});

// 스레드 안전한 값 저장소
var store = new ValueStore(threadSafe: true);
```

### 5. 중첩 컨테이너

계층적 데이터 구조:

```csharp
// 중첩 구조 생성
var address = new ValueContainer();
address.Add(new StringValue("street", "강남대로 123"));
address.Add(new StringValue("city", "서울"));

var user = new ValueContainer();
user.Add(new StringValue("name", "홍길동"));
user.Add(new ContainerValue("address", address));
```

### 6. 동일 이름의 다중 값

동일한 키에 여러 값을 저장하는 기능 (C++/Rust 호환):

```csharp
// 동일 키에 여러 값 추가 (덮어쓰기가 아닌 추가)
container.Add(new StringValue("tag", "중요"));
container.Add(new StringValue("tag", "긴급"));
container.Add(new StringValue("tag", "검토"));

// 첫 번째 값 조회 (이전 버전 호환)
var first = container.GetValue("tag");  // "중요" 반환

// 키에 대한 모든 값 조회
var allTags = container.Store.GetValues("tag");  // 3개 값 모두 반환
foreach (var tag in allTags)
{
    Console.WriteLine(tag.ToString());
}

// 키에 대한 값 개수 조회
int count = container.Store.GetValueCount("tag");  // 3 반환

// 단일 값 설정 (기존 모든 값 대체)
container.Store.Set("tag", new StringValue("tag", "단일"));

// 특정 값만 제거
container.Store.RemoveValue("tag", specificValue);

// 전체 값 수 vs 고유 키 수
Console.WriteLine($"고유 키 수: {container.Store.Size}");
Console.WriteLine($"전체 값 수: {container.Store.TotalValueCount}");
```

**주요 기능:**
- `Add()` - 값 추가 (다중 값 의미론)
- `Set()` - 모든 값 대체 (단일 값 의미론)
- `Get()` - 첫 번째 값 반환 (이전 버전 호환)
- `GetValues()` - 모든 값을 `IReadOnlyList<Value>`로 반환
- `GetValueCount()` - 키에 대한 값 개수 반환
- `RemoveValue()` - 특정 값 제거, 빈 키 자동 정리
- `TotalValueCount` - 모든 키의 전체 값 수

---

## 값 타입 상세

### 숫자 타입

| 타입 | .NET 타입 | 범위 | 바이트 |
|------|-----------|------|--------|
| ShortValue | short | -32,768 ~ 32,767 | 2 |
| UShortValue | ushort | 0 ~ 65,535 | 2 |
| IntValue | int | -2³¹ ~ 2³¹-1 | 4 |
| UIntValue | uint | 0 ~ 2³²-1 | 4 |
| LongValue | int | -2³¹ ~ 2³¹-1 (C++ 호환) | 4 |
| ULongValue | uint | 0 ~ 2³²-1 (C++ 호환) | 4 |
| LLongValue | long | -2⁶³ ~ 2⁶³-1 | 8 |
| ULLongValue | ulong | 0 ~ 2⁶⁴-1 | 8 |
| FloatValue | float | ±3.4 × 10³⁸ | 4 |
| DoubleValue | double | ±1.7 × 10³⁰⁸ | 8 |

### 타입 변환

모든 값 타입은 안전한 타입 변환을 지원합니다:

```csharp
var value = container.GetValue("number");

// 변환
bool b = value.ToBoolean();
int i = value.ToInt();
long l = value.ToLong();
float f = value.ToFloat();
double d = value.ToDouble();
string s = value.ToString();
byte[] bytes = value.ToBytes();
```

### 타입 검사

```csharp
var value = container.GetValue("data");

if (value.IsNull()) { /* ... */ }
if (value.IsBoolean()) { /* ... */ }
if (value.IsNumeric()) { /* ... */ }
if (value.IsString()) { /* ... */ }
if (value.IsBytes()) { /* ... */ }
if (value.IsContainer()) { /* ... */ }
```

---

## 고급 기능

### 통계 추적

```csharp
var container = new ValueContainer();

// 연산...
container.Add(new IntValue("x", 1));
var val = container.GetValue("x");

// 통계 확인
Console.WriteLine($"읽기 횟수: {container.Store.ReadCount}");
Console.WriteLine($"쓰기 횟수: {container.Store.WriteCount}");
Console.WriteLine($"직렬화 횟수: {container.Store.SerializationCount}");
```

### 부모-자식 관계

```csharp
var parent = new ContainerValue("parent", new ValueContainer());
var child = new StringValue("child", "value");

parent.AddChild(child);

// 탐색
Console.WriteLine(child.Parent?.Name); // "parent"
Console.WriteLine(parent.ChildCount);   // 1
```

---

## 크로스 언어 호환성

### 지원 언어

| 언어 | 라이브러리 | 상태 |
|------|-----------|------|
| C++ | container_system | ✅ 전체 |
| Python | python_container_system | ✅ 전체 |
| Go | - | ✅ JSON v2.0 통해 |
| Rust | - | ✅ JSON v2.0 통해 |
| Node.js | - | ✅ JSON v2.0 통해 |

### 타입 매핑

| .NET 타입 | C++ 타입 | Python 타입 |
|-----------|----------|-------------|
| NullValue | null_value | NullValue |
| BoolValue | bool_value | BoolValue |
| IntValue | int32_value | IntValue |
| LLongValue | int64_value | LLongValue |
| DoubleValue | double_value | DoubleValue |
| StringValue | string_value | StringValue |
| BytesValue | bytes_value | BytesValue |
| ContainerValue | container_value | ContainerValue |

---

## C++ 버전과의 비교

| 기능 | C++ | .NET |
|------|-----|------|
| 값 타입 | 15 | 16 (+ ArrayValue) |
| Binary 직렬화 | ✅ | ✅ |
| JSON 직렬화 | ✅ | ✅ |
| XML 직렬화 | ✅ | ✅ |
| SIMD 최적화 | ✅ | ❌ |
| 메모리 풀링 | ✅ | ❌ |
| 스레드 안전성 | ✅ | ✅ |
| 크로스 언어 | ✅ | ✅ |

---

## 참조

- [빠른 시작](guides/QUICK_START.md) - 시작하기
- [아키텍처](ARCHITECTURE.md) - 시스템 설계
- [API 레퍼런스](API_REFERENCE.md) - 전체 API
- [벤치마크](performance/BENCHMARKS.md) - 성능 메트릭
