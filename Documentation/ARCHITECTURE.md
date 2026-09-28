# 숨결섬 런타임 아키텍처

## 의존 방향

`BreathIslandGame → 기능 시스템 → GameContext → 도메인 상태·저장·콘텐츠·월드`

- `BreathIslandGame`: 객체 조립, 상태별 Update 전달, UI/종료 수명주기 전달.
- `Core/GameContext`: 동일한 저장 상태·월드·콘텐츠 참조 공유, 위치 캡처와 복원.
- `Core/PlayerPositionSystem`: 저장 위치 복원과 월드 아래 낙하 복구. 이동 컨트롤러의 수직 속도를 초기화하며 자원과 저장 원본은 변경하지 않는다.
- `Core/GamePhaseController`: 화면 상태, 이동 허용, 카메라 모드 전환. 화면 전환은 저장 상태나 대상 객체를 교체하지 않는다.
- `Core/GameSession`: 전역 입력, 슬롯 저장·불러오기, 언어 변경, 알림. 저장 불러오기는 실내 여부에 따라 Home 또는 Explore로 돌아가 진행 중 포획 보상의 중복 적용을 막는다.
- `Core/ExploreSystem`: 탐험 중 낚시·NPC·시설·도감 진입 명령.
- `Fishing`, `Combat`, `Relationships`, `Facilities`: 기능 규칙과 입력. 전투 숫자키 처리는 Update에서 한 번만 실행하며 UI에서 키보드를 읽지 않는다.
- `GameState`: 기존 JSON 도메인 데이터와 순수 경제·낚시 계산. 확장 상태는 `Core/IslandLifeState`에 분리한다.
- `Core/IslandLifeSession`: 장비·숙련도·도전과제·육지 활동·집 꾸미기 객체 연결, 탐험의 신규 입력 전달과 로드 시 표현 갱신.
- `Equipment`: 장착, 도전과제 조건, 직업 숙련도와 훈련 대상 공격. 기존 포식자 전투는 독립적으로 유지한다.
- `LandActivities`: 거리·도구·소진 조건, 생활 자원 지급, 휴식에 따른 날짜 전진.
- `Housing`: 제작 비용, 가구 재고, 배치 영역·겹침 검사, 확정·취소·회수와 실내 출입.
- `Presentation`: 모델 정규화·URP 재질·캐릭터 애니메이션·손 장비·채집 지점·가구 미리보기·공격 표현. 규칙은 기능 시스템에 남긴다.
- `ModelPresentation`은 FBX 원본 축 보정 회전을 유지한다. `ToolSwing`은 Animator 이후 고정 리그 기준으로 팔 준비·타격·복귀를 표현하며, 보상은 ToolUseSystem만 처리한다.
- `ProceduralWorld`, `PlayerAndCamera`, `ProceduralSeaAudio`: 월드 표현 계층.

## 콘텐츠

`Resources/Content/VerticalSlice.asset`의 `ContentManifest`가 어종 6종, 시설 2종, NPC 2명의 실제 ScriptableObject 에셋을 참조한다. `DefinitionCatalog`는 이를 Resources에서 읽어 읽기 전용 목록·사전으로 제공한다. 어종 순서, 문자열 ID, enum 값, 출현 시간, 보상, 시설 비용은 이전 버티컬 슬라이스와 같다.

각 ScriptableObject는 같은 이름의 C# 파일에 정의한다. 런타임에서 콘텐츠용 `ScriptableObject.CreateInstance`를 호출하지 않는다. 기존 에셋의 ID와 GUID는 저장 호환성과 참조를 위해 유지한다. 콘텐츠 수정은 에셋에서 수행하며 초기 데이터를 재생성하는 런타임/에디터 부트스트랩은 없다.

확장 콘텐츠는 `Resources/Content/IslandLife.asset`와 `Content/Life/` 에셋에 둔다. 일반 무기 3종·특수 무기 3종, 직업 3종, 기본 공격·스킬 각 3종, 도전과제 3종, 가구 8종, 채집 지점 7개를 참조한다. `Editor/IslandLifeAuthoring`은 명시적 초기 작성 명령이며 기존 정의를 덮어쓰지 않는다. 기본 빌드 경로에서는 호출하지 않는다. `IslandLifeLocalization`도 기존 키를 보존하고 없는 키만 추가한다.

## Localization

`Assets/BreathIsland/Localization/GameText_ko.asset`, `GameText_en.asset`는 Unity Localization 1.5.12의 StringTable이다. Shared Data에 공통 키를 보관하며 UI, 알림, 어종 이름·설명, 시설 이름, NPC 이름·역할·대사와 신규 생활 기능 문구가 모두 이 테이블을 사용한다.

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
| EquipmentHud | 인벤토리의 도구·무기·자원·가구 탭, 상세·해금·숙련도, 손에 들기·제작·배치 명령 |
| HousingHud | 배치 격자·회전·확정·취소·회수·휴식·퇴장, 제작·재고는 인벤토리로 안내 |

