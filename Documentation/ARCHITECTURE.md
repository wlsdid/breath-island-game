# 숨결섬 런타임 아키텍처

## 입질·챔질 (2026-10-15)

- 낚시 흐름은 `Ready → Waiting → Bite → Hooked`다. 기존 출현 시간·어종 선택을 통과하면 Bite로 들어가고 **표시된 입질부터 3초** 안에 `Hook` 명령을 받아야 줄 감기로 넘어간다. 좌클릭·Space·패드 A의 새 누름 또는 UI 챔질 버튼으로 명령을 전달한다. 던지기 때 누른 키를 계속 유지하는 것만으로 챔질하지 않는다.
- 입질 중에는 진행률·장력이 증가하지 않으며 Reel/Ease는 무시한다. 기한이 끝난 프레임은 실패가 우선이다. 성공은 기존 초기 진행률 8·장력 34로 한 번만 전환하며 같은 업데이트에서 감기까지 실행하지 않는다.
- 시간 초과는 `MissedBite`로 줄 끊김과 구분한다. 추가 미끼·보상·포획 실적 변경 없이 Failed로 이동하며, 재시도 후 다음 캐스팅에서만 미끼를 다시 소비한다. 미끼 비용은 기존 캐스팅당 1개다. 일시정지 중 카운트다운과 챔질 명령은 멈춘다. 일시 상태이므로 저장 버전은 유지한다.
- 표현은 입질 중 찌의 흔들림·잠김·빠른 파문, 성공 시 낚싯대 들어 올리기를 담당한다. HUD는 한·영 테이블의 입질 안내·남은 시간·챔질 버튼·놓침 문구를 표시한다. `FishingBiteLocalization.Build`는 기존 번역을 보존하며 새 키만 작성한다.

## 해안·낚시 표현 (2026-10-15)

- `CoastalScenery`는 모래·풀 가장자리를 생성하고 GroundCover를 연결한다. 풀은 작성된 KayKit 잔디 프리팹을 사용한다. 자체 `CoastalBlend` 셰이더로 가장자리를 흐리게 한다. 기존 평면 얕은 물·포말·반사 띠는 OceanSurface로 대체했다. 지형 충돌·좌표·Unity 전역 난수는 변경하지 않으며 생성한 메시·재질은 종료 시 해제한다.
- `FishingSystem.PresentAction`은 유효한 캐스팅·감기·풀기 뒤 표현 이벤트를 전달한다. 어종 선택·대기 시간·장력·포획·미끼·보상 계산은 기존 시스템에 남는다. 풀기는 다른 화면이나 입질 전에는 무시한다.
- `FishingPresentation`은 단계·장력·진행률을 읽어 `World/FishingRig`를 표시한다. 손잡이 자세, 캐스팅 궤적, 감기·풀기, 찌, 줄 처짐, 어종별 도약과 수면 파문은 표현 전용이다. 표시 시간은 일시정지 동안 멈추며 화면을 나갈 때 즉시 숨긴다. 표현에서 게임 입력·저장·보상을 변경하지 않는다.
- `FishingRodView`는 가늘어지는 원통 메시를 변형하고 줄 가이드·릴 크랭크를 갱신한다. `FishingPose`는 휨·캐스팅·줄·도약의 순수 계산이다. `FishingCatchView`의 기존 어종 ID와 모델 연결은 프리팹에 작성한다.
- `CoastalVisualAuthoring.Build`는 낚싯대·손 표현과 재질을 명시적으로 작성하는 에디터 명령이다. 신규 외부 모델은 반입하지 않았으며 물고기는 기존 Generated 에셋 6종을 사용한다. 런타임 콘텐츠 ScriptableObject 생성이나 저장 버전 변경은 없다.
- 낚시 카메라는 바다를 향하는 별도 구도를 사용하며 탐험 yaw/pitch는 보존한다. 캐릭터는 Walking_A 걷기와 기존 점프 클립을 사용하고 속도 전환 보간과 착지 압축을 표현 모델에만 적용한다. 이동 속도·접지·점프 물리는 유지한다.

## NPC·마을 표현 보완 (2026-10-15)

