# Pneuma-Client

## 네이밍 컨벤션

### 커밋 컨벤션

[Conventional Commits](https://www.conventionalcommits.org/) 규칙을 따릅니다.

```
<type>: <subject>
```

- **type**: 영어로 작성
- **subject**: 한글로 작성 (50자 이내)

| Type | 설명 |
|------|------|
| `feat` | 새로운 기능 추가 |
| `fix` | 버그 수정 |
| `docs` | 문서 변경 |
| `style` | 코드 포맷, 공백 정리 (기능 변화 없음) |
| `refactor` | 리팩토링 (기능 추가 및 버그 수정 없음) |
| `test` | 테스트 추가 또는 수정 |
| `chore` | 빌드 설정, 패키지 관리 등 기타 변경 |
| `perf` | 성능 개선 |

**예시**
```
feat: 이동 속도 조절 기능 추가
fix: 체력바 표시 오류 수정
docs: README 컨벤션 항목 추가
```

---

### 이슈 / PR 컨벤션

```
[Type] Content
```

| Type | 설명 |
|------|------|
| `Feat` | 새로운 기능 |
| `Fix` | 버그 수정 |
| `Docs` | 문서 작업 |
| `Style` | 코드 스타일 변경 |
| `Refactor` | 리팩토링 |
| `Test` | 테스트 관련 |
| `Chore` | 기타 작업 |

**예시**
```
[Feat] 플레이어 이동 속도 조절 기능 추가
[Fix] 체력바 표시 오류 수정
[Docs] README 컨벤션 항목 추가
```

---

### 머지 규칙

PR을 머지하려면 아래 조건을 **모두** 충족해야 합니다.

1. **Gemini Code Assist** 리뷰를 1회 이상 받을 것
2. **개발자 2명 이상**의 Approve를 받을 것