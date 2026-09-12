# 숨결섬 (가제)

Unity 6000.5.4f1 기반 상용 3D 힐링 낚시 게임 프로젝트입니다. 현재 구현은 첫 달 버티컬 슬라이스 범위입니다.

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

1. `숨결섬 > 플레이 씬 생성`으로 `Assets/BreathIsland/Scenes/BeachVerticalSlice.unity`를 생성합니다.
2. 생성된 씬을 열고 Play를 누릅니다.
3. `숨결섬 > Windows 검증 빌드 (Mono)`로 `Builds/Windows/BreathIsland.exe`를 생성합니다.

배치 빌드:

```powershell
.\Build-BreathIsland.ps1
```

MSVC C++ Build Tools와 Windows SDK가 설치된 출시 환경에서는 다음 명령으로 IL2CPP 빌드를 생성합니다.

```powershell
.\Build-BreathIsland.ps1 -ReleaseIl2Cpp
```

## 코드 구조

런타임은 낚시, 포식자 전투, 관계, 시설, 콘텐츠, 현지화, UI, 코어 흐름으로 분리되어 있습니다. 씬 진입점은 기능 객체를 조립하고 상태별 업데이트만 전달합니다. 세부 의존 규칙은 [Documentation/ARCHITECTURE.md](Documentation/ARCHITECTURE.md)를 참고하세요.

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