- `World/Npcs/Nari`, `Maru`는 기존 KayKit Medium 리그와 대기 컨트롤러를 사용하는 실제 프리팹이다. 기존 NPC ID와 상호작용 앵커는 유지한다. 표현의 발 높이는 앵커 아래 0.6m로 맞춘다.
- `NpcPresentation`은 5m 안의 플레이어를 바라보도록 **표현 자식만** 회전한다. 위치·관계·대사·저장을 변경하지 않는다. NPC 이동이나 일정은 추가하지 않았다.
- `VillagePolishAuthoring.Build`는 명시적 에디터 작성 명령이다. 기존 ForestVillage의 `Village Detail Pass`만 교체하며 도로·건물 충돌체·출입 좌표는 유지한다. 창틀·차양·화단·벤치·광장 가장자리·굴뚝은 장식이며 상호작용 대상이 아니다. 빌드 시 자동 재작성하지 않는다.

## 포트폴리오 시연과 표시 보완 (2026-10-12)

- `GatheringHint`는 채집 시스템의 진행률·현재 도구·방향을 읽어 안내만 만든다. 보상·소진 판정은 기존 기능 시스템에 남는다. 채집 횟수/남은 시간, 배치 가능 여부, 건축 미리보기·비용 부족은 한·영 테이블을 사용한다.
- `FirstPersonBodyView`는 낚시·포식자 전투의 1인칭 전환 동안 플레이어 렌더러만 숨겨 머리·망토의 화면 관통을 방지한다. 복귀·컴포넌트 종료 시 기존 렌더 상태를 복원하며 NPC·충돌체·장비 저장 데이터는 변경하지 않는다. GamePhaseController가 전환을 전달한다.
- `NotebookSurface`의 본문은 24, 작은 안내는 22(자동 축소 하한 20)이며 기존 제목·본문 서체를 유지한다. 선택·주요 행동 배경을 어둡게 해 밝은 글자의 대비를 높였다.
- `Editor/PortfolioAuthoring.Prepare`는 명시적으로 실행하는 번역/길 재질/합성 저장 작성 도구다. `Play-Portfolio.ps1`은 매 실행 새 격리 저장을 만들고 Resume.cmd로 같은 세션을 재실행한다. 런타임 분기·저장 버전·일반 새 게임 지급량은 추가하거나 변경하지 않는다.
- 시연 JSON은 Documentation/Portfolio/Seeds에 있으며 자원과 배치 가구가 준비된 데이터임을 밝힌다. PortfolioPresentationTests가 배치 유효성·네 슬롯 복원·안내 상태를 검사하고 Editor 렌더를 남긴다. 해당 이미지와 Windows 직접 조작 증거는 구분한다.

## 격자 건축 (v5)

