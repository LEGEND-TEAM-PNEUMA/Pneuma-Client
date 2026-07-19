# 카드 UI 시스템 구조

> 작성일: 2026-07-14 · 최종 수정: 2026-07-18 · 기준 브랜치: `feature/game-ui`
> 대상: `Assets/Scripts/UI/CardUI/`, `Assets/Scripts/Card/`, `Assets/Scripts/Battle/`

카드의 데이터 → 표기 → 연출 → 입력을 담당하는 UI 시스템의 **현재 구조**를 정리한 문서입니다.
신규 구조(`Pneuma.UI.Card`)와 레거시 테스트 코드(`CardUITest/`)가 공존하는 과도기 상태입니다.

---

## 1. 전체 구성

```
Assets/Scripts/
├── Card/                       # 카드 원본 데이터 (도메인)
│   ├── Data/CardData.cs        # ScriptableObject. 카드 1종의 정적 데이터
│   ├── Enums/CardType.cs
│   └── Enums/CardRarity.cs
├── UI/CardUI/                  # 신규 구조 (namespace Pneuma.UI.Card)
│   ├── CardManager.cs          # 모델: 더미/드로우 관리 + 이동 이벤트 발행
│   ├── RuntimeCard.cs          # 런타임 카드 1장 (고유 ID + CardData)
│   ├── CardHandView.cs         # 뷰: 손패 이벤트 구독 → 프리팹 스폰/제거
│   ├── UICardData.cs           # 카드 오브젝트: 데이터 + 자식 컴포넌트 참조
│   ├── UICardVisual.cs         # 뷰: 데이터를 UI로 표기
│   ├── UICardAnimator.cs       # 연출: 애니메이션 (현재 껍데기)
│   └── UICardDrag.cs           # 입력: 드래그 이동
└── UI/CardUITest/              # 레거시 (부채꼴 정렬·호버·선택 프로토타입)
    ├── CardLayout.cs           # 정렬 + 선택 + 스폰/제거 (모놀리식)
    ├── CardUI.cs               # 호버/선택/등장/제거 애니메이션
    └── UITestCanvas.cs         # 테스트용 캔버스 버튼 바인딩
```

리소스: DOTween(`Assets/Plugins/Demigiant`), 카드 데이터(`Assets/SO/Card/_TestCardData.asset` 1종)

⚠️ `Assets/Prefabs/UI/CardTest1~4.prefab`은 **레거시 `CardUI` 프리팹**이며 신규 구조(`UICardData`)가
들어있지 않습니다. 4종은 카드 종류가 아닌 테스트용 복제본입니다.
신규 구조용 카드 프리팹은 **아직 없고**, `UITestScene`에 씬 오브젝트로만 존재합니다.
→ `CardHandView.cardPrefab`에 연결할 프리팹을 씬 오브젝트로부터 생성해야 합니다.

---

## 2. 신규 구조 (`Pneuma.UI.Card`)

데이터(모델)와 화면(뷰)을 분리하려는 방향으로 설계되어 있습니다.

### CardData (ScriptableObject)
카드 1종의 정적 데이터. 런타임 상태는 갖지 않습니다.
- 이름/타입/희귀도/캐릭터/코스트
- `SkillGroupID` — 효과 연결용 (효과 시스템 미구현)
- 소멸/예언/업그레이드/컷신 플래그, 일러스트, 설명

### RuntimeCard (런타임 카드)
`CardData`는 카드 **종류**이므로 같은 카드를 2장 보유하면 서로 구분되지 않습니다.
더미 이동·손패 오브젝트 매칭을 **장 단위**로 하기 위해 고유 `Id`를 부여한 래퍼입니다.
- `Id`(자동 증가) + `Data : CardData`

### CardManager (모델)
카드 더미와 드로우 흐름을 관리합니다. **뷰를 알지 못합니다.**
- `startingDeck : List<CardData>` — 에디터에서 등록하는 시작 덱
- `cardListDictionary : Dictionary<CardListType, List<RuntimeCard>>`
  - `CardListType` = DRAW / HAND / DISCARD / EXHAUST / PERMANENT
- `InitializeDeck()` — `startingDeck`을 `RuntimeCard`로 만들어 DRAW 더미에 채움
- `DrawCards(count)` → 내부 `DrawCard()` → `MoveCard(DRAW → HAND)`
- ✅ `OnCardMoved(RuntimeCard, from, to)` 이벤트 발행 → **뷰 연동 지점**
- 현재는 `Start()`에서 테스트로 `testDrawCount`장 드로우

