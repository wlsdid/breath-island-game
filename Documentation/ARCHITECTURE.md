# 숨결섬 런타임 아키텍처

## 의존 방향

`BreathIslandGame` → 기능 시스템 → `GameContext` → 도메인 모델·저장·월드

- `BreathIslandGame`: 씬의 Composition Root. 객체 생성, 프레임 라우팅, 전역 입력만 담당한다.
- `Core`: 공유 런타임 문맥과 게임 상태 전환을 담당한다.
- `Fishing`, `Combat`, `Relationships`, `Facilities`: 각 기능의 규칙과 입력 처리를 독립적으로 담당한다.
- `Content`: 어종·시설·NPC 정의를 공급한다. 정식 콘텐츠는 ScriptableObject 에셋으로 치환한다.
- `Localization`: 표시 문자열 조회를 담당한다.
- `UI`: 상태를 표시하고 기능 시스템의 공개 명령만 호출한다.
- 기존 `Definitions`, `GameState`: 저장 가능한 도메인 데이터와 순수 계산 규칙을 담당한다.
- 기존 `ProceduralWorld`, `PlayerAndCamera`, `ProceduralSeaAudio`: 월드 표현 계층을 담당한다.

## 모듈 경계 규칙

1. 새 게임 규칙을 `BreathIslandGame`이나 `GameHud`에 직접 작성하지 않는다.
2. 기능 간 호출은 Composition Root가 주입한 참조 또는 콜백으로 연결한다.
3. 저장 데이터는 `GameContext.Save`를 통해 접근하고 저장은 `SaveTo`를 사용한다.
4. UI는 경제·낚시·관계 수치를 직접 변경하지 않고 해당 시스템의 공개 메서드를 호출한다.
5. 계산 가능한 규칙은 MonoBehaviour와 분리해 Edit Mode 테스트가 가능하게 만든다.
6. 저장 형식, 경제 밸런스, 콘텐츠 범위, 플랫폼 동작처럼 결과가 달라지는 요구가 불명확하면 구현 전에 사용자에게 확인한다.

## 폴더 구조

```text
Runtime/
├─ BreathIslandGame.cs       # Composition Root
├─ Core/                     # 공유 문맥, 상태 전환
├─ Content/                  # 콘텐츠 카탈로그
├─ Fishing/                  # 낚시 흐름
├─ Combat/                   # 포식자 전투
├─ Relationships/            # NPC 관계
├─ Facilities/               # 시설 성장
├─ Localization/             # 한·영 문자열
└─ UI/                       # 화면 표시와 UI 명령 연결
```

현재는 모듈 간 순환 참조를 막기 위해 단일 Runtime Assembly Definition 안에서 폴더 경계를 사용한다. 기능 규모가 커지면 Domain → Application → Presentation 순서로 Assembly Definition을 분리한다.