- `HomeConstructionSystem`은 집 꾸미기에서 원본 구조를 복제하여 미리보기한다. 적용 명령만 가방 나무와 HousingState.structure를 함께 변경하고 저장하며 취소·화면 전환·불러오기는 초안을 버린다.
- `HomeFootprint`는 기본 20×20 또는 저장된 바닥 칸/벽 모서리를 읽는다. 이동 복구·가구 배치·카메라·표현이 동일한 저장 구조를 사용하며 콘텐츠 ScriptableObject를 런타임에 수정하지 않는다.
- 편집 좌표는 0~31이다. 바닥은 나무 2개, 직접 추가한 바닥 철거는 1개 반환한다. 기본 바닥은 철거 보상이 없으며 출입구 예약 영역은 유지한다. 내부 벽은 1개 소비/1개 반환한다. 해금 조건은 없다. 미확정 초안에서 추가 후 취소한 칸은 비용이 없다.
- 외벽은 바닥 경계를 따라 자동 생성된다. 내부 벽은 칸의 북쪽/동쪽 모서리에서 편집한다. 전체 바닥은 출입구에서 연결되어야 하고 플레이어·기존 가구를 침범할 수 없다. 침대를 놓을 마지막 공간을 없애는 편집도 차단하여 휴식·자원 재생을 유지한다. 이 검사는 UI와 독립적이다.
- `HomeConstructionView`는 에디터 작성 BuildFloor/BuildWall 프리팹을 배치하며 렌더링용 벽 높이와 이동 충돌 높이를 분리한다. `HomeConstructionPanel`은 명령과 미리보기만 표시한다. 기존 GameHud 라우팅은 유지한다.
- v1~v4의 구조 누락은 customized=false의 기본 20×20으로 이전한다. 기존 가구 좌표·재고·상자·장비·실외 위치는 유지하며 로드만으로 원본 저장을 덮어쓰지 않는다. 확정된 바닥에는 유료 추가 여부를 기록해 기본 공간 철거로 자원이 생기지 않게 한다.

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
- `LandActivities`: 거리·도구·소진 조건, 생활 자원 지급, 휴식에 따른 날짜 전진. `GatheringWork`는 대상별 유효 타격 횟수와 현재 길게 누르기 시간을 임시로 관리한다. `ResourceNodeDefinition.requiredHits/gatherSeconds`로 완료 조건을 읽고 완료 시에만 기존 보상·채집 횟수·저장을 한 번 적용한다. 길게 누르기는 놓기·메뉴·거리/방향 이탈 시 취소하며, 타격 진행은 로드·휴식 때 초기화한다. 진행 중 수치는 저장하지 않는다. 실제 콘텐츠는 나무·바위 3회, 맨손 채집 3초이며 보상량은 유지한다.
- `Housing`: 제작 비용, 가구 재고, 배치 영역·겹침 검사, 확정·취소·회수와 실내 출입.
- `Presentation`: 모델 정규화·URP 재질·캐릭터 애니메이션·손 장비·채집 지점·가구 미리보기·공격 표현. 규칙은 기능 시스템에 남긴다.
- `ModelPresentation`은 FBX 원본 축 보정 회전을 유지한다. `ToolSwing`은 Animator 이후 고정 리그 기준으로 팔 준비·타격·복귀를 표현하며, 보상은 ToolUseSystem만 처리한다.
- `ProceduralWorld`, `PlayerAndCamera`, `ProceduralSeaAudio`: 월드 표현 계층.

## 콘텐츠

`Resources/Content/VerticalSlice.asset`의 `ContentManifest`가 어종 6종, 시설 2종, NPC 2명의 실제 ScriptableObject 에셋을 참조한다. `DefinitionCatalog`는 이를 Resources에서 읽어 읽기 전용 목록·사전으로 제공한다. 어종 순서, 문자열 ID, enum 값, 출현 시간, 보상, 시설 비용은 이전 버티컬 슬라이스와 같다.

각 ScriptableObject는 같은 이름의 C# 파일에 정의한다. 런타임에서 콘텐츠용 `ScriptableObject.CreateInstance`를 호출하지 않는다. 기존 에셋의 ID와 GUID는 저장 호환성과 참조를 위해 유지한다. 콘텐츠 수정은 에셋에서 수행하며 초기 데이터를 재생성하는 런타임/에디터 부트스트랩은 없다.

확장 콘텐츠는 `Resources/Content/IslandLife.asset`와 `Content/Life/` 에셋에 둔다. 일반 무기 3종·특수 무기 3종, 직업 3종, 기본 공격·스킬 각 3종, 도전과제 3종, 가구 9종, 채집 지점 7개를 참조한다. `Editor/IslandLifeAuthoring`은 명시적 초기 작성 명령이며 기존 정의를 덮어쓰지 않는다. 기본 빌드 경로에서는 호출하지 않는다. `IslandLifeLocalization`도 기존 키를 보존하고 없는 키만 추가한다.

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
| HousingHud | 실내 이동 중 대상 안내·가방·꾸미기 진입 |
| HomeDecorateHud | 기존 격자 배치·회전·확정·취소·회수 |
| HomeRestHud | 침대 휴식 확인·취소 |
| HomeStorageHud | 가방·상자 16칸 표시와 수량 이전 명령 |

`ExploreHud`는 근처 대상 안내와 훈련 대상 상태만 표시한다. 인벤토리는 탐험과 실내에서 열 수 있고 닫으면 직전 상태로 돌아간다. `InventorySystem`은 기존 저장 데이터를 읽는 통합 표시 모델과 기능 명령을 제공한다. 실내는 고정 카메라를 사용한다. Home의 Esc는 실내 상태를 보존한 채 Pause를 열고, 실내 퇴장은 HousingSystem 명령으로만 수행한다.