### CardHandView (뷰: 스폰)
`CardManager.OnCardMoved`를 구독해 카드 오브젝트를 생성·제거합니다.
- `to == HAND` → 프리팹 `Instantiate` 후 `UICardData.Bind()`
- `from == HAND` → 해당 오브젝트 `Destroy`
- 구독은 `OnEnable`에서 수행 (모델의 `Start()` 드로우보다 먼저 돌아야 함)
- `spawnedCards : Dictionary<RuntimeCard, UICardData>`로 카드↔오브젝트 매칭
- ❌ 정렬 없음 → 생성된 카드가 **같은 자리에 겹침** (부채꼴 정렬은 ③에서)
- ❌ 퇴장 애니메이션 없이 즉시 `Destroy` (`TODO(#25)`)

### UICardData (카드 오브젝트)
카드 GameObject에 붙어 데이터와 자식 컴포넌트를 연결합니다.
- `CardData` + `RuntimeCard`(스폰된 카드만, 씬 배치 카드는 null) 보유
- `Awake()`에서 자식의 `UICardVisual`·`UICardAnimator` 캐싱
- `Bind(RuntimeCard)` — 스폰 직후 `CardHandView`가 호출해 데이터 주입
- `Start()` — 씬에 직접 배치된 카드는 인스펙터 데이터로 표기 (하위 호환)

### UICardVisual (뷰)
`SetCard(CardData)`로 이름/코스트/설명/일러스트를 UI에 반영. **완성 상태.**

### UICardAnimator (연출)
등장/사용/드로우 애니메이션 담당 예정. **현재 껍데기** — `StartAnimation()`은 로그만,
`CardAppearAnimation()`/`CardDisappearAnimation()`은 빈 함수.

### UICardDrag (입력)
드래그로 카드를 이동. 마우스를 즉시 따라가지 않고 `SmoothDamp`로 지연(관성) 추적 + 기울기 연출.
- ✅ 드래그 이동 + tilt
- ❌ `OnEndDrag` 드롭 판정 미구현 (원위치 복귀만, `TODO(#27)`)

> ⚠️ ③(정렬) 착수 시 주의: `Update()`가 매 프레임 **무조건** `anchoredPosition`을
> `targetPosition`(스폰 시점 위치)으로 되돌립니다. 정렬이 카드를 옮겨도 다음 프레임에
> 즉시 끌려오므로, 드래그 중일 때만 위치를 쓰도록 먼저 고쳐야 합니다.

---

## 3. 레거시 구조 (`CardUITest/`)

정식 구조 이전의 프로토타입. **부채꼴 정렬·호버·선택 알고리즘의 실동작본**이 여기 있습니다.
(신규 구조로 이식할 원본)

### CardLayout
정렬 + 선택 + 스폰/제거를 모두 쥔 모놀리식 컴포넌트.
- `UpdateLayout()` — 살아있는 카드를 **논리 인덱스** 기준으로 부채꼴 배치
  (spacing / cardRotation / cardYOffset), 선택 카드 양옆을 `pushAmount`만큼 밀어냄
- 삭제 시 `IsRemoving`으로 정렬 대상에서 즉시 제외 → 빈자리 자동 보정
- 선택 시 `SetAsLastSibling`으로 렌더 순서만 바꾸고, 정렬은 논리 인덱스라 흐트러지지 않음
- 스폰/제거/클리어/리스폰 테스트 함수 포함

### CardUI
카드 1장의 상호작용 + 애니메이션(DOTween).
- 등장(아래→제자리), 호버(확대·정면·상승), 선택(더 확대·상승), 제거(아래로 퇴장)
- `baseRotation`에 레이아웃 각도를 저장, 호버/선택 중엔 정면(0도) 유지 후 복귀
- `IPointerClick/Enter/Exit` 입력 처리

### UITestCanvas
테스트 버튼(카드 추가/제거/리스폰) 및 선택 카드 이름 표시. `[임시 코드]` 표기.

---

## 4. 데이터 흐름 (현재)

