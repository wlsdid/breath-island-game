# 숨결섬 (가제)

Unity 6000.5.4f1 기반 3D 힐링 낚시 게임 프로젝트입니다. 콘텐츠 범위는 첫 달 버티컬 슬라이스이며, 콘텐츠 에셋·Localization·화면 모듈 구조로 분리되어 있습니다.

테스트·Windows 빌드·실행 화면 검증 결과는 [Documentation/VALIDATION.md](Documentation/VALIDATION.md)를 참고하세요.

## 현재 플레이 범위

- 절차형 로우폴리 해안 마을과 3인칭 캐릭터 이동
- 1인칭 낚시, 5종 일반 어류, 1종 포식자
- 판매·전시·도감·재료 처리
- 조선소·수족관 2단계 성장
- NPC 2명과 누적형 호감도
- 자동 저장 1개, 수동 저장 3개
- 한국어·영어 전환, 키보드·마우스와 Xbox 계열 게임패드

## 실행 및 빌드

Unity Hub에서 Personal 라이선스를 활성화한 뒤 프로젝트를 엽니다.

1. 첫 임포트에서 `LocalizationSetup`이 기존 한국어·영어 테이블을 Unity Localization/Addressables에 등록합니다. 필요하면 `숨결섬 > Localization 설정 검증`을 실행합니다.
2. 기존 `Assets/BreathIsland/Scenes/BeachVerticalSlice.unity`를 열고 Play를 누릅니다. 씬이 없을 때만 `숨결섬 > 플레이 씬 생성`을 사용합니다.
3. `숨결섬 > Windows 검증 빌드 (Mono)`로 `Builds/Windows/BreathIsland.exe`를 생성합니다. 기존 씬은 빌드 중 재생성하지 않습니다.

Localization 설정 등록, Edit Mode·Play Mode 테스트, Windows Mono 배치 빌드:

```powershell
.\Build-BreathIsland.ps1
```

테스트 결과는 `Builds/editmode-results.xml`, `Builds/playmode-results.xml`에 기록됩니다. `-SkipTests`는 테스트를 생략하며 완료 조건 검증용으로 사용하지 않습니다.

MSVC C++ Build Tools와 Windows SDK가 설치된 출시 환경에서는 다음 명령으로 IL2CPP 빌드를 생성합니다.

```powershell
.\Build-BreathIsland.ps1 -ReleaseIl2Cpp
```

## 코드 구조

런타임은 낚시, 포식자 전투, 관계, 시설, 콘텐츠, 현지화, UI, 코어 흐름으로 분리되어 있습니다. `BreathIslandGame`은 기능 객체 조립과 프레임 전달, `GameHud`는 8개 화면의 라우팅을 담당합니다.

- 콘텐츠: `Assets/BreathIsland/Resources/Content/VerticalSlice.asset`에서 기존 어종·NPC·시설 에셋을 참조합니다. 수치와 저장용 ID는 에셋에서 유지합니다.
- 문자열: `Assets/BreathIsland/Localization`의 한국어·영어 Unity String Table에서 편집합니다. 콘텐츠 에셋에는 번역 키를 저장합니다.
- 저장: JSON 버전 1과 자동 1개·수동 3개 슬롯 형식을 유지합니다. 마이그레이션은 기존 파일을 읽을 때 원본을 수정하지 않습니다.

새로운 콘텐츠·그래픽·Steamworks·장비 시스템·경제 밸런스 변경은 포함하지 않습니다. 세부 경계는 [Documentation/ARCHITECTURE.md](Documentation/ARCHITECTURE.md)를 참고하세요.

## 기본 조작

- 이동: `WASD` / 왼쪽 스틱
- 카메라: 마우스 / 오른쪽 스틱
- 상호작용: `E` / 게임패드 South
- 낚시 시작·캐스팅·감기: `F`, `Space` / 게임패드 South
- 줄 느슨하게: `S` / 게임패드 East
- 마을 성장: `M`
- 도감: `Tab`
- 수동 저장: `F5`, 불러오기: `F9`
- 언어 변경: `L`
- 메뉴 닫기: `Esc`
