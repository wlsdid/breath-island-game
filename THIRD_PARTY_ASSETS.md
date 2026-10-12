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