`ExploreHud`는 근처 대상 안내와 훈련 대상 상태만 표시한다. 인벤토리는 탐험과 실내에서 열 수 있고 닫으면 직전 상태로 돌아간다. `InventorySystem`은 기존 저장 데이터를 읽는 통합 표시 모델과 기능 명령을 제공한다. 실내는 고정 카메라를 사용한다. Home의 Esc는 실내 상태를 보존한 채 Pause를 열고, 실내 퇴장은 HousingSystem 명령으로만 수행한다.

`HudScreen`은 화면 수명과 공통 스타일, `NotebookSurface`는 에셋 프리팹에서 만든 uGUI 노드를 재사용한다. `GameHud.Draw`는 LateUpdate에서 현재 화면과 오버레이만 갱신하고 나머지를 숨긴다. 화면 프리팹과 공통 요소는 `Resources/UI`에 있으며 Editor/NotebookAuthoring은 명시적 작성 명령이다. `NotebookGraphic`, `IslandIcon`, `NotebookButtonMotion`은 벡터 도형·아이콘·버튼 움직임만 담당한다. UI는 공개 기능 명령만 호출하며 게임 규칙이나 저장 수치를 직접 변경하지 않는다.

## 상태와 저장 호환성

- 화면 전환은 플레이어 위치·인벤토리·선택 어종·NPC 참조를 유지한다. 낚시 시작 시 플레이어를 강제 이동/회전시키지 않는다.
- 포식자 패배의 해안 복귀와 미끼 1개 손실은 기존 규칙대로 유지한다.
- 포획 처리와 전투 명령은 현재 상태를 검사하여 반복 UI 이벤트로 보상이 중복 적용되지 않게 한다.
- 야간에 출현 가능한 일반 어종이 없으면 대기 후 준비 상태로 돌아간다. 기존 캐스팅 시간 진행·미끼 소모·출현 시간·보상은 유지한다.
- `CurrentVersion = 3`. `Core/SaveMigration`은 v0/v1의 기존 필드를 보존하고 장비·숙련도·도전과제 해금, 생활 자원·누적 채집·소진 날짜, 가구 재고·배치·실내 여부·야외 복귀 위치를 추가한다. v2의 누락 객체·목록도 복구한다. v3는 벽·바닥 마감 ID를 추가하고 v1/v2는 wood_dark·planks_light로 채운다. 지원하지 않는 미래 버전은 로드/저장 데이터 처리 시 거부한다.
- 자동 저장은 `autosave.json`, 수동 슬롯 1~3은 `save_0.json`~`save_2.json`이다. `.tmp`와 교체 시 `.bak` 정책을 유지한다. 읽기와 마이그레이션은 원본 파일을 수정하지 않는다.
- 새 게임의 시작 z는 4이며 기존 저장 파일의 유효한 z=0은 그대로 복원한다.
- 기존 저장이나 플레이 중 좌표가 월드 아래(y < -5)이면 안전한 해안 `(0, 1.2, 8)`으로 복귀한다. 복구 자체는 인벤토리·파일을 수정하지 않으며, 정상 좌표와 화면 전환 위치는 유지한다.
- 일반 무기는 스킬 참조가 없고 특수 무기는 도전과제 ID를 요구한다. 잠긴 무기는 장착할 수 없다. 스킬 명령은 현재 무기·화면·재사용 시간·거리·대상 생존을 다시 확인한다. 스킬 대기 시간은 장비 교체로 초기화하지 않는다.
- 생활 도구 선택은 직업·숙련도를 바꾸지 않는다. 도구를 들고 있는 동안 무기 공격·스킬 명령을 거부한다. 무기는 인벤토리에서 다시 선택해야 한다. 숙련도는 직업 ID별 유효 명중 수이며 스킬 수치만 변화시킨다.
- 생활 자원은 기존 시설 재료·코인과 독립적이다. 지점 ID별 소진 날짜를 저장해 같은 날 중복 보상을 차단한다. 휴식은 실내에서만 가능하며 날짜를 1 올리고 시간을 09시로 설정한다.
- 실내는 독립된 8×8 공간이다. 배치 미리보기는 저장하지 않고 확정된 인스턴스 ID·가구 ID·격자 좌표·90도 회전만 저장한다. 경계·다른 가구·출입구 겹침을 차단하며 회수는 같은 인스턴스에 한 번만 적용한다.
- 집 표현은 `HomeInteriorView`가 실제 `Resources/World/HomeInterior` 프리팹을 생성한다. 야외 집 외관·문은 ForestVillage 프리팹에 포함한다. Kenney 목재 구조 모델·URP 재질은 Editor/HomeInteriorAuthoring의 명시적 작성 명령으로 구성한다. 마루 윗면은 기존 InteriorOrigin 높이이고 벽은 8×8 가구 영역 밖에 둔다. 출입·저장·가구 규칙은 HousingSystem에 남긴다.
- `GameContext.Loaded` 이벤트로 장비·채집 지점·가구 표현을 새 저장에 맞춰 복원한다. 실내 저장을 불러오면 Home으로 라우팅하며 야외 복귀 위치를 유지한다.
- `BREATH_ISLAND_SAVE_ROOT`는 명시적인 격리 검증용 경로 재정의다. 테스트용 Composition Root도 이 경로를 사용하여 실제 사용자 저장을 읽거나 쓰지 않는다.