`HudScreen`은 화면 수명과 공통 스타일, `NotebookSurface`는 에셋 프리팹에서 만든 uGUI 노드를 재사용한다. `GameHud.Draw`는 LateUpdate에서 현재 화면과 오버레이만 갱신하고 나머지를 숨긴다. 화면 프리팹과 공통 요소는 `Resources/UI`에 있으며 Editor/NotebookAuthoring은 명시적 작성 명령이다. `NotebookGraphic`, `IslandIcon`, `NotebookButtonMotion`은 벡터 도형·아이콘·버튼 움직임만 담당한다. UI는 공개 기능 명령만 호출하며 게임 규칙이나 저장 수치를 직접 변경하지 않는다.

## 상태와 저장 호환성

- 화면 전환은 플레이어 위치·인벤토리·선택 어종·NPC 참조를 유지한다. 낚시 시작 시 플레이어를 강제 이동/회전시키지 않는다.
- 포식자 패배의 해안 복귀와 미끼 1개 손실은 기존 규칙대로 유지한다.
- 포획 처리와 전투 명령은 현재 상태를 검사하여 반복 UI 이벤트로 보상이 중복 적용되지 않게 한다.
- 야간에 출현 가능한 일반 어종이 없으면 대기 후 준비 상태로 돌아간다. 기존 캐스팅 시간 진행·미끼 소모·출현 시간·보상은 유지한다.
- `CurrentVersion = 5`. `Core/SaveMigration`은 v0/v1의 기존 필드를 보존하고 장비·숙련도·도전과제 해금, 생활 자원·누적 채집·소진 날짜, 가구 재고·배치·실내 여부·야외 복귀 위치를 추가한다. v2의 누락 객체·목록도 복구한다. v3는 벽·바닥 마감 ID를 추가하고 v1/v2는 wood_dark·planks_light로 채운다. 지원하지 않는 미래 버전은 로드/저장 데이터 처리 시 거부한다.
- 자동 저장은 `autosave.json`, 수동 슬롯 1~3은 `save_0.json`~`save_2.json`이다. `.tmp`와 교체 시 `.bak` 정책을 유지한다. 읽기와 마이그레이션은 원본 파일을 수정하지 않는다.
- 새 게임의 시작 z는 4이며 기존 저장 파일의 유효한 z=0은 그대로 복원한다.
- 기존 저장이나 플레이 중 좌표가 월드 아래(y < -5)이면 안전한 해안 `(0, 1.2, 8)`으로 복귀한다. 복구 자체는 인벤토리·파일을 수정하지 않으며, 정상 좌표와 화면 전환 위치는 유지한다.
- 일반 무기는 스킬 참조가 없고 특수 무기는 도전과제 ID를 요구한다. 잠긴 무기는 장착할 수 없다. 스킬 명령은 현재 무기·화면·재사용 시간·거리·대상 생존을 다시 확인한다. 스킬 대기 시간은 장비 교체로 초기화하지 않는다.
- 생활 도구 선택은 직업·숙련도를 바꾸지 않는다. 도구를 들고 있는 동안 무기 공격·스킬 명령을 거부한다. 무기는 인벤토리에서 다시 선택해야 한다. 숙련도는 직업 ID별 유효 명중 수이며 스킬 수치만 변화시킨다.
- 생활 자원은 기존 시설 재료·코인과 독립적이다. 지점 ID별 소진 날짜를 저장해 같은 날 중복 보상을 차단한다. 휴식은 실내에서만 가능하며 날짜를 1 올리고 시간을 09시로 설정한다.
- 실내는 RoomLayoutDefinition의 기본 20×20 공간이다. 배치 미리보기는 저장하지 않고 확정된 인스턴스 ID·가구 ID·격자 좌표·90도 회전만 저장한다. 경계·다른 가구·출입구 겹침을 차단하며 회수는 같은 인스턴스에 한 번만 적용한다.
- 집 표현은 `HomeInteriorView`가 실제 `Resources/World/HomeInterior` 프리팹을 생성한다. 야외 집 외관·문은 ForestVillage 프리팹에 포함한다. Kenney 목재 구조 모델·URP 재질은 Editor/HomeInteriorAuthoring의 명시적 작성 명령으로 구성한다. 마루 윗면은 기존 InteriorOrigin 높이이고 벽은 정의된 가구 영역 밖에 둔다. 출입·저장·가구 규칙은 HousingSystem에 남긴다.
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
- 가구 4종(소파 3×1/나무 8, 수납장 2×1/나무 6, 선반 2×1/나무 4, 낮은 탁자 2×1/나무 3)을 기존 제작·배치에 추가했다. 제작 목록은 4개씩 페이지를 나눈다. 기존 가구 비용·해금 조건을 유지하고 점유 범위는 RoomLayoutDefinition에서 읽는다.

