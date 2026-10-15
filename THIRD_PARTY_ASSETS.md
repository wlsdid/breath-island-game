# Third-party asset register

## 포트폴리오 표시 정리 — 2026-10-12

새 게임 아트·폰트 반입 없음. 기존 Kenney road 모델에 프로젝트 작성 VillagePath URP 재질을 적용했다. 모델·충돌·배치 좌표는 유지한다. 대표 화면에는 아래 등록된 KayKit·Kenney 모델과 Gaegu·Gowun Dodum 서체가 포함된다. UI 색·문양과 레이아웃 작성, 기존 팩의 모델 제작 기여를 구분한다.

녹화용 imageio-ffmpeg 0.6.0/FFmpeg 도구는 Git 제외 Builds/Portfolio/Tools에만 설치했다. 게임 실행 파일이나 소스 에셋에 포함하지 않으며 제3자 실행 도구 자체를 재배포하지 않는다.

Unity 패키지는 `Packages/manifest.json` 및 각 패키지 라이선스를 따른다. 아래 아트는 사용자가 제공한 `design asset/` 압축파일에서 선별했다. 원본 압축파일은 수정하지 않았다.

외부 에셋을 추가할 때 아래 항목을 반드시 기록합니다.

| Asset | Author | Source URL | License | Version/date | Local path | Modifications |
|---|---|---|---|---|---|---|
| KayKit Furniture Bits | Kay Lousberg | 사용자 제공 `design asset/KayKit_Furniture_Bits_1.0_FREE.zip`; 제작자 www.kaylousberg.com (동봉 문서) | CC0, 동봉 `License.txt` | 1.0 / 원본 2023-09-08 | `Assets/BreathIsland/Art/ThirdParty/KayKit/Furniture` | Unity용 FBX 12종과 공용 텍스처만 추출, 원본 내용 변경 없음 |
| KayKit Dungeon Pack | Kay Lousberg | 사용자 제공 `design asset/KayKit Dungeon Pack 1.0.zip`; 제작자 www.kaylousberg.com (동봉 문서) | CC0, 동봉 `License.txt` | 1.0 / 원본 2020-11-13 | `Assets/BreathIsland/Art/ThirdParty/KayKit/HouseParts` | FBX 8종만 추출, 원본 내용 변경 없음 |
| KayKit Adventurers | Kay Lousberg | 사용자 제공 `design asset/KayKit_Adventurers_2.0_FREE.zip`; 제작자 www.kaylousberg.com | CC0, 동봉 `License.txt` | 2.0 / 원본 2025-10-22 | `Assets/BreathIsland/Art/ThirdParty/KayKit/Character` | Ranger·텍스처·Rig Medium 애니메이션 2개 추출. Unity Generic 임포트·루프 설정, URP 재질·대기/걷기 컨트롤러 작성 |
| KayKit Fantasy Weapons Bits | Kay Lousberg | 사용자 제공 `design asset/KayKit_FantasyWeaponsBits_1.0_FREE.zip`; 제작자 www.kaylousberg.com | CC0, 동봉 `License.txt` | 1.0 / 원본 2026-02-19 | `Assets/BreathIsland/Art/ThirdParty/KayKit/Weapons` | 검·활·지팡이 및 텍스처 추출, URP 재질과 런타임 크기 정규화 |
| KayKit RPG Tools Bits | Kay Lousberg | 사용자 제공 `design asset/KayKit_RPGToolsBits_1.0_FREE.zip`; 제작자 www.kaylousberg.com | CC0, 동봉 `License.txt` | 1.0 / 원본 2025-11-26 | `Assets/BreathIsland/Art/ThirdParty/KayKit/Tools` | 도끼·곡괭이·망치 및 텍스처 추출, URP 재질과 런타임 크기 정규화 |
| KayKit Forest Nature | Kay Lousberg | 사용자 제공 `design asset/KayKit_Forest_Nature_Pack_1.0_FREE.zip`; 제작자 www.kaylousberg.com | CC0, 동봉 `License.txt` | 1.0 / 원본 2025-04-29 | `Assets/BreathIsland/Art/ThirdParty/KayKit/Nature` | 나무·바위·덤불 및 텍스처 추출. 덤불은 약초 채집 표현, URP 재질·크기·충돌체 적용 |

