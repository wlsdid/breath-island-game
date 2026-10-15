# 확장용 디자인 에셋 선별 — 2026-09-22

## 해안·낚시 표현 — 2026-10-15

제공된 `KayKit_RPGToolsBits_1.0_FREE.zip`의 목록에서 낚싯대 모델을 찾지 못해 신규 파일을 반입하지 않았다. 낚싯대 몸체·코르크 손잡이·릴·가이드와 단순화한 손은 `CoastalVisualAuthoring`으로 프로젝트 내에서 작성한다. 휘어지는 메시, 해안 장식, 경계 혼합 셰이더도 자체 생성이다. 기존 `Generated/BI_Fish_*` 5종과 `BI_Predator_Breaker`를 도약 모델로 재사용한다. 원본 ZIP·FBX와 기존 캐릭터 애니메이션은 변경하지 않는다.

## 포트폴리오 보완 — 2026-10-12

추가 모델 선별 없이 기존 Kenney road 모델의 재질을 따뜻한 모래빛 VillagePath로 통일했다. 원본 팩과 보행 동선·충돌체는 보존했다. 실내 시연은 기존 가구 9종으로 생활 구역을 구성한 합성 저장이며, 일반 시작 시 가구를 추가 지급하는 변경은 아니다.

## 확정된 기능 범위

- 검·활·지팡이 장착으로 해당 직업 전환. 일반 무기는 기본 공격만, 도전과제로 해금된 특수 무기는 스킬 사용. 직업별 숙련도는 스킬만 강화한다.
- 생활 도구에는 직업 전환과 스킬이 없다.
- 벌목·채광·채집과 집 꾸미기 포함. 농사는 이번 범위에서 제외.
- 이 문서는 에셋 준비 상태이며 위 기능이 구현됐음을 의미하지 않는다.

위 문장의 준비 상태는 최초 선별 시점 기준이며, 아래 ‘기능 연결’에 후속 적용 내용을 기록한다.

## 이번에 반입한 에셋

원본은 `design asset/`에 보존한다. 전체 팩 대신 아래 파일, 필요한 공용 텍스처, 라이선스만 `Assets/BreathIsland/Art/ThirdParty/KayKit/`에 추출했다. Furniture 팩의 `sample.png`와 Dungeon 팩의 `Overview.png`를 확인하여 용도를 구분했다.

| 폴더 | 선택 모델 | 용도 |
| --- | --- | --- |
| Furniture | bed_single_A, chair_A_wood, table_medium, table_low, couch | 기본 생활 가구 |
| Furniture | cabinet_medium, shelf_A_small | 수납 가구 |
| Furniture | lamp_standing, lamp_table, rug_rectangle_A, cactus_small_A, pictureframe_small_A | 조명·바닥·식물·벽 장식 |
| HouseParts | wall, wallCorner, wall_door, wall_window, door, tileBrickA_large | 임시 석조 실내 구조 |
| HouseParts | crate, weaponRack | 수납 상자·무기 전시 소품 |

총 FBX 20종, 가구 공용 텍스처 1개, 라이선스 2개다. 장식 변형·중복 포맷·함정·해골·던전 전투 소품은 가져오지 않았다. 무기 모델은 기존 Fantasy Weapons 팩을 사용할 후보이므로 Dungeon 팩의 무기는 중복 반입하지 않았다.

파일별 원본 압축 경로와 추출 파일 해시는 [ASSET_SELECTION.json](ASSET_SELECTION.json)에 기록한다. 두 팩은 동봉 문서에 CC0와 상업적 이용 가능 문구가 있다. 별도 런타임 코드나 저장 데이터는 변경하지 않았다.

## 기존 제공 팩의 역할

