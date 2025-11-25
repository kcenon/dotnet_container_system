# .NET Container System 문서

> **언어:** [English](README.md) | **한국어**

**버전:** 1.0.0
**최종 업데이트:** 2025-11-25
**상태:** 종합

dotnet_container_system 문서에 오신 것을 환영합니다! 크로스 언어 호환성을 갖춘 .NET 8용 타입 안전, 고성능 컨테이너 및 직렬화 시스템입니다.

---

## 빠른 탐색

| 하고 싶은 것 | 문서 |
|--------------|------|
| 빠르게 시작하기 | [빠른 시작](guides/QUICK_START.md) |
| 아키텍처 이해하기 | [아키텍처](ARCHITECTURE.md) |
| 소스에서 빌드하기 | [빌드 가이드](guides/BUILD_GUIDE.md) |
| 자주 묻는 질문 찾기 | [FAQ](guides/FAQ.md) |
| 성능 검토하기 | [벤치마크](performance/BENCHMARKS.md) |
| 크로스 언어 호환성 | [호환성](advanced/COMPATIBILITY.md) |

---

## 문서 구조

### 핵심 문서

| 문서 | 설명 |
|------|------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | 시스템 아키텍처, 디자인 패턴, 타입 시스템 |
| [FEATURES.md](FEATURES.md) | 전체 기능 문서 |
| [API_REFERENCE.md](API_REFERENCE.md) | 전체 API 문서 |
| [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) | 코드 구성 |

### 사용자 가이드

| 문서 | 설명 |
|------|------|
| [QUICK_START.md](guides/QUICK_START.md) | 5분 만에 시작하기 |
| [BUILD_GUIDE.md](guides/BUILD_GUIDE.md) | 빌드 지침 및 옵션 |
| [BEST_PRACTICES.md](guides/BEST_PRACTICES.md) | 권장 사용 패턴 |
| [TROUBLESHOOTING.md](guides/TROUBLESHOOTING.md) | 일반적인 문제 및 해결책 |
| [FAQ.md](guides/FAQ.md) | 자주 묻는 질문 |

### 고급 주제

| 문서 | 설명 |
|------|------|
| [COMPATIBILITY.md](advanced/COMPATIBILITY.md) | 크로스 언어 호환성 분석 |
| [COMPATIBILITY_UPDATE.md](advanced/COMPATIBILITY_UPDATE.md) | 구현 상태 업데이트 |

### 성능

| 문서 | 설명 |
|------|------|
| [BENCHMARKS.md](performance/BENCHMARKS.md) | 성능 분석 및 메트릭 |

### 개발

| 문서 | 설명 |
|------|------|
| [CHANGELOG.md](CHANGELOG.md) | 버전 히스토리 및 변경사항 |
| [CONTRIBUTING.md](CONTRIBUTING.md) | 기여 가이드라인 |

---

## 프로젝트 정보

### 현재 상태
- **버전**: 1.0.0
- **대상 프레임워크**: .NET 8.0
- **라이선스**: BSD 3-Clause

### 주요 기능
- **타입 안전한 컨테이너** - 컴파일 타임 검사가 가능한 16개의 고유 값 타입
- **크로스 언어 호환성** - C++, Python, Go, Rust, Node.js와 바이너리 호환
- **스레드 안전 연산** - 동시 접근을 위한 ReaderWriterLockSlim
- **다중 포맷** - Binary, JSON 직렬화
- **고성능** - O(1) 조회, 효율적인 직렬화

### 지원되는 값 타입

| 카테고리 | 타입 |
|----------|------|
| **Null** | NullValue |
| **Boolean** | BoolValue |
| **16비트 정수** | ShortValue, UShortValue |
| **32비트 정수** | IntValue, UIntValue, LongValue*, ULongValue* |
| **64비트 정수** | LLongValue, ULLongValue |
| **부동소수점** | FloatValue, DoubleValue |
| **복합** | StringValue, BytesValue, ContainerValue, ArrayValue |

*\* LongValue/ULongValue는 C++ 호환성을 위해 32비트*

---

## 관련 프로젝트

- [container_system](https://github.com/kcenon/container_system) - 원본 C++ 구현
- [python_container_system](https://github.com/kcenon/python_container_system) - Python 동등 버전
- [messaging_system](https://github.com/kcenon/messaging_system) - 고수준 메시징

---

## 도움 받기

- **이슈**: [GitHub Issues](https://github.com/kcenon/dotnet_container_system/issues)
- **토론**: [GitHub Discussions](https://github.com/kcenon/dotnet_container_system/discussions)
- **이메일**: kcenon@naver.com

---

**최종 업데이트**: 2025-11-25