## 검증과 확장 규칙

- `Tests/EditMode`: 기존 규칙, 콘텐츠 에셋 및 밸런스, 두 언어 키, 처리별 보상, 시설·관계 경계, 기존 v1 전체 필드, v0 복구, 네 슬롯, 백업 복구 검증.
- `Tests/PlayMode`: 탐험→포획 처리→시설→NPC, 승리·패배, 상태 전환 참조 유지, 언어 전환, 슬롯 불러오기, 줄 끊김, 야간 빈 후보, 낙하 복구, 양 언어 10개 HUD 렌더링 및 실제 Composition Root 초기화 검증. 테스트 저장은 임시 디렉터리를 사용하며 Composition Root 스모크 테스트는 종료 전에 객체를 파괴하여 사용자 저장에 쓰지 않는다.
- 신규 기능 테스트는 무기 해금·숙련도·도구 분리, 채집·재생·제작, 배치 경계·회수, 실내 저장 복원과 야외 위치 유지, 실제 키보드 입력 및 모델·아바타·재질을 검증한다.
- `Build-BreathIsland.ps1`: Localization 등록 → Edit Mode → Play Mode → Windows Mono 빌드. `-ReleaseIl2Cpp`는 별도 출시 도구 체인이 필요하다.
- Play Mode는 실제 Game View 및 RenderTexture로 uGUI·폰트·벡터 메시·화면 비율을 검증한다. 인벤토리의 1280×720, 1920×1080, 2560×1080 한·영 렌더링을 기록한다.
- 실제 검증 상태는 [VALIDATION.md](VALIDATION.md)에 기록한다. 테스트 추가와 소스 컴파일을 Unity 테스트/플레이/빌드 통과로 간주하지 않는다.
- 기능 간 참조는 Composition Root에서 주입한다. UI에 게임 규칙을 넣지 않는다. 저장은 `GameContext.SaveTo`를 사용한다.
- 저장 형식·경제·콘텐츠 범위·플랫폼 동작 등 구현 결과를 바꾸는 요구가 불명확하면 작업 전에 질문한다.
- 단일 Runtime Assembly Definition 내 폴더 경계를 유지한다. 향후 어셈블리 분리는 Domain → Application → Presentation 방향으로 진행한다.

## 필드 입력과 지형

- `ForestVillage`는 에디터 `VillageAuthoring`에서 작성한 고정 마을 프리팹이다. 런타임은 이를 생성하며 기존 절차형 건물·장식 생성 코드는 제거했다. `VillageObstacle` 표시 건물에 저장 위치가 겹칠 때만 `VillageLayout`의 가까운 안전 지점으로 복구한다. 로드 자체는 저장 파일을 수정하지 않으며 실내에서 야외로 돌아올 때도 같은 검사를 적용한다.
- `InteriorFinishSystem`은 실내 가방에서만 임시 마감 선택을 허용한다. 적용만 HousingState와 저장 파일을 갱신하며 취소·가방 닫기·불러오기는 저장된 마감으로 되돌린다. `InteriorFinishView`는 재질과 판재/타일 표현만 갱신한다. 마감 6종은 Resources/Content/Finishes의 실제 정의·재질 에셋이다.
- 가구 4종(소파 3×1/나무 8, 수납장 2×1/나무 6, 선반 2×1/나무 4, 낮은 탁자 2×1/나무 3)을 기존 제작·배치에 추가했다. 제작 목록은 4개씩 페이지를 나눈다. 기존 가구 비용·해금 조건과 8×8 점유 검사는 유지한다.

- 탐험에서는 커서를 고정하고 좌클릭을 손에 든 물건에 전달한다. 메뉴 입력을 먼저 처리하고 이후 ToolUseSystem을 갱신하므로 메뉴 진입 시 대기 중인 타격을 취소한다. 화면 전환 직후 두 프레임은 필드 사용 입력을 차단한다.
- ToolUseSystem은 0.5초 사용 동작의 중간 시점에 2.3m 거리·전방 내적 0.65 이상의 지정 대상을 검사한다. 실제 보상과 소진 검사는 LandActivitySystem에 남긴다. 도구 교체·로드·메뉴 진입은 진행 중 사용을 취소한다. E는 맨손 채집과 기존 대상 상호작용에 사용한다.
- 납작한 섬·잔디·해변 원통의 기본 CapsuleCollider는 보이는 지형과 다른 돔을 만들므로 해당 세 지형만 정적 MeshCollider로 교체했다. 지형 충돌 높이는 유지한다. 마을 확장으로 육지 원통의 X/Z 크기를 늘리고 중심을 안쪽(z=-5)으로 옮겼으며 해변·부두·낚시·채집 좌표는 유지한다.