| 팩 | 사용할 후보 범위 | 상태 |
| --- | --- | --- |
| Adventurers / Universal Animation Library | 캐릭터와 이동·행동 애니메이션 | 리깅·애니메이션 호환성 확인 후 적용 |
| Fantasy Weapons Bits | 검·활·지팡이와 필요한 투사체 | 장비 기능 구현 시 선별 |
| RPG Tools Bits | 도끼·곡괭이 등 생활 도구 | 생활 활동 구현 시 선별 |
| Resource Bits / Forest Nature | 자원·나무·바위 등 육지 환경 | 채집 대상과 배경을 구분하여 선별 |
| ocean animals / 개별 Fish·Shark FBX | 기존 어종 표현 후보 | 출처·라이선스 확인 자료가 아직 없어 반입 보류 |

## 보완할 항목과 적용 경계

- 집 꾸미기 기본 가구는 확보됐다. 현재 석조 구조는 임시 구성용이며, 아늑한 주택 외관·지붕·실내 마감은 추가 에셋으로 보완할 수 있다.
- 집 외관 에셋이 없어도 실내 가구 배치 기능을 먼저 구현할 수 있다.
- 모델 반입과 게임 배치는 별개다. 실제 배치 전 URP 재질, 크기·축·피벗, 충돌체와 가구 점유 영역을 확인한다. 무기 전시대 자체에 장비 규칙을 넣지 않는다.
- 원본 팩의 프리뷰 이미지는 실제 프로젝트 렌더링 검증을 대신하지 않는다.
- 신규 어종이나 던전 콘텐츠는 이번 에셋 선별로 추가하지 않는다.

## 반입 검증

- 추출 파일 23개의 SHA-256이 압축파일 내 원본과 모두 일치한다.
- Unity 6000.5.4f1 배치 임포트 완료, FBX 20개 모두 `.meta` 생성 확인. C# 컴파일 오류·모델 임포트 오류는 발견되지 않았다.
- 로그: `Builds/AssetReview/import.log`. 시작 시 라이선스 토큰 갱신 메시지가 있었으나 이후 entitlement 확인과 정상 배치 종료를 기록했다.
- 이번 작업은 에셋 준비만 수행했다. 게임 장면 배치·재질 렌더링·가구 조작은 아직 검증하지 않았고, 기존 테스트나 Windows 빌드를 재실행하지 않았다.

## 기능 연결

- `Furniture`: 준비한 12종 중 의자·탁자·침대·조명 4종을 제작·배치 정의에 연결했다. 나머지는 향후 콘텐츠용으로 보존한다.
- `Weapons`: sword_A, bow_A_withString, staff_A와 공용 텍스처. 일반/특수 무기는 같은 원본 외형을 공유하며 표시 이름·해금·스킬 정의로 구분한다.
- `Tools`: axe, pickaxe, hammer와 공용 텍스처. 생활 도구의 직업·스킬 정의는 만들지 않는다.
- `Nature`: Tree_1_A_Color1, Rock_1_A_Color1, Bush_1_A_Color1 및 공용 텍스처. 덤불을 약초 채집 대상으로 사용한다.
- `Character`: Ranger, ranger_texture, Rig_Medium_General, Rig_Medium_MovementBasic. 같은 팩의 Generic 리그로 대기·걷기 컨트롤러를 구성한다. Universal Animation Library는 중복 반입하지 않는다.
- 원본 파일 43개(기존 23 + 후속 20)의 경로·해시는 JSON 목록에 기록한다. 파생 URP 재질, 컨트롤러, 콘텐츠 정의는 `Resources/Content/Life`에 둔다.
- 모델 원본은 수정하지 않고 표현 루트에서 바닥 피벗과 높이를 정규화한다. 가구는 정의된 점유 영역 안에 맞추고 배치 충돌은 격자에서 별도로 검사한다.
- 내부 바닥·낮은 벽은 배치 시야를 확보하기 위한 기본 도형으로 구성한다. 주택 외관·지붕은 제외한다. 기존 HouseParts 에셋은 준비 상태로 유지한다.
- 새로 제공된 Restaurant 팩, 출처 미확인 해양 생물, 나머지 도구·무기 변형은 이번 기능에 필요하지 않아 원본 폴더에 보존한다.
- 최종 테스트·빌드·화면 검증 결과는 `VALIDATION.md`에 기록한다.

