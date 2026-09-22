# Third-party asset register

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