선별 근거와 보류 항목은 `Documentation/ASSET_SELECTION.md`, 파일별 원본 경로·SHA-256은 `Documentation/ASSET_SELECTION.json`에 기록한다. 여섯 팩의 라이선스 원문은 각 대상 폴더에 포함했다. 원본 FBX·텍스처 바이트는 변경하지 않았으며 임포트 설정·파생 재질·컨트롤러만 별도로 관리한다. 준비된 가구 12종 중 의자·탁자·침대·조명 4종을 플레이에 연결했다. 나머지 사용자 제공 압축파일의 게임 적용을 의미하지 않는다.

상업적 이용, 수정, 재배포 포함 여부를 확인할 수 없는 에셋은 프로젝트에 추가하지 않습니다.

## UI 서체와 자체 그래픽 — 2026-09-22

| Asset | Author | Source URL | License | Local path | Modifications |
|---|---|---|---|---|---|
| Gaegu Bold | The Gaegu Project Authors | https://github.com/google/fonts/tree/main/ofl/gaegu | SIL OFL 1.1; Gaegu-OFL.txt 동봉, 소프트웨어 포함·임베딩·재배포 허용 | `Assets/BreathIsland/Resources/UI/Fonts/Gaegu-Bold.ttf` | 원본 바이트 그대로, 제목·버튼용 Unity 동적 폰트 임포트 |
| Gowun Dodum Regular | The Gowun Dodum Project Authors | https://github.com/google/fonts/tree/main/ofl/gowundodum | SIL OFL 1.1; GowunDodum-OFL.txt 동봉, 소프트웨어 포함·임베딩·재배포 허용 | `Assets/BreathIsland/Resources/UI/Fonts/GowunDodum-Regular.ttf` | 원본 바이트 그대로, 본문용 Unity 동적 폰트 임포트 |

천 탭·종이 라벨·박음질·물결과 아이템 선화는 프로젝트의 `NotebookGraphic`·`IslandIcon` 코드로 직접 작성했다. 다른 게임의 UI 이미지나 외부 아이콘 팩을 반입하지 않았다. 서체 파일을 단독 판매하거나 수정하지 않는다.

## 제공 원본 확인·미적용 — 2026-09-23

| Asset | Author | Source | License evidence | Status |
|---|---|---|---|---|
| Fantasy Town Kit 2.0 | Kenney | 사용자 제공 `design asset/kenney_fantasy-town-kit_2.0.zip`; 동봉 제작자 주소 www.kenney.nl | 압축 내 `License.txt`: CC0, personal/educational/commercial use 허용, 크레딧 선택 | FBX 167종, 샘플과 구성 확인. 게임 미반입 |
| Food Kit 2.0 | Kenney | 사용자 제공 `design asset/kenney_food-kit.zip`; 동봉 제작자 주소 www.kenney.nl | 압축 내 `License.txt`: CC0, personal/educational/commercial use 허용, 크레딧 선택 | FBX 200종, 구성 확인. 게임 미반입 |

두 원본 압축파일은 변경하지 않았다. 후보와 적용 경계는 `Documentation/ASSET_SELECTION.md`에 기록한다.

### Fantasy Town Kit 게임 적용

2026-09-23 후속 요청으로 FBX 5종·colormap·License.txt를 `Assets/BreathIsland/Art/ThirdParty/Kenney/Town`에 원본 바이트 그대로 반입했다. CC0 원문을 동봉했다. 파생 URP 재질과 실내·출입문 프리팹은 `Resources/World`에 작성했으며 모델 스케일·위치는 프리팹에서 조정했다. 파일별 원본 경로와 해시는 `Documentation/HOUSE_ASSET_SELECTION.json`에 기록한다. Food Kit는 여전히 미적용이다.

