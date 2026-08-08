# 카드 UI 시스템 구조

> 작성일: 2026-07-14 · 최종 수정: 2026-08-08 · 기준 브랜치: `develop`
> 대상: `Assets/Scripts/UI/CardUI/`, `Assets/Scripts/Card/`, `Assets/Scripts/Battle/`

카드의 데이터 → 표기 → 정렬 → 연출 → 입력을 담당하는 UI 시스템의 **현재 구조**를 정리한 문서입니다.
로드맵 ①스폰 → ②연출 → ③부채꼴 정렬 → ④호버/상호작용까지 신규 구조(`Pneuma.UI.Card`)로 이식을
마치고, ⑤레거시(`CardUITest/`) 정리(#25)까지 끝냈습니다. 남은 것은 드롭 판정·효과 실행 등 후속 기능입니다.

---

## 1. 전체 구성

```
Assets/Scripts/
├── Card/                       # 카드 원본 데이터 (도메인)
│   ├── Data/CardData.cs        # ScriptableObject. 카드 1종의 정적 데이터
│   ├── Enums/CardType.cs
│   └── Enums/CardRarity.cs
├── UI/CardUI/                  # 신규 구조 (namespace Pneuma.UI.Card)
│   ├── CardManager.cs          # 모델: 더미/드로우/셔플 관리 + 이동 이벤트 발행
│   ├── RuntimeCard.cs          # 런타임 카드 1장 (고유 ID + CardData)
│   ├── CardHandView.cs         # 뷰: 손패 이벤트 구독 → 프리팹 스폰/제거 + 정렬 호출
│   ├── CardHandLayout.cs       # 정렬: 손패를 부채꼴로 배치 (제자리만 지정)
│   ├── UICardData.cs           # 카드 오브젝트: 데이터 + 자식 컴포넌트 참조 허브
│   ├── UICardVisual.cs         # 뷰: 데이터를 UI로 표기
│   ├── UICardAnimator.cs       # 연출: 등장/호버/퇴장 트윈 (자식 Visual 대상)
│   ├── UICardDrag.cs           # 입력: 드래그 이동 (루트 위치·회전 소유)
│   ├── UICardInteraction.cs    # 입력: 포인터 호버 + 사용 가능 표시
│   └── CardPileView.cs         # 뷰: 더미 1곳의 장수 표기 (뽑을/버린)
└── (레거시 UI/CardUITest/·CardTest1~4.prefab은 #25에서 제거됨 — 아래 3. 참조)
```

리소스: DOTween(`Assets/Plugins/Demigiant`), 카드 데이터(`Assets/SO/Card/_TestCardData.asset` 1종)
신규 구조용 카드 프리팹: `Assets/Prefabs/UI/Card.prefab` (아래 1.1 참조)

### 1.1 Card.prefab 계층

정렬·드래그와 연출이 서로 값을 덮어쓰지 않도록 **역할별로 계층을 나눕니다.**

```
Card (루트)                     ← 위치·회전은 이 오브젝트가 소유
 ├ UICardData                   (데이터 + 자식 참조 허브)
 ├ UICardDrag                   (드래그 이동: 루트 anchoredPosition/rotation 소유)
 ├ UICardInteraction            (포인터 호버·사용 가능 표시)
 └ CanvasGroup                  (사용 불가 시 반투명)
 └─ Visual (자식)               ← 연출은 이 오브젝트의 로컬 좌표만 건드림
     ├ UICardAnimator           (등장/호버/퇴장 트윈)
     ├ UICardVisual             (표기)
     ├ Border                   (호버 테두리. 평소 비활성)
     ├ Background / Illust
     └ Cost / Title / Description
```

> **핵심 원칙**: 루트의 위치·회전은 `UICardDrag`가 매 프레임 `SmoothDamp`로 소유합니다.
> 그래서 `UICardAnimator`·`UICardInteraction`의 연출은 **자식 `Visual`의 로컬 좌표만** 움직입니다.
> 루트를 트윈하면 드래그와 값을 두고 다퉈 연출이 사라지기 때문입니다.

---

## 2. 신규 구조 (`Pneuma.UI.Card`)

데이터(모델) → (이벤트) → 뷰 단방향으로 흐르고, 정렬·연출·입력을 역할별로 분해했습니다.

### CardData (ScriptableObject)
카드 1종의 정적 데이터. 런타임 상태는 갖지 않습니다.
- 이름/타입/희귀도/캐릭터/코스트
- `SkillGroupID` — 효과 연결용 (효과 시스템 미구현, #26)
- 소멸/예언/업그레이드/컷신 플래그, 일러스트, 설명

### RuntimeCard (런타임 카드)
`CardData`는 카드 **종류**이므로 같은 카드를 2장 보유하면 서로 구분되지 않습니다.
더미 이동·손패 오브젝트 매칭을 **장 단위**로 하기 위해 고유 `Id`를 부여한 래퍼입니다.
- `Id`(자동 증가) + `Data : CardData` + `CardName`

### CardManager (모델) 🟢
카드 더미와 드로우 흐름을 관리합니다. **뷰를 알지 못합니다.** (기획서 3.1.2 / 3.2)
- `startingDeck : List<CardData>` — 에디터에서 등록하는 시작 덱
- `handDrawCount`(기본 5) / `maxHandSize`(기본 10) / `autoStartOnPlay`
- `cardListDictionary : Dictionary<CardListType, List<RuntimeCard>>`
  - `CardListType` = DRAW / HAND / DISCARD / EXHAUST / PERMANENT
- `StartBattle()` — `InitializeDeck()`(셔플) 후 초기 손패 드로우 (3.2.1)
- `StartPlayerTurn()` — 손패 전체 버리고 새로 드로우 (3.2.3) · 턴 흐름(#34)이 호출
- `DrawCard()` — 셔플된 더미 **맨 위**에서 순차 드로우. 손패가 가득(10장) 차면 카드는
  손패 대신 곧바로 버림 더미로 감(증발, 3.2.4). 더미가 비면 버림 더미를 셔플해 다시 채움(3.2.5)
- `Shuffle()` — Fisher-Yates
- ✅ `OnCardMoved(RuntimeCard, from, to)` 이벤트 발행 → **뷰 연동 지점**

### CardHandView (뷰: 스폰) 🟢
`CardManager.OnCardMoved`를 구독해 카드 오브젝트를 생성·제거합니다. **배치는 하지 않습니다.**
- `to == HAND` → 프리팹 `Instantiate` 후 `UICardData.Bind()` → `RefreshLayout()`
- `from == HAND` → `UICardData.PlayDisappear(퇴장 연출 후 Destroy)` → `RefreshLayout()`
- 구독은 `OnEnable`에서 수행 (모델의 `Start()` 드로우보다 먼저 돌아야 함)
- `spawnedCards`(카드↔오브젝트 매칭) + `handCards`(정렬용 순서 리스트)
- 생성·제거 시 `CardHandLayout.UpdateLayout(handCards)` 호출 → 빈자리 자동 보정

### CardHandLayout (정렬) 🟢
손패를 **부채꼴로 배치**합니다. 위치를 직접 쓰지 않고 각 카드의 `UICardDrag`에
제자리(home)만 알려줍니다. 실제 이동·회전은 카드가 스스로 수행 → 드래그와 충돌 없음.
- `UpdateLayout(cards)` — 손패 순서(리스트 인덱스) 기준으로 `GetSlot()` 위치 계산
- `spacing`(가로 간격) / `cardAngle`(장당 각도) / `arcRadius`(세로 처짐) / `offset`
- 각 카드에 `SetHandIndex(i)` + `SetSiblingIndex(i)` 지정 후 `CardDrag.SetHome()` 호출
- `OnDrawGizmos`로 호 궤도·카드 자리를 씬 뷰에 미리 그림 (플레이 중 실시간 반영)

> 레거시 `CardLayout`은 **논리 인덱스**로 배치해 렌더 순서(SetAsLastSibling)를 바꿔도
> 흐트러지지 않았습니다. 신규도 같은 원리 — 정렬은 손패 리스트 순서를 쓰므로 호버가
> 렌더 순서를 올려도 배치는 그대로입니다.

### UICardData (카드 오브젝트) 🟢
카드 GameObject에 붙어 데이터와 자식 컴포넌트를 잇는 허브입니다.
- `CardData` + `RuntimeCard`(스폰된 카드만, 씬 배치 카드는 null) 보유
- 자식/루트 참조 캐싱: `CardAnimator` / `CardDrag`
- `HandIndex` — 손패 내 순서. 호버가 올린 렌더 순서를 되돌릴 때 기준
- `Bind(RuntimeCard)` — 스폰 직후 `CardHandView`가 호출해 데이터 주입 + 등장 연출
- `PlayDisappear(onComplete)` — 드래그를 막고(`Interactable=false`) 퇴장 연출 위임

### UICardVisual (뷰) 🟢
`SetCard(CardData)`로 이름/코스트/설명/일러스트를 UI에 반영. **완성 상태.**

### UICardAnimator (연출) 🟢
카드 한 장의 트윈. **자식 Visual에 붙어 로컬 좌표만 움직입니다.** (DOTween)
- `PlayAppear()` — 아래(`appearOffsetY`)에서 제자리로 + 확대 복귀 (`IsAppearing` 플래그)
- `PlayHover()` — 위로 살짝 + 약 120% 확대 (기획서 3.3.2 [1], 0.1초)
- `PlayUnhover()` — 기본 크기·제자리 복귀
- `PlayDisappear(onComplete)` — 아래로 축소 퇴장 후 콜백 (호출자가 파괴)
- `KillTweens()`로 이전 트윈 정리, `OnDestroy`에서 `DOKill`

### UICardDrag (입력: 이동) 🟡
카드 **루트의 위치·회전을 소유**하는 유일한 컴포넌트. 평소엔 정렬이 준 제자리를,
드래그 중엔 마우스를 `SmoothDamp`로 지연(관성) 추적 + 기울기 연출.
- `SetHome(position, rotation)` — 정렬이 제자리만 지정
- `Interactable` — 사용 불가·퇴장 중 카드를 못 집게 막음(컴포넌트를 끄지 않아 정렬은 계속 따라감)
- `IsDragging` — 호버가 상태 유지 판단에 참조
- ❌ `OnEndDrag` 드롭 판정 미구현 (원위치 복귀만) — 아래 6. 참조

### UICardInteraction (입력: 호버) 🟢
손패 카드의 포인터 입력(호버)과 사용 가능 여부 표시. (기획서 3.3.2 [1])
확대·상승은 `UICardAnimator`에 위임하고, 여기선 **언제** 연출할지와 테두리·반투명만 결정.
카드 사용은 클릭 선택이 아닌 드래그 앤 드롭(3.3.2 [2])이라 **선택 상태를 두지 않습니다.**
- `OnPointerEnter` — 맨 앞으로(`SetAsLastSibling`) + 테두리 표시 + `PlayHover`
  (등장 중 `IsAppearing`이면 튐 방지를 위해 무시)
- `OnPointerExit` — 드래그 중이면 해제를 **드래그 종료까지 미룸**(카드가 커서를 늦게
  따라와 커서가 카드 밖으로 나가도 잡은 카드가 갑자기 작아지지 않게), 아니면 즉시 `Unhover`
- `Unhover` — `HandIndex`로 렌더 순서 복원 + 테두리 숨김 + `PlayUnhover`
- `SetInteractable(bool)` — 코스트 부족 시 반투명 + 회색 테두리 + 드래그 차단
  ❌ **호출부 없음** — 코스트 시스템이 붙어야 실제로 쓰임

### CardPileView (뷰: 더미 표기) 🟢
더미 **한 곳**의 장수를 화면에 표기합니다. (기획서 3.2.6)
`CardHandView`와 마찬가지로 `OnCardMoved`만 구독하며 카드 흐름에는 관여하지 않습니다.
- `pileType : CardListType` — 표기할 더미. 뽑을/버린 더미는 **오브젝트 2개**에 타입만 다르게 지정
- `countText : TMP_Text` — `countFormat`(`{0}`)으로 장수 표기
- `pileBody : GameObject` — 더미가 비면 숨김(카드 뒷면). 지정 안 하면 항상 표시
  - 오브젝트를 `SetActive(false)`하지 않고 **하위 `Graphic`의 `enabled`만** 끕니다.
    `countText`를 카드 뒷면의 자식으로 두는 배치가 흔한데, 통째로 끄면 빈 더미에서
    숫자까지 사라져 아무것도 안 보이기 때문입니다. `countText`와 그 하위는 캐싱에서 제외됩니다.
- 이 더미가 출발지/도착지인 이동에만 반응 → 무관한 이동엔 갱신하지 않음
- 구독은 `OnEnable`(모델의 `Start()` 드로우보다 먼저), 초기 표기는 `Start()`에서 `Refresh()`
- `Count` — 현재 표기 중인 장수 (읽기 전용)

> 배치 팁: 뽑을 더미는 손패 왼쪽, 버린 더미는 오른쪽에 두면 이후 등장/퇴장 연출의
> 시작·끝점을 각 더미 위치로 잡기 좋습니다. (아래 6-2 후속)

---

## 3. 레거시 구조 (`CardUITest/`) — 제거 완료 #25

정식 구조 이전의 프로토타입이었고, 부채꼴 정렬·호버·선택 알고리즘의 원본이었습니다.
아래처럼 신규 구조로 역할별 이식이 끝나 **죽은 코드가 되어 제거했습니다.**

- `CardLayout` — 정렬 + 선택 + 스폰/제거 모놀리식 → `CardHandView`+`CardHandLayout`으로 분해
- `CardUI` — 호버/선택/등장/제거 애니메이션 → `UICardAnimator`+`UICardInteraction`으로 분해
- `UITestCanvas` — 테스트 버튼(`[임시 코드]`)
- `CardTest1~4.prefab` — 레거시 `CardUI` 프리팹 (테스트용 복제본)

> **이식하지 않은 동작**: 레거시엔 **선택(클릭) 상태**와 **선택 카드 양옆 밀어내기(pushAmount)**가
> 있었으나, 사용 방식을 드래그 앤 드롭으로 정하며(기획서 3.3.2 [2]) 선택 상태를 두지 않았습니다.
> 삭제 전 남은 코드에서 이 클래스들을 참조하는 곳이 없음을 확인했습니다(빌드 통과).

---

## 4. 데이터 흐름 (현재)

```
CardManager.StartBattle()
  └ InitializeDeck() ─ 셔플 ─ DrawCards(5) ─ DrawCard() ─ MoveCard(DRAW→HAND)
                                                              └ OnCardMoved 발행
                                                                   │
                                                                   ▼
                                      CardHandView.HandleCardMoved()
                                        ├ Instantiate(Card.prefab) ─ UICardData.Bind()
                                        │    ├ UICardVisual.SetCard()      (표기)
                                        │    └ UICardAnimator.PlayAppear() (등장 연출)
                                        └ CardHandLayout.UpdateLayout()    (부채꼴 배치)
                                             └ 각 카드 UICardDrag.SetHome()

매 프레임: UICardDrag.Update() ─ 제자리(home) 또는 드래그 목표를 SmoothDamp 추적
호버: UICardInteraction ─ PlayHover/Unhover (자식 Visual만)
퇴장: MoveCard(HAND→DISCARD) ─ PlayDisappear ─ 연출 후 Destroy
```

**핵심 공백**: 모델 → 뷰 → 정렬 → 연출 → 호버까지 연결 완료.
남은 병목은 **드롭 판정(카드 사용)**과 **효과 실행**입니다.

---

## 5. 구현 현황 요약

| 영역 | 상태 | 비고 |
|------|------|------|
| 카드 데이터(CardData) | 🟢 완료 | 효과용 `SkillGroupID`만 있고 실행부 없음 |
| 더미/드로우/셔플(CardManager) | 🟢 완료 | 손패 상한·증발·재셔플 반영 (#39) |
| 손패 스폰(CardHandView) | 🟢 완료 | 스폰/제거 시 정렬 호출 |
| 부채꼴 정렬(CardHandLayout) | 🟢 완료 | 레거시에서 이식 · 기즈모 지원 |
| 데이터 표기(UICardVisual) | 🟢 완료 | |
| 등장/호버/퇴장 연출(UICardAnimator) | 🟢 완료 | 자식 Visual 대상 트윈 |
| 호버 상호작용(UICardInteraction) | 🟢 완료 | 테두리·반투명, 드래그 중 호버 유지 |
| 드래그 이동(UICardDrag) | 🟡 일부 | 이동 O, **드롭 판정 X** (아래 6-1) |
| 카드 사용/드롭 판정 | 🔴 미착수 | drop zone·사용/취소 판정 (기획 대기) |
| 코스트 시스템 | 🔴 미착수 | `SetInteractable` 호출부 없음 |
| 카드 더미 UI(뽑을/버린 더미) | 🟡 일부 | 카운트 표기 O(`CardPileView`), **연출 시작·끝점 X** (3.2.6) |
| 카드 효과 실행 | 🔴 미착수 | (#26) |
| 레거시 정리(CardUITest/) | 🟢 완료 | (#25) 이식 완료로 제거 |

---

## 5-1. ⚠️ 카드 순환 모델이 두 벌입니다 (정리 필요)

`develop`에 #40(`feature/card-draw`)과 #41(`feature/game-ui`)이 각각 머지되면서
**덱/손패/버림 순환 로직이 중복 구현**된 상태입니다.

| | `Pneuma.UI.Card.CardManager` | `Pneuma.Card.Management.CardCycleManager` |
|---|---|---|
| 위치 | `Assets/Scripts/UI/CardUI/` | `Assets/Scripts/Card/Management/` |
| 형태 | MonoBehaviour | 순수 C# (`DeckManager`+`HandManager` 조합) |
| 카드 단위 | `RuntimeCard` (고유 `Id`) | `CardInstance` (`CurrentCost`·운명각인 등 전투 상태) |
| 뷰 연동 | ✅ `OnCardMoved` 이벤트 | ❌ 이벤트 없음 (카운트 프로퍼티만) |
| 사용처 | `UITestScene` — `CardHandView`·`CardPileView` | `_TestCardUseController` (테스트 전용) |

둘 다 셔플·재셔플·손패 상한을 각자 구현했습니다. 그리고 **#44 TO-DO에
"전투용 손패 UI에 드로우 결과 반영"**이 있어, 그대로 진행되면 `CardCycleManager` → 손패 UI
배선이 하나 더 생겨 `CardHandView`와 충돌합니다.

> **현재 UI 계층(`CardHandView`/`CardPileView`)은 `CardManager` 쪽에 붙어 있습니다.**
> 어느 쪽을 정본으로 삼을지는 #44 담당자와 합의가 필요합니다. 합쳐야 할 축:
> `CardInstance`의 전투 상태(코스트 변동·운명각인)와 `RuntimeCard`의 고유 `Id`·이동 이벤트는
> 서로 배타적이지 않으므로, **한 클래스로 합치고 이동 이벤트를 남기는 방향**이 자연스럽습니다.

---

## 6. 남은 일 (우선순위 제안)

이식(①~④)과 레거시 정리(⑤)가 끝났으므로, 남은 작업은 **후속 기능**입니다.

1. **드롭 판정 (`UICardDrag.OnEndDrag`)** — 드롭 위치로 카드 사용/취소 판정.
   ⚠️ 기획 대기: 공격 카드 대상 지정 UI, 드롭 존 규칙(기획서 3.3.2 이하 미확정 구간).
2. **카드 더미 UI — 연출 연결** — 장수 카운트는 `CardPileView`로 완료. 남은 것은
   `UICardAnimator`의 등장/퇴장 시작·끝점을 각 더미 위치로 잡는 것(카드가 뽑을 더미에서
   날아오고 버린 더미로 날아가는 연출). 현재는 `appearOffsetY`로 아래에서 올라올 뿐임.
3. **BattleManager 턴 루프 연동 (#34)** — `StartBattle()`/`StartPlayerTurn()` 호출부.
   현재는 `autoStartOnPlay`로 씬 단독 실행만 검증. 실제 전투에선 배틀 흐름이 호출해야 함.
4. **코스트 시스템** — 코스트 부족 카드에 `UICardInteraction.SetInteractable(false)` 호출.
   현재 반투명·회색 테두리·드래그 차단 로직은 준비됐으나 호출자가 없음.
5. **카드 효과 실행 (#26)** — `SkillGroupID` 기반 효과 실행.
6. **CardType enum 정합성 확인** — 현재 `Attack/Defense/Buff/Debuff/Unique/Skill`.
   기획서 표 기준과 일치하는지(예: Power/Skill 구분) 확인 필요.
7. **에디터 검증** — `CardManager` 인스펙터 값(`handDrawCount=5`/`maxHandSize=10`/
   `autoStartOnPlay`)과 `CardHandView`·`CardHandLayout`·`CardPileView` 씬 배선을 플레이로 확인.
8. **카드 순환 모델 통합** — 위 5-1 참조. #44 담당자와 정본 결정 필요.

---

## 7. 관련 이슈

- **#44** [Feature] 전투 게임 사이클과 카드 드로우 연결 — `CardCycleManager`↔`BattleManager` (위 5-1 충돌 주의)
- **#34** [Feature] Battle Flow 턴 루프 — `StartBattle`/`StartPlayerTurn` 호출부
- **#26** [Feature] 인게임 효과 구현 — `SkillGroupID` 기반 효과 실행
- **#1** [Feature] 인게임 전투 UI
- ~~**#39** [Feature] 덱 및 카드 드로우 시스템 구현~~ — CLOSED (#40 머지)
- ~~**#25** [Refactor] 카드 시스템 리팩토링~~ — 레거시 이식·제거 완료
- ~~**#27** [TODO] 카드 드래그/드롭~~ — CLOSED (드래그 이동 완료, 드롭 판정은 6-1로 이어짐)

---

## 8. 향후 방향

모델 → (이벤트) → 뷰 단방향 정리가 끝난 최종 구조:

```
CardManager (모델, 이벤트 발행)              ← 완료
   ├─▶ CardHandView (스폰/제거)              ← 완료
   ├─▶ CardHandLayout (부채꼴 정렬)          ← 완료
   ├─▶ UICardAnimator (등장/호버/퇴장 트윈)  ← 완료
   ├─▶ UICardInteraction (포인터 호버)       ← 완료
   ├─▶ UICardDrag (드래그 이동)              ← 이동 완료, 드롭 판정 남음
   ├─▶ UICardVisual (표기)                   ← 완료
   └─▶ CardPileView (더미 장수 표기)         ← 카운트 완료, 연출 연결 남음
```

권장 순서: ~~① 스폰~~ → ~~② 연출~~ → ~~③ 정렬~~ → ~~④ 호버/상호작용~~ → ~~⑤ 레거시 정리(#25)~~
→ ⑥ 드롭 판정 → ⑦ 카드 효과 실행(#26)