- 탐험에서는 커서를 고정하고 좌클릭을 손에 든 물건에 전달한다. 메뉴 입력을 먼저 처리하고 이후 ToolUseSystem을 갱신하므로 메뉴 진입 시 대기 중인 타격을 취소한다. 화면 전환 직후 두 프레임은 필드 사용 입력을 차단한다.
- ToolUseSystem은 0.5초 사용 동작의 중간 시점에 2.3m 거리·전방 내적 0.65 이상의 지정 대상을 검사한다. 실제 보상과 소진 검사는 LandActivitySystem에 남긴다. 도구 교체·로드·메뉴 진입은 진행 중 사용을 취소한다. E는 맨손 채집과 기존 대상 상호작용에 사용한다.
- 납작한 섬·잔디·해변 원통의 기본 CapsuleCollider는 보이는 지형과 다른 돔을 만들므로 해당 세 지형만 정적 MeshCollider로 교체했다. 지형 충돌 높이는 유지한다. 마을 확장으로 육지 원통의 X/Z 크기를 늘리고 중심을 안쪽(z=-5)으로 옮겼으며 해변·부두·낚시·채집 좌표는 유지한다.

## 실내 생활·보관 (v4)

- Home는 고정 각도의 추적 카메라와 자유 이동, HomeDecorate·HomeStorage·HomeRest는 이동이 차단된 별도 화면이다. 닫기/Esc로 보조 화면을 닫으면 Home로 복귀하고 두 프레임 동안 상호작용 입력을 차단한다. GameHud는 13개 화면 라우팅만 담당한다.
- `HomeInteractionSystem`은 전방 2.3m의 첫 충돌체만 조사해 벽·다른 가구 뒤의 사용을 막는다. 문, 의자·소파, 침대, 상자를 처리하며 UI가 위치·날짜를 직접 수정하지 않는다. 침대 확인은 일회성이고 날짜 진행은 기존 LandActivitySystem.Rest로 전달한다.
- `HomeNavigation`은 플레이어 점유와 기존 배치 영역을 검사한다. 신규 배치는 플레이어와 겹칠 수 없으며, 막힌 실내 저장 좌표·일어서는 위치는 가까운 빈 위치 또는 출입 공간으로 복구한다. 복구만으로 저장 파일을 덮어쓰지 않는다.
- FurnitureDefinition에 상호작용 종류·좌석 오프셋·방향을 작성한다. HousingView는 배치 인스턴스 ID를 가진 HomeFurnitureTarget과 실제 격자 크기의 충돌체를 생성한다. 모델의 크기 조절을 충돌체 계층과 분리했다. HomeInterior 프리팹에는 이동 경계 충돌체가 있다.
- HomeSeatedView는 기존 Generic 캐릭터의 다리 자세와 시각 모델 위치를 짧게 보간하고 손 장비 표현만 숨긴다. 장비·직업·저장 좌표를 바꾸지 않는다. 앉기 상태와 메뉴는 로드 시 해제한다.
- `HomeStorageSystem`은 상자 인스턴스마다 16개의 StorageSlot을 관리한다. 자원 탭의 물건은 종류별 99개, 가구는 1개씩 저장한다. 기존 스택을 먼저 채우고 빈 칸을 사용하며 성공한 명령만 저장한다. 인벤토리는 저장 데이터의 표시이며 UI가 별도 수량을 보관하지 않는다.
- v4는 HousingState.chests를 추가한다. v1~v3의 기존 데이터는 유지하고 보관 목록을 초기화한다. 상자에서 꺼내기는 LifeResources.Add를 호출하지 않으므로 누적 획득·채집 횟수·도전과제가 변하지 않는다. 내용물이 있는 상자는 회수를 거부한다.
- `Editor/HomeInteractionAuthoring.Build`는 상자 정의·실내 경계·화면 프리팹·번역의 명시적 작성 명령이다. 빌드 시 자동 콘텐츠 재생성이나 런타임 ScriptableObject 생성은 없다.
- StarterBedSystem은 새 게임과 기존 저장 복원 시 가방·배치·상자 전체의 침대 소유 여부를 확인한다. 침대가 없고 starterBedGranted가 false일 때 재고 1개를 지급하고 일회성 기록을 남긴다. 소유 중이면 기록만 남긴다. 제작 실적·재료·해금에는 영향을 주지 않고 로드 자체는 원본 파일을 수정하지 않는다. 다음 정상 저장에서 기록된다.