2026-09-28 숲속 마을 확장으로 같은 CC0 팩의 벽·지붕·길·울타리·가로등·기둥·나무 FBX를 추가 선별했다(`Documentation/VILLAGE_ASSET_SELECTION.json`). 파생 마을 프리팹과 단색 URP 재질을 작성했으며, 실내 반복 무늬는 프로젝트 에디터 코드로 직접 생성했다. 실내 UV 파생 메시도 별도 에셋으로 저장하며 원본 FBX는 수정하지 않는다. 기존 KayKit 가구 4종을 추가 게임 정의에 연결했다.

## 2026-10-01 보관 상자

KayKit Dungeon Pack 1.0의 빈 일반 상자 본체·뚜껑 FBX 2종과 동봉 CC0 원문을 `Assets/BreathIsland/Art/ThirdParty/KayKit/Storage`에 선별했다. 출처는 사용자 제공 `design asset/KayKit Dungeon Pack 1.0.zip`, 저작자는 Kay Lousberg(www.kaylousberg.com)이다. 원본 모델·ZIP은 변경하지 않았으며 파일별 해시는 `Documentation/STORAGE_ASSET_SELECTION.json`에 기록한다. 상자 UI 아이콘과 기존 캐릭터의 앉기 자세 레이어는 프로젝트에서 직접 작성했다.

## 2026-10-04 모델 기반 인벤토리 아이콘

기존 KayKit 도구·무기·가구·자연 모델을 프로젝트의 `InventoryIconAuthoring`으로 렌더링하여 투명 PNG 아이콘을 작성했다. 원본 메시와 텍스처는 변경하지 않았다. 별도 외부 UI 그래픽을 사용하지 않는다.

자원 아이콘용으로 사용자 제공 `KayKit_ResourceBits_1.0_FREE.zip`에서 Wood_Log_A, Stone_Chunks_Small FBX 두 개, resource_bits_texture.png와 License.txt만 선별했다. 동봉 라이선스는 Kay Lousberg의 CC0이며 상업 이용을 허용한다. 경로·SHA-256은 `Documentation/RESOURCE_ICON_SELECTION.json`에 기록한다. 재질은 아이콘 작성 중에만 사용하고 새 자원 종류·보상·게임 규칙은 추가하지 않는다.


## 2026-10-04 격자 건축·마을 형태

기존 Kenney Fantasy Town Kit의 CC0 wall-wood·지붕·문 부품을 재사용했다. BuildWall은 원본 모델의 파생 프리팹이고 BuildFloor는 자체 기본 메시를 사용한다. 마을 주택 두 동은 기존 점유 범위 안에서 별채와 높이가 다른 몸체로 다시 조립했다. 신규 다운로드·원본 모델 변경은 없다. 기존 HOUSE_ASSET_SELECTION.json과 VILLAGE_ASSET_SELECTION.json의 출처·해시가 적용된다.

## 2026-10-15 기존 NPC 외형 보완

### 같은 날 추가한 해안·낚시 표현

낚싯대·릴·가이드·단순화한 손, 해안 메시와 CoastalBlend 셰이더는 프로젝트 코드로 작성했다. 외부 UI 이미지나 새 모델은 반입하지 않았다. 물고기 도약에는 기존 Generated 폴더의 어류 5종·포식자 1종을 재사용한다. 기존 에셋의 출처·라이선스 상태를 그대로 유지하며 사용자 제공 원본 ZIP·FBX를 변경하지 않는다.

사용자 제공 KayKit_Adventurers_2.0_FREE.zip에서 Rogue.fbx, Mage.fbx, rogue_texture.png, mage_texture.png와 License.txt를 선별했다. 제작자는 Kay Lousberg(www.kaylousberg.com), 동봉 라이선스는 CC0이며 상업 이용·수정·재배포를 허용한다. 원본 바이트는 유지하며 Assets/BreathIsland/Art/ThirdParty/KayKit/Villagers에 둔다. 파일 해시는 Documentation/NPC_ASSET_SELECTION.json에 기록한다. 기존 Medium Generic 리그·애니메이션을 재사용한다.

