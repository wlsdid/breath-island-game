# 숨결섬 런타임 아키텍처

## 의존 방향

`BreathIslandGame → 기능 시스템 → GameContext → 도메인 상태·저장·콘텐츠·월드`

- `BreathIslandGame`: 객체 조립, 상태별 Update 전달, UI/종료 수명주기 전달.
- `Core/GameContext`: 동일한 저장 상태·월드·콘텐츠 참조 공유, 위치 캡처와 복원.
- `Core/PlayerPositionSystem`: 저장 위치 복원과 월드 아래 낙하 복구. 이동 컨트롤러의 수직 속도를 초기화하며 자원과 저장 원본은 변경하지 않는다.
- `Core/GamePhaseController`: 화면 상태, 이동 허용, 카메라 모드 전환. 화면 전환은 저장 상태나 대상 객체를 교체하지 않는다.
- `Core/GameSession`: 전역 입력, 슬롯 저장·불러오기, 언어 변경, 알림. 저장 불러오기는 탐험으로 돌아가 진행 중 포획 보상의 중복 적용을 막는다.
- `Core/ExploreSystem`: 탐험 중 낚시·NPC·시설·도감 진입 명령.
- `Fishing`, `Combat`, `Relationships`, `Facilities`: 기능 규칙과 입력. 전투 숫자키 처리는 Update에서 한 번만 실행하며 UI에서 키보드를 읽지 않는다.
- `GameState`: JSON 도메인 데이터와 순수 경제·낚시 계산. 현재 별도 장비/장착 시스템은 없으며 인벤토리, 어종 선호 미끼 정의를 유지한다.
- `ProceduralWorld`, `PlayerAndCamera`, `ProceduralSeaAudio`: 월드 표현 계층.

## 콘텐츠

`Resources/Content/VerticalSlice.asset`의 `ContentManifest`가 어종 6종, 시설 2종, NPC 2명의 실제 ScriptableObject 에셋을 참조한다. `DefinitionCatalog`는 이를 Resources에서 읽어 읽기 전용 목록·사전으로 제공한다. 어종 순서, 문자열 ID, enum 값, 출현 시간, 보상, 시설 비용은 이전 버티컬 슬라이스와 같다.

각 ScriptableObject는 같은 이름의 C# 파일에 정의한다. 런타임에서 콘텐츠용 `ScriptableObject.CreateInstance`를 호출하지 않는다. 기존 에셋의 ID와 GUID는 저장 호환성과 참조를 위해 유지한다. 콘텐츠 수정은 에셋에서 수행하며 초기 데이터를 재생성하는 런타임/에디터 부트스트랩은 없다.

## Localization

`Assets/BreathIsland/Localization/GameText_ko.asset`, `GameText_en.asset`는 Unity Localization 1.5.12의 StringTable이다. Shared Data에 공통 키를 보관하며 UI, 알림, 어종 이름·설명, 시설 이름, NPC 이름·역할·대사가 모두 이 테이블을 사용한다.

`GameText.Get`은 Localization 초기화 후 명시적인 ko/en Locale로 StringDatabase를 조회한다. 화면별 문자열 사전이나 하드코딩 언어 분기는 없다. 포맷 문자열의 `{0}` 등은 호출부에서 채운다. 현재 동기 초기화는 검증 대상인 Windows Mono용이다.

`Editor/LocalizationSetup`은 첫 에디터 임포트 또는 빌드 스크립트에서 기존 테이블을 StringTableCollection과 Addressables에 등록한다. 텍스트와 콘텐츠를 재생성하지 않는다. Locale, LocalizationSettings, Addressables 설정을 등록하고 테이블 미리 읽기와 플레이어 빌드 시 Addressables 빌드를 켠다. 생성된 프로젝트 설정과 `.meta`도 최종 검증 후 함께 버전 관리한다.

## UI 경계

`GameHud`는 GamePhase에 해당하는 화면과 공통 오버레이를 호출한다. 개별 화면의 배치·버튼·표시 규칙은 포함하지 않는다.

| 화면 | 책임 |
| --- | --- |
| ExploreHud | 이동 도움말, 근처 NPC와 낚시 안내 |
| FishingHud | 캐스팅·대기·장력·재시도 |
| CatchResolutionHud | 포획 결과와 처리 명령 |
| PredatorCombatHud | 체력·공격 예고·전투 명령 |
| DialogueHud | NPC 이름·대사·관계 명령 |
| FacilityHud | 시설 상태·복구 명령 |
| CollectionHud | 발견·전시 상태 |
| PauseHud | 이어하기·수동 슬롯·언어·종료 |