## 2026-10-04 디자인·조작 개편

- ThirdPersonMotor는 2.86m/s 이동과 접지 시 점프를 처리한다. 화면 전환/텔레포트 직후 입력을 차단하고 천장 충돌 시 상승 속도를 제거한다. 탐험의 F 낚시 시작과 Space 점프를 분리한다.
- ModelPresentation은 스킨 메시의 실제 형상을 측정해 원점과 크기를 맞춘다. 애니메이션 전체 범위를 감싼 culling bounds는 발 위치에 사용하지 않는다. CharacterPresentation은 수평 이동과 접지 상태를 Animator에 전달한다. 점프 클립은 제공된 KayKit Generic 리그에서 선별했다.
- 실제 RoomLayoutDefinition(HomeDefault) 에셋은 폭·깊이·도착점·출입 예약 영역을 정의한다. 기본 20×20 확대에서도 기존 좌표를 이동시키지 않는다. HomeInteriorAuthoring과 가구/이동 검사·카메라가 같은 정의를 사용한다. InteriorCameraFraming은 평상시 근접 추적, 꾸미기/가방은 전체 구도를 계산한다.
- 전체 가방의 InventoryLayoutSystem은 inventory.slotOrder의 ID/빈칸 배치만 저장한다. 수량은 기존 상태에서 읽는 InventorySystem의 표시 모델에만 있으며 중복 저장하지 않는다. 24칸은 페이지 크기이며 종류가 늘어나면 페이지가 확장된다. 기존 v4의 선택 필드이고 누락 데이터는 빈 목록으로 복구한다.
- InventorySlotDrag는 uGUI 포인터·임시 아이콘만 담당하고 Move/Sort 명령은 InventoryLayoutSystem이 검증·저장한다. 닫기·탭 변경은 드래그 상태를 버린다. 종류별 탭은 기존 제작·장착·해금 설명을 유지한다.
- ItemIcons의 파생 PNG 21종은 제공된 모델/재질을 에디터 InventoryIconAuthoring으로 렌더링한다. 통화 등 공통 표지는 원래 자체 선화를 사용한다. 런타임에서 새 콘텐츠 정의를 만들지 않는다.
- 채집은 나무·바위 유효 타격 3회, 맨손 3초 유지로 확정되었다. 플레이어 집은 격자 건축으로 확장·변형하고 마을 집은 고정된 서로 다른 외형이다.

## 수면과 수중 표현 (2026-10-15)

- OceanSurface는 해안 부근이 촘촘한 수면 메시와 충돌체 없는 해저를 구성한다. CPU Height/Normal과 OceanWater 셰이더의 파도 수식은 일치하며 찌·물고기·파문이 같은 높이를 참조한다.
- OceanWater는 URP 깊이/불투명 색상 텍스처를 사용해 깊이별 흡수·굴절·하늘 반사·접촉 포말을 합성한다. 카메라의 UniversalAdditionalCameraData에서 두 텍스처를 요청한다. 원거리 잔물결 법선은 줄여 수평선의 깜박임을 완화한다.
- OceanAuthoring.Build가 실제 Ocean/Seabed 재질을 작성하며 런타임은 Resources에서 읽는다. 화면 수면은 표현용이며 이동 충돌이나 저장 형식을 변경하지 않는다.
- AmbientFishMotion은 기존 어종 모델을 수면 아래에서 이동·회전시킨다. FishingPresentation의 찌는 기본적으로 수면에 뜨고 입질 동안 잠긴다. 입질 물고기 접근·걸린 물고기의 수중 이동과 도약은 기존 게임 단계의 표현이다.
- 수면 파도는 게임 시간에 따라 계속 움직인다. 메뉴에서 낚시 타이머와 동작 진행은 멈추며 복귀 시 현재 수면 높이에 다시 맞춘다. 새로운 입력·보상·어종 난수는 추가하지 않는다.