## UI 개편 적용 — 2026-09-22

기존 선별 모델을 유지하고 UI 아이콘은 직접 작성한 선화로 통일했다. 제목·행동 버튼은 Gaegu Bold, 본문은 Gowun Dodum Regular를 사용하며 공식 Google Fonts 저장소 원본과 OFL 문서를 함께 반입했다. 별도 이미지 팩을 사용하지 않는다. 화면·공통 요소 프리팹은 `Resources/UI`, 디자인 기준은 `UI_DESIGN.md`에 있다. `design asset/` 원본은 변경하지 않았다.

## 추가 제공 팩 확인 — 2026-09-23

- `kenney_fantasy-town-kit_2.0.zip`: Fantasy Town Kit 2.0, FBX 모델 167종과 OBJ·GLB 대체 포맷, 공용 `colormap.png`. 동봉 `License.txt`에 제작자 Kenney, CC0 및 상업 이용 허용 명시. `Sample.png`를 확인했다. 완성된 집 프리팹이 아닌 벽·문·창·바닥 판재·계단·지붕 등을 조합하는 모듈형 팩이다.
- 실내 마감 후보: `wall-wood.fbx`, `wall-wood-corner.fbx`, `wall-wood-doorway-square.fbx`, `wall-wood-door.fbx`, `planks.fbx`. 기존 가구와 함께 쓸 수 있는 구조 부품 후보이며 실제 치수·피벗·재질·충돌 및 뒷면 렌더링은 Unity에서 별도 검증해야 한다.
- `kenney_food-kit.zip`: Food Kit 2.0, FBX 모델 200종과 OBJ·GLB 대체 포맷. 음식·그릇·조리 소품이며 동봉 라이선스는 Kenney CC0, 상업 이용 허용. 신규 요리 기능을 추가하거나 기존 가구 정의를 늘리지 않는다.
- 이번 확인은 압축 구성·라이선스·주택 샘플 검토다. 모델은 `Assets/`에 반입하지 않았으며 Unity 임포트·실행 호환성을 검증한 상태는 아니다. 외관·지붕 적용은 기존 보류 범위를 유지한다. 원본 압축파일과 진행 중 UI·도구 수정은 보존했다.

### 후속 게임 적용

사용자의 적용 요청에 따라 Fantasy Town Kit의 `wall-wood`, `wall-wood-window-small`, `wall-wood-doorway-square`, `wall-wood-door`, `planks` FBX 5종과 공용 colormap, License.txt를 `Art/ThirdParty/Kenney/Town`에 선별 반입했다. 파일별 원본 경로·SHA-256은 `HOUSE_ASSET_SELECTION.json`에 있다.

`Resources/World/HomeInterior.prefab`은 기존 8×8 실내를 목재 마루와 창문 있는 벽으로 교체한다. 벽은 배치 영역 밖에 두고 카메라 쪽을 열어 가구 조작 시야를 확보한다. `HomeDoorway.prefab`은 기존 야외 입구 표시를 문틀·문으로 바꾼다. 집 전체 외관·지붕과 Food Kit는 적용하지 않았다. 기존 저장 위치·가구 점유 영역·출입 규칙은 그대로다. 모델은 정적 구조물이므로 캐릭터 리깅 변경은 없다.

## 숲속 마을·꾸미기 확장 — 2026-09-28

- 사용자 승인으로 외관·지붕 보류를 해제했다. Kenney Town의 벽, 박공 지붕, 길, 울타리, 가로등, 기둥, 나무를 선별했다. 추가 원본 경로와 해시는 `VILLAGE_ASSET_SELECTION.json`에 기록한다. Food Kit는 적용하지 않는다.
- 기존 KayKit 가구 중 couch, cabinet_medium, shelf_A_small, table_low를 신규 가구 정의에 연결한다. 원본 FBX 변경 없이 크기·점유 영역을 정의한다.
- ForestVillage 프리팹은 기존 시설 2동·주거 2동과 플레이어 집, 광장·길·장식을 포함한다. 기존 집 출입 좌표는 유지하며 외관을 그 뒤에 둔다.
- 실내 마감용 반복 무늬 6종은 에디터 코드로 직접 작성한다. 원본 메시 대신 파생 메시의 UV만 실내 좌표에 맞춰 생성하여 판재·타일 무늬가 이어지게 한다. 원본 압축파일과 모델 바이트는 보존한다.