창틀·차양·벤치·굴뚝 등 마을 추가 구조는 기본 메시로 작성한 파생 프리팹이며 화단 식물은 이미 선별된 KayKit Forest Nature Bush를 재사용한다. 기존 Kenney 건물·원본 에셋은 보존한다.

### 수면 표현 추가

OceanWater 셰이더와 OceanSurface 메시·해저 재질은 자체 작성 코드이며 외부 그래픽 파일을 반입하지 않았다. 수중 물고기는 앞서 사용한 Generated 모델을 재사용한다. 원본 에셋과 기존 라이선스 상태는 유지한다.

낚시 후속 표현은 기존 자체 생성 물고기 FBX 6종을 파생 프리팹으로 재사용했다. 새 색상 재질·꼬리 동작·물방울·파문은 프로젝트 코드로 작성했으며 외부 다운로드나 원본 변경은 없다.

낚시 손은 외부 모델 추가 없이 자체 기본 메시로 손바닥·엄지·접힌 손가락·청록 소매를 구성했다. FishingHandAuthoring으로 파생 프리팹을 작성하며 원본 캐릭터·에셋은 보존한다.

### 2026-10-15 잔디와 실제 캐릭터 손

Kay Lousberg의 KayKit Forest Nature Pack 1.0에서 Grass_1_A_Color1.fbx·Grass_1_B_Color1.fbx를 추가 선별했다. 제공된 ZIP의 Assets/fbx 경로가 출처이며 동봉 CC0 라이선스는 상업 이용·수정·배포를 허용한다. 기존 forest_texture.png 및 Nature/License.txt를 재사용한다.

낚시 손 파생 메시는 기존 KayKit Adventurers Ranger의 팔/손과 텍스처에서 작성했다(CC0). 원본 FBX·텍스처는 보존한다. 집의 보강 메시와 지면 셰이더는 자체 코드이며 추가 구매·다운로드는 없다.

2026-10-15: 기존 CC0 KayKit 캐릭터 애니메이션의 WalkingLoop·IdleLoop·AirLoop 파생 .anim 에셋을 추가했다. 원본 FBX는 변경하지 않았으며 출처와 라이선스는 기존 KayKit Adventurers 기록을 따른다.

2026-10-15 KayKit Fantasy Weapons Bits 1.0 (Kay Lousberg, CC0): 제공된 팩의 Unity용 sword_E, bow_A, bow_B, staff_B, arrow_A를 추가 사용한다. Resources/World/Weapons의 파생 프리팹·색 재질과 자체 줄·스킬 표현을 연결한다. 원본과 기존 라이선스 파일은 보존한다.

## Brackeys VFX bundle (2026-10-15)

- 사용자 제공 `design asset/brackeys_vfx_bundle.zip`에서 predrawn 3종만 선별했다. 원본 ZIP은 보존한다.
- 적용 위치: `Assets/BreathIsland/Art/ThirdParty/BrackeysVfx/`.
- 포함된 `LICENSE & CREDITS.txt`는 전체 팩을 CC0로 명시한다. Brackeys 재패키징, predrawn 원저자 CodeManu (https://codemanu.itch.io/vfx-free-pack). 원문 라이선스를 함께 보관한다.
- 검: big_hit, 활: star_explosion, 지팡이: electric_ring. PNG를 수정하지 않고 URP용 플립북 셰이더·재질·프리팹으로 재생한다.
- `design asset/tornado vfx.zip`은 Godot 프로젝트다. 출처 https://soygus.itch.io/tornado-vfx-godot 에서 개인·상업 프로젝트 사용과 수정을 허용하며 재배포·재판매·재패키징은 금지한다(2026-10-15 확인). 이번에는 이식하지 않았으며 공개 저장소에 원본을 포함하지 않는다.