## 낚시 접촉 표현 (2026-10-15)

- FishVisualAuthoring은 기존 Generated FBX 원본에서 World/Fish의 어종별 파생 프리팹과 URP 재질을 작성한다. 원본의 -X 머리 방향을 +Z 진행 방향으로 맞추며 배·눈 재질을 구분한다. 원본 FBX·게임 어종 정의는 변경하지 않는다.
- FishSwimView는 꼬리·등지느러미의 기준 자세를 보관하고 시각 자식만 움직인다. AmbientFishMotion과 FishingPresentation은 루트 위치·진행 방향을 담당한다.
- FishingWaterEffects는 4개 묶음의 파문·물방울을 재사용한다. 캐스팅 착수, 입질 진입, 물고기의 수면 위/아래 전환 때만 효과를 발생시킨다. 수면 높이는 OceanSurface를 읽고 난수·물리 충돌·보상·입력을 변경하지 않는다. 비활성 낚시 화면에서는 샘플링과 동작 시간이 멈춘다.
- 낚싯대의 좌우 반응은 물고기 이동 위상과 현재 장력에 연결한다. 줄 끝과 가이드 연결을 유지하며 기존 감기·풀기 동작과 합성한다. 게임 장력 계산과 3초 챔질 규칙은 유지한다.

낚시 손 표현 보완: FishingHandAuthoring이 손바닥·엄지·접힌 손가락·소매를 작성한다. FishingRodView.reelingHand는 크랭크 위치를 따라가면서 회전은 낚싯대 기준을 유지해 손목의 연속 회전을 막는다. FishingPresentation의 카메라 상대 위치를 높여 화면 하단의 조작 패널과 구별되게 한다. 플레이어 본체 숨김 규칙은 유지한다.

## 2026-10-15 이동·캐릭터·마을 표시 수정

- 걷기 상태의 자동 첫 Walk 클립 선택을 제거하고 Walking_A를 명시했다(기존 Walking_C). 플레이어 시각 모델은 1.45m, 나리 1.42m, 마루 1.55m로 조정했으며 이동 속도·물리 충돌체·상호작용 앵커는 유지한다.
- 낚시에서는 ThirdPersonMotor가 DockMovement의 부두 안쪽 범위에서 WASD 이동을 처리한다. Space는 점프하지 않고 기존 낚시 명령으로 남으며 줄 풀기는 Q로 옮겼다. 마우스 이동/패드 오른쪽 스틱으로 별도 낚시 yaw/pitch를 조절하고 탐험 각도는 보존한다. 일시정지 중 이동·시점은 멈춘다.
- FishingPresentation은 던지는 순간의 전방과 목적지를 보관한다. 시점·플레이어 이동에 대기 중 찌가 끌려오지 않으며 실제 화면상의 투척은 준비·빠른 전방 스윙·복귀 순서다. 게임의 미끼/챔질/장력/보상 규칙은 유지한다.
- FishingHandAuthoring은 기존 Ranger의 팔 메시에서 손·아래팔 삼각형을 추출해 UV와 재질을 유지한 정적 메시 에셋을 작성한다. 원본은 수정하지 않는다. 이전 구형 손 조합은 대체한다.
- WeaponActionView는 유효 무기 공격 이벤트에 검 휘두르기·활 당기기·지팡이 들기 표현을 연결한다. 생활 도구와 분리하며 UI가 직업이나 보상을 바꾸지 않는다. 무기 직업은 기존 EquipmentSystem.Job에서 읽는다.
- GroundCover는 작성된 잔디 프리팹과 Ground 재질을 읽는다. 겹친 양면 삼각형 풀을 KayKit Grass 모델로 교체하고 지면은 월드 좌표의 색/입자 변화·조명을 합성한다. 지형 높이·충돌은 유지한다.
- HouseShellAuthoring은 기존 건물 충돌 범위 안쪽에 벽 뒷면을 보강하고, 지붕은 경사면·박공·하부가 연결된 닫힌 메시로 교체한다. 기존 지붕 모델은 프리팹 안에서 비활성 보존한다. 기존 출입 위치·건물 점유 범위·저장은 유지한다. 탐험 대상 안내는 하단 진행 표시보다 위쪽으로 분리했다.

