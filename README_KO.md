[![CI](https://github.com/kcenon/dotnet_container_system/actions/workflows/ci.yml/badge.svg)](https://github.com/kcenon/dotnet_container_system/actions/workflows/ci.yml)
[![Code Coverage](https://github.com/kcenon/dotnet_container_system/actions/workflows/coverage.yml/badge.svg)](https://github.com/kcenon/dotnet_container_system/actions/workflows/coverage.yml)

# .NET Container System

> **언어:** [English](README.md) | **한국어**

## 개요

메시지 직렬화와 데이터 저장을 위한 고성능, 타입 안전한 컨테이너 시스템입니다. C++ [container_system](https://github.com/kcenon/container_system)의 .NET 동등 버전으로, 기업용 메시징 애플리케이션을 위한 크로스 언어 호환성을 제공합니다.

**주요 특징**:
- **타입 안전**: 컴파일 타임 검사가 가능한 16개의 고유 값 타입
- **크로스 언어**: C++, Python, Go, Rust, Node.js와 바이너리 호환
- **스레드 안전**: ReaderWriterLockSlim을 활용한 내장 동시 접근 지원
- **고성능**: O(1) 키 조회, 효율적인 직렬화
- **다중 포맷**: Binary, JSON, XML 직렬화 지원

### 미션

.NET 개발자를 위해 크로스 플랫폼 데이터 직렬화를 **타입 안전**하고, **효율적**이며, **상호 운용 가능**하게 만듭니다.

## 빠른 시작

### 기본 사용 예제

```csharp
using ContainerSystem.Core;
using ContainerSystem.Values;

// 메타데이터가 포함된 컨테이너 생성
var container = new ValueContainer();
container.MessageType = "user_profile";
container.SetSource("client_app", "session_123");
container.SetTarget("user_service", "handler");

// 타입이 지정된 값 추가
container.Add(new StringValue("username", "john_doe"));
container.Add(new IntValue("age", 30));
container.Add(new DoubleValue("balance", 1500.75));
container.Add(new BoolValue("is_active", true));

// JSON으로 직렬화
string json = container.Serialize();

// 역직렬화
var restored = new ValueContainer(json);
var username = restored.GetValue("username")?.ToString();
```

### Fluent 빌더 패턴

```csharp
using ContainerSystem.Messaging;

// Fluent 빌더를 사용한 컨테이너 생성
var container = new ContainerBuilder()
    .WithSource("client_app", "session_123")
    .WithTarget("user_service", "handler")
    .WithMessageType("user_profile")
    .WithValue(new StringValue("username", "john_doe"))
    .WithValue(new IntValue("age", 30))
    .WithThreadSafety()
    .Build();

// 또는 편의 팩토리 메서드 사용
var request = ContainerBuilder
    .CreateRequest("client", "server")
    .WithValue(new StringValue("action", "login"))
    .Build();
```

### 필수 조건

- **.NET SDK**: 8.0 이상
- **IDE**: Visual Studio 2022, VS Code, 또는 JetBrains Rider

### 설치

```bash
# NuGet 패키지 (배포 시)
dotnet add package ContainerSystem

# 또는 소스에서 빌드
git clone https://github.com/kcenon/dotnet_container_system.git
cd dotnet_container_system
./scripts/build.sh      # Linux/macOS
# 또는
scripts\build.bat       # Windows (CMD)
.\scripts\build.ps1     # Windows (PowerShell)
```

## 핵심 기능

### 타입 안전한 값 시스템
- **16개의 내장 타입**: null부터 중첩 컨테이너까지
- **컴파일 타임 검사**: 강력한 타입 지정으로 런타임 오류 방지
- **안전한 변환**: null 처리가 포함된 ToInt(), ToDouble(), ToString()

### 다중 직렬화 포맷
- **Binary**: 빠르고 컴팩트한 직렬화
- **JSON**: 사람이 읽기 쉽고, 디버깅에 유용
- **XML**: 기업 시스템을 위한 구조화된 포맷
- **JSON v2.0**: 크로스 언어 호환 포맷

### 스레드 안전성
- **ReaderWriterLockSlim**: 효율적인 동시 접근
- **스레드 안전 연산**: 여러 스레드에서 안전하게 Add, Get, Serialize
- **통계 추적**: 읽기/쓰기/직렬화 카운트

### 크로스 언어 호환성
- **C++ 호환**: container_system과 호환
- **Python 호환**: python_container_system과 호환
- **범용 JSON**: 모든 언어를 위한 JSON v2.0 어댑터

📚 **[전체 기능 보기 →](docs/FEATURES.md)**

## 성능

| 연산 | 처리량 | 비고 |
|------|--------|------|
| 컨테이너 생성 | ~1M/초 | 빈 컨테이너 |
| 값 추가 | ~2M/초 | 단일 값 |
| JSON 직렬화 | ~200K/초 | 10개 값 |
| Binary 직렬화 | ~500K/초 | 10개 값 |
| 값 조회 | ~5M/초 | 키로 조회 |

⚡ **[전체 벤치마크 →](docs/performance/BENCHMARKS.md)**

## 아키텍처

```
                    ┌─────────────────┐
                    │ common_system   │
                    │     (공유)      │
                    └────────┬────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌────────────────┐  ┌────────────────┐  ┌────────────────┐
│container_system│  │  .NET 동등버전  │  │ Python 동등버전│
│     (C++)      │  │  (이 프로젝트)  │  │    (Python)    │
└────────────────┘  └────────────────┘  └────────────────┘
```

🏗️ **[아키텍처 가이드 →](docs/ARCHITECTURE.md)**

## 문서

### 시작하기
- 📖 [빠른 시작 가이드](docs/guides/QUICK_START.md)
- 🔧 [빌드 가이드](docs/guides/BUILD_GUIDE.md)
- ✅ [모범 사례](docs/guides/BEST_PRACTICES.md)
- 🔍 [문제 해결](docs/guides/TROUBLESHOOTING.md)

### 핵심 문서
- 📚 [기능](docs/FEATURES.md) - 전체 기능 문서
- ⚡ [벤치마크](docs/performance/BENCHMARKS.md) - 성능 분석
- 📦 [프로젝트 구조](docs/PROJECT_STRUCTURE.md) - 코드 구성
- 📘 [API 레퍼런스](docs/API_REFERENCE.md) - 전체 API 문서

### 고급 주제
- 🔗 [크로스 언어 호환성](docs/advanced/COMPATIBILITY.md) - 상호 운용성
- 📋 [FAQ](docs/guides/FAQ.md) - 자주 묻는 질문
- 📝 [변경 이력](docs/CHANGELOG.md) - 버전 히스토리

## 값 타입

포괄적인 데이터 처리를 위한 16개의 고유 값 타입:

| 카테고리 | 타입 | 크기 |
|----------|------|------|
| **Null** | NullValue | 0 바이트 |
| **Boolean** | BoolValue | 1 바이트 |
| **16비트** | ShortValue, UShortValue | 2 바이트 |
| **32비트** | IntValue, UIntValue, LongValue*, ULongValue* | 4 바이트 |
| **64비트** | LLongValue, ULLongValue | 8 바이트 |
| **부동소수점** | FloatValue, DoubleValue | 4-8 바이트 |
| **복합** | StringValue, BytesValue, ContainerValue, ArrayValue | 가변 |

*\* LongValue/ULongValue는 C++ 호환성을 위해 32비트 범위를 적용*

**예제**:
```csharp
// 32비트 정수
container.Add(new IntValue("count", 100));

// 64비트 정수 (큰 값에 사용)
container.Add(new LLongValue("big_number", 5_000_000_000L));

// 부동소수점
container.Add(new DoubleValue("price", 99.99));

// 중첩 컨테이너
var address = new ValueContainer();
address.Add(new StringValue("city", "서울"));
container.Add(new ContainerValue("address", address));
```

📚 **[값 타입 상세 정보 →](docs/FEATURES.md#value-types)**

## 크로스 언어 사용

### JSON v2.0 어댑터

```csharp
using ContainerSystem.Adapters;

// JSON v2.0 포맷으로 직렬화 (C++ 호환)
string jsonV2 = JsonV2Adapter.ToJson(container);

// JSON v2.0에서 역직렬화
var restored = JsonV2Adapter.FromJson(jsonV2);
```

이 JSON은 다음에서 읽을 수 있습니다:
- C++ `container_system`
- Python `python_container_system`
- Go, Rust, Node.js (JSON 파싱 통해)

🔗 **[호환성 가이드 →](docs/advanced/COMPATIBILITY.md)**

## 빌드

### 빌드 스크립트 사용

```bash
# Linux/macOS
./scripts/build.sh              # Release 빌드
./scripts/build.sh debug        # Debug 빌드
./scripts/build.sh --test       # 빌드 및 테스트
./scripts/build.sh --pack       # 빌드 및 NuGet 패키지 생성

# Windows (PowerShell)
.\scripts\build.ps1 -Test -Pack
```

### 수동 빌드

```bash
dotnet restore
dotnet build --configuration Release
dotnet test
```

🔧 **[빌드 가이드 →](docs/guides/BUILD_GUIDE.md)**

## 스레드 안전성

```csharp
var container = new ValueContainer();

// 스레드 안전한 동시 쓰기
Parallel.For(0, 1000, i =>
{
    container.Add(new IntValue($"value_{i}", i));
});

// 스레드 안전한 읽기
var values = container.Values();
Console.WriteLine($"전체: {container.Count}");
```

## C++ 버전과의 비교

| 기능 | C++ | .NET |
|------|-----|------|
| 값 타입 | 15 | 16 (+ArrayValue) |
| Binary 직렬화 | ✅ 1.8M/초 | ✅ 500K/초 |
| JSON 직렬화 | ✅ 950K/초 | ✅ 200K/초 |
| SIMD 최적화 | ✅ | ❌ |
| 스레드 안전성 | ✅ | ✅ |
| 크로스 언어 | ✅ | ✅ |
| 메모리 풀링 | ✅ | ❌ |

## 기여

기여를 환영합니다! 다음 가이드라인을 따라주세요:

1. 저장소 Fork
2. 기능 브랜치 생성 (`git checkout -b feature/amazing-feature`)
3. 변경 사항 커밋 (`git commit -m 'Add amazing feature'`)
4. 브랜치에 Push (`git push origin feature/amazing-feature`)
5. Pull Request 열기

### 코드 스타일
- .NET 코딩 컨벤션 준수
- nullable 참조 타입 사용
- 포괄적인 테스트 작성
- 공개 API 문서화

## 지원

- 💬 [GitHub Discussions](https://github.com/kcenon/dotnet_container_system/discussions)
- 🐛 [이슈 트래커](https://github.com/kcenon/dotnet_container_system/issues)
- 📧 이메일: kcenon@naver.com

## 라이선스

이 프로젝트는 BSD 3-Clause 라이선스에 따라 라이선스가 부여됩니다 - 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

## 관련 프로젝트

- [container_system](https://github.com/kcenon/container_system) (C++) - 원본 구현
- [python_container_system](https://github.com/kcenon/python_container_system) - Python 동등 버전
- [messaging_system](https://github.com/kcenon/messaging_system) - 고수준 메시징

---

<p align="center">
  Made with ❤️ by 🍀☀🌕🌥 🌊
</p>