## 2026-10-01 실내 보관 상자

KayKit Dungeon Pack 1.0의 `Models/fbx/chest_common_empty.fbx`와 `chestTop_common.fbx`를 별도 Storage 폴더로 선별했다. 내용물이 없는 본체와 뚜껑을 StorageChest 프리팹으로 조립하고 자체 목재색 URP 재질을 사용한다. 동봉 CC0 라이선스는 상업 사용을 허용한다. 원본 ZIP·FBX는 수정하지 않았다. 1×1 점유·0.85m 높이의 게임 정의와 자체 선화 아이콘을 사용한다. 상자별 16칸 보관 규칙은 Housing 기능에 있으며 모델에는 게임 규칙이 없다. 파일 경로·해시는 `STORAGE_ASSET_SELECTION.json`에 기록한다.

## 2026-10-04 아이콘 표현

기존 도구 3종·무기 6종·가구 9종의 실제 모델과 게임 재질로 아이콘을 렌더링했다. Resource Bits의 Wood_Log_A·Stone_Chunks_Small, 공용 텍스처와 CC0 문서를 선별하여 나무·돌 아이콘에 사용한다. 약초는 기존 Bush 모델에서 렌더링한다. 새 자원 콘텐츠나 보상은 없다. 파생 PNG는 Resources/UI/ItemIcons, 작성 명령은 InventoryIconAuthoring.Build이며 원본 ZIP·FBX는 보존한다. 세부 출처·해시는 RESOURCE_ICON_SELECTION.json에 있다.


## 2026-10-04 격자 건축·마을 형태

기존 Kenney Fantasy Town Kit의 CC0 wall-wood·지붕·문 부품을 재사용했다. BuildWall은 원본 모델의 파생 프리팹이고 BuildFloor는 자체 기본 메시를 사용한다. 마을 주택 두 동은 기존 점유 범위 안에서 별채와 높이가 다른 몸체로 다시 조립했다. 신규 다운로드·원본 모델 변경은 없다. 기존 HOUSE_ASSET_SELECTION.json과 VILLAGE_ASSET_SELECTION.json의 출처·해시가 적용된다.

## 2026-10-15 NPC·마을 표현

- 제공된 KayKit Adventurers 2.0 FREE의 Rogue, Mage FBX와 각각의 텍스처, CC0 License.txt를 Villagers 폴더에 선별했다. 원본 ZIP과 기존 Ranger는 변경하지 않았다. 원본 경로·SHA-256은 NPC_ASSET_SELECTION.json에 있다.
- 기존 Nari/Maru의 도형 외형을 각각 Rogue/Mage 파생 프리팹으로 교체한다. 신규 NPC·직업·대사·일정은 추가하지 않는다. 기존 Medium Generic Avatar와 Idle 컨트롤러를 재사용한다.
- 마을은 기존 건물·길을 보존하면서 자체 기본 메시로 창틀·회벽 패널·청록 차양·벤치·굴뚝·광장 가장자리를 작성하고, 기존 KayKit Bush로 화단을 구성한다. 화단은 채집 대상과 분리된 장식이다. 추가 다운로드는 없다.

## 2026-10-15 수면 표현

OceanWater 셰이더, OceanSurface 파도 메시와 해저 재질은 프로젝트 코드로 작성했다. 기존 Generated 물고기를 수중 이동에 재사용하며 신규 외부 파일·텍스처·다운로드는 없다. 기존 모델의 출처 기록과 design asset 원본을 유지한다.

### 2026-10-15 물고기·물보라 보완