`HudScreen`은 스타일과 미터 등 공통 그리기 도구, `HudCanvas`는 해상도 스케일 및 GUI 행렬 복원, `HudOverlay`는 자원·시간·알림을 담당한다. UI는 경제·낚시·관계 수치를 직접 변경하지 않고 기능 시스템의 공개 명령을 호출한다. 생성한 GUI 텍스처는 화면 수명 종료 때 해제한다.

## 상태와 저장 호환성

- 화면 전환은 플레이어 위치·인벤토리·선택 어종·NPC 참조를 유지한다. 낚시 시작 시 플레이어를 강제 이동/회전시키지 않는다.
- 포식자 패배의 해안 복귀와 미끼 1개 손실은 기존 규칙대로 유지한다.
- 포획 처리와 전투 명령은 현재 상태를 검사하여 반복 UI 이벤트로 보상이 중복 적용되지 않게 한다.
- 야간에 출현 가능한 일반 어종이 없으면 대기 후 준비 상태로 돌아간다. 기존 캐스팅 시간 진행·미끼 소모·출현 시간·보상은 유지한다.
- JSON 스키마와 `CurrentVersion = 1`은 유지한다. `Core/SaveMigration`은 v0/v1의 누락 객체·목록을 복구하며 알려진 값은 보존한다. 지원하지 않는 미래 버전은 로드/저장 데이터 처리 시 거부한다.
- 자동 저장은 `autosave.json`, 수동 슬롯 1~3은 `save_0.json`~`save_2.json`이다. `.tmp`와 교체 시 `.bak` 정책을 유지한다. 읽기와 마이그레이션은 원본 파일을 수정하지 않는다.
- 새 게임의 시작 z는 4이며 기존 저장 파일의 유효한 z=0은 그대로 복원한다.
- 기존 저장이나 플레이 중 좌표가 월드 아래(y < -5)이면 안전한 해안 `(0, 1.2, 8)`으로 복귀한다. 복구 자체는 인벤토리·파일을 수정하지 않으며, 정상 좌표와 화면 전환 위치는 유지한다.

## 검증과 확장 규칙

- `Tests/EditMode`: 기존 규칙, 콘텐츠 에셋 및 밸런스, 두 언어 키, 처리별 보상, 시설·관계 경계, 기존 v1 전체 필드, v0 복구, 네 슬롯, 백업 복구 검증.
- `Tests/PlayMode`: 탐험→포획 처리→시설→NPC, 승리·패배, 상태 전환 참조 유지, 언어 전환, 슬롯 불러오기, 줄 끊김, 야간 빈 후보, 낙하 복구, 양 언어 8개 HUD 렌더링 및 실제 Composition Root 초기화 검증. 테스트 저장은 임시 디렉터리를 사용하며 Composition Root 스모크 테스트는 종료 전에 객체를 파괴하여 사용자 저장에 쓰지 않는다.
- `Build-BreathIsland.ps1`: Localization 등록 → Edit Mode → Play Mode → Windows Mono 빌드. `-ReleaseIl2Cpp`는 별도 출시 도구 체인이 필요하다.
- Play Mode는 IMGUI를 그리는 실제 Game View로 실행한다. `-batchmode`에서는 `OnGUI`가 호출되지 않으므로 HUD 렌더링 검증을 대신할 수 없다.
- 실제 검증 상태는 [VALIDATION.md](VALIDATION.md)에 기록한다. 테스트 추가와 소스 컴파일을 Unity 테스트/플레이/빌드 통과로 간주하지 않는다.
- 기능 간 참조는 Composition Root에서 주입한다. UI에 게임 규칙을 넣지 않는다. 저장은 `GameContext.SaveTo`를 사용한다.
- 저장 형식·경제·콘텐츠 범위·플랫폼 동작 등 구현 결과를 바꾸는 요구가 불명확하면 작업 전에 질문한다.
- 단일 Runtime Assembly Definition 내 폴더 경계를 유지한다. 향후 어셈블리 분리는 Domain → Application → Presentation 방향으로 진행한다.