```
CardManager.Start()
  └─ InitializeDeck() ─ DrawCards(n) ─ MoveCard(DRAW→HAND)
                                          └ OnCardMoved 발행
                                               │
                                               ▼
                          CardHandView.HandleCardMoved()
                            └ Instantiate(cardPrefab) ─ UICardData.Bind()
                                 └ UICardVisual.SetCard()          (표기 O)
                                 └ UICardAnimator.StartAnimation() (로그만)
                            └ [정렬 없음 → 같은 자리에 겹침]

UICardDrag  ─ 드래그 이동 O / 드롭 판정 X
```

**핵심 공백**: 모델 → 뷰 스폰까지는 연결됨. 이제 **정렬**이 다음 병목.
부채꼴 정렬은 여전히 레거시(`CardLayout`)에만 존재.

---

## 5. 구현 현황 요약

| 영역 | 상태 | 비고 |
|------|------|------|
| 카드 데이터(CardData) | 🟢 완료 | 효과용 `SkillGroupID`만 있고 실행부 없음 |
| 더미/드로우(CardManager) | 🟢 완료 | 덱 = `List<CardData>`, `OnCardMoved` 발행 |
| 손패 스폰(CardHandView) | 🟡 미검증 | 코드 O, **씬 배선·플레이 확인 필요** |
| 데이터 표기(UICardVisual) | 🟢 완료 | |
| 등장/사용 연출(UICardAnimator) | 🔴 껍데기 | 레거시 CardUI에서 이식 필요 |
| 부채꼴 정렬 | 🟡 레거시만 | 신규 구조로 미이식 → **다음 병목** |
| 호버/선택 | 🟡 레거시만 | 단일 상태 관리자 부재 |
| 드래그 | 🟡 일부 | 이동 O, 드롭 판정 X (#27) |
| 카드 효과 실행 | 🔴 미착수 | (#26) |

### 남은 에디터 작업 (① 마무리)

코드는 작성됐으나 **컴파일·플레이 검증 전**입니다. 아래는 에디터에서 직접 해야 합니다.

1. 신규 카드 프리팹 생성 — `UITestScene`의 카드 오브젝트를 `Assets/Prefabs/UI/`로 드래그
2. `CardManager` 컴포넌트 활성화 — 현재 체크 해제 상태라 `Start()`가 실행되지 않음
3. `startingDeck`에 `CardData` 등록 — 현재 에셋은 `_TestCardData` 1종뿐 (중복 등록 가능)
4. `CardHandView` 배치 후 `cardManager` / `cardPrefab` / `handRoot` 연결

> 이미 완료된 상태입니다 (`Assets/Prefabs/UI/Card.prefab`, `UITestScene`).
> 카드는 스폰되지만 정렬이 없어 **한 자리에 겹쳐 나오는 것이 현재의 정상 동작**입니다.

---

## 6. 관련 이슈

- **#27** [TODO] 카드 드래그/드롭 — `UICardDrag.OnEndDrag` 드롭 판정
- **#25** [Refactor] 카드 시스템 리팩토링 — 레거시 정렬/연출을 신규 구조로 이식, `CardUITest/` 정리
- **#26** [Feature] 인게임 효과 구현 — `SkillGroupID` 기반 효과 실행
- **#1** [Feature] 인게임 전투 UI

---

## 7. 향후 방향 (제안)

모델 → (이벤트) → 뷰 단방향으로 정리하고, 레거시의 정렬·선택·연출을 역할별로 분해:

```
CardManager (모델, 이벤트 발행)              ← ① 완료
   ├─▶ CardHandView (스폰/제거)              ← ① 완료 (검증 대기)
   ├─▶ CardHandLayout (부채꼴 정렬)          ← 레거시 CardLayout 이식
   ├─▶ CardSelectionController (호버/선택 단일 상태)
   ├─▶ UICardAnimator (실제 트윈)            ← 레거시 CardUI 이식
   ├─▶ UICardInteraction (포인터 입력)
   └─▶ UICardVisual (표기, 완료)
```

권장 순서: ~~① CardManager 손패 이벤트 + 프리팹 스폰~~ → ② UICardAnimator 채우기
→ ③ CardHandLayout 정렬 → ④ SelectionController/Interaction → ⑤ 레거시 정리(#25)

> 스폰 책임은 당초 `CardHandLayout`에 두려 했으나, 스폰(생명주기)과 정렬(배치)을 분리해
> `CardHandView`(스폰) / `CardHandLayout`(정렬)로 나눴습니다. ③에서 `CardHandView`가
> 스폰·제거 시 `CardHandLayout.UpdateLayout()`을 호출하는 형태가 됩니다.