## 2026-10-15 연속 이동·장비 표현 후속

- 걷기·대기·공중 대기는 원본 FBX를 보존한 파생 AnimationClip의 반복 설정을 명시한다. CharacterMotionAuthoring이 WalkingLoop·IdleLoop·AirLoop 에셋을 작성한다. 단발 점프 시작/착지는 반복하지 않는다.
- 플레이어 시각 높이는 1.30m이며 접촉 여백에 맞춰 표시 원점만 7cm 내렸다. 이동 속도·충돌체·저장 좌표는 유지한다. 장비는 손 본의 축소 스케일을 상쇄하여 무기 1.05m, 도구 0.58m의 최대 길이를 유지한다.
- ToolSwing의 운반 팔은 몸 바깥쪽으로 두고, WeaponActionView는 운반→준비→공격→복귀 자세를 계속 적용한다. 공격의 거리·피해·재사용 시간은 바꾸지 않는다. CombatFeedback의 임시 직선 빔은 제거했다.
- Play-Portfolio.ps1의 -UnlockedWeapons는 새로 복사한 격리 시연 저장 네 개에만 도전과제 완료 상태를 준비한다. 일반 새 게임·원본 시연 seed·실제 사용자 저장은 수정하지 않는다.
- ModelPresentation의 손 장비 크기 검사는 높이 대신 최장 축을 사용한다. 얇고 가로로 작성된 활도 보정 대상이며, EquipmentView에서 활의 세로 방향을 맞춘다.

## 2026-10-15 무기 식별·활 발사·낚시 시점

- WeaponIdentityAuthoring은 제공된 KayKit Fantasy Weapons Bits Unity FBX에서 일반 활과 특수 검/활/지팡이, 화살 프리팹을 작성한다. 특수 무기는 별도 형상과 스킬 색 재질을 사용한다. 모델의 최장 축을 Y축으로 정렬하고 기존 ID·스킬 참조·밸런스는 유지한다.
- EquipmentView는 검의 날 방향을 90도 보정하고 생활 도구 최대 길이는 0.58m로 제한한다. 무기의 1.05m 길이는 유지한다.
- TrainingCombatSystem.Presented는 성공한 공격의 무기·스킬 여부·대상을 표시 계층에 전달한다. 기존 Hit와 피해/숙련도/저장 시점은 유지한다. WeaponActionView는 활의 조준·당김·발사·복귀를, BowShotView는 움직이는 줄과 화살 표현을 담당한다. 화살은 표시용이며 추가 명중/피해를 적용하지 않는다.
- WeaponSkillView는 특수 검의 호형 타격, 활의 착탄 파동, 지팡이의 마법진을 제한된 수명의 표시로 그린다. 효과는 짧은 수명 후 숨기고 소켓 파괴 시 표시와 재질을 정리한다. 활 교체 시 진행 중 화살도 제거한다. 일반 무기에는 스킬 효과가 없다.
- 낚시 중 커서를 고정해 마우스 이동으로 시점을 조절한다. Alt 유지 중에는 커서를 풀고 시점을 멈춰 HUD 버튼을 조작한다. 메뉴 전환은 기존 GamePhaseController 정책을 따른다. 한·영 안내도 갱신한다.

- 도구 운반 시 위팔·아래팔을 몸 바깥쪽으로 두고 도구를 바깥쪽으로 25도 기울인다. 사용 중에는 준비·타격·복귀 각도를 보간한다. 모델 경계 측정은 스킨 메시의 실제 BakeMesh 경계를 사용한다.