기존 자체 생성 Generated 어류 6종의 FBX는 보존하고 World/Fish에 파생 프리팹과 URP 재질을 작성했다. 원본 생성 스크립트의 어종별 색 구분을 참고해 몸통·배·눈을 분리했다. 꼬리·지느러미 동작과 물방울·파문은 프로젝트 코드로 작성하며 새 외부 에셋은 없다.

## 2026-10-15 잔디·낚시 손 재선별

- 제공된 KayKit_Forest_Nature_Pack_1.0_FREE.zip의 Assets/fbx/Grass_1_A_Color1.fbx와 Grass_1_B_Color1.fbx만 추가 선별했다. 기존 forest_texture.png와 Nature/License.txt(CC0, Kay Lousberg)를 재사용한다. 원본 ZIP은 보존한다.
- 낚시 손은 기존 CC0 KayKit Adventurers Ranger_ArmLeft/Right 메시에서 손·아래팔 삼각형을 추출한 파생 메시다. 기존 ranger_texture.png를 사용하며 원본 Ranger.fbx는 변경하지 않는다.
- Ground 셰이더·집 벽 뒷면·박공 끝면은 프로젝트 코드로 작성했다. 새 외부 다운로드는 없다. Universal Animation Library의 CC0 라이선스는 확인했지만 기존 Generic 리그와 다른 애니메이션을 이번에 직접 적용하지 않았다.

2026-10-15 연속 이동 수정: 기존 KayKit Rig_Medium_MovementBasic/General에서 WalkingLoop·IdleLoop·AirLoop 파생 AnimationClip을 작성했다. 원본 FBX와 가져오기 설정은 보존하고 파생 클립의 반복 재생만 명시했다. 추가 다운로드나 외부 애니메이션은 사용하지 않았다.

2026-10-15 무기 식별: 제공된 KayKit_FantasyWeaponsBits_1.0_FREE.zip의 Assets/fbx(unity)/sword_E.fbx, bow_A.fbx, bow_B.fbx, staff_B.fbx, arrow_A.fbx만 추가 선별했다. 기존 Weapons/License.txt의 CC0와 weapons_bits_texture.png를 사용한다. 원본 ZIP·기존 모델은 보존했다. 일반 활은 정적 줄 없는 모델로 바꾸고 자체 동적 줄을 연결했다. 신규 외부 다운로드 없음.
- 무기 아이콘 6개만 새 모델로 재렌더링했다. 특수 무기는 파생 재질 색을 유지하며 기존 도구·자원·가구 아이콘은 재생성하지 않았다.

## 2026-10-15 특수 무기 VFX 선별

Brackeys VFX bundle의 CC0 predrawn 세 장을 선택했다. big_hit은 6×5 중 빈 첫 프레임을 제외한 11프레임, star_explosion은 파일명과 달리 실제 7×6 배열의 29프레임, electric_ring은 6×5의 29프레임을 재생한다. 효과는 대상 위치에서 0.8초 동안 카메라를 향하며 일반 무기에는 표시하지 않는다. 원본 PNG·ZIP은 변형하지 않았다. tornado vfx는 출처 https://soygus.itch.io/tornado-vfx-godot 에서 사용·수정 허용과 에셋 재배포 금지를 확인했다. Godot→Unity 이식과 제한 에셋 배포 분리는 이번 적용에서 제외했다.

## 2026-10-15 직업 외형·허수아비

Adventurers 2.0의 Knight/Mage와 각 텍스처를 선별했다. 기존 Ranger와 같은 뼈 이름을 확인하고 스킨만 교체해 손 소켓과 애니메이션을 유지한다. 훈련 허수아비는 짚 머리·모자·목재 지지대·천 몸통·표적 패치로 만든 작성된 프리팹이다. 원본 에셋 수정 없음.

## 궁수 외형 추가 선별

KayKit_Adventurers_2.0_FREE.zip의 Characters/fbx/Rogue_Hooded.fbx와 rogue_texture.png를 선별했다. 기본 Ranger와 활 직업을 구분하기 위한 후드형 스킨이며 같은 뼈 이름 검사를 통과했다. 원본 ZIP은 보존한다. 라이선스는 기존 Character/License.txt의 CC0이다.
