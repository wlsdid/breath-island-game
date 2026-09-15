# 검증 기록 — 2026-09-15

## 최종 자동 검증

- Unity 6000.5.4f1, Windows Standalone x64, Mono.
- Edit Mode: **27/27 통과**, 실패 0.
- Play Mode: **10/10 통과**, 실패 0. 실제 Game View에서 실행하여 IMGUI 호출도 검증했다.
- `Build-BreathIsland.ps1` 전체 실행 성공: Localization 등록 → 두 테스트 모드 → Windows Mono 빌드.
- `Builds/Windows/BreathIsland.exe` 빌드 성공, 약 100.4 MB. 한·영 String Table, 공통 키, Locale Addressables 번들 포함 확인.
- C# 컴파일 오류 없음. 콘텐츠 에셋 참조, 102개 양 언어 키와 포맷 인자, 저장 호환성 검사 통과.

## 검증 범위

- 기존 어종 6종·시설 2종·NPC 2명의 에셋 영속성, 기존 ID·비용·보상 유지.
- v1 전체 필드 왕복 보존, v0 누락 필드 복구, 미래 버전 거부, 백업 복구, 자동 저장 1개와 수동 저장 3개의 독립성.
- 탐험→낚시→처리→시설 성장→NPC 대화·선물, 중복 보상 방지.
- 포식자 승리·패배, 기존 피해·보상·해안 복귀 규칙.
- 화면 전환의 위치·인벤토리·대상 참조 유지, 언어 전환, 슬롯 불러오기.
- 줄 끊김·재시도, 야간 빈 어종 후보, 월드 아래 낙하 좌표 복구.
- 8개 HUD의 한국어·영어 렌더링, Composition Root의 실제 월드·카메라·오디오 초기화.

## Windows 실행 확인과 발견 사항

첫 Windows 빌드가 실제 저장 파일을 읽고 한국어 HUD를 표시했으며, 시작 로그에서 게임 예외는 발생하지 않았다. 이때 기존 저장 좌표가 섬 밖의 y=-464 부근에 있어 계속 낙하하는 진행 차단 문제를 발견했다.

`PlayerPositionSystem`으로 정상 저장 좌표는 유지하고 y < -5인 위치만 안전한 해안으로 복구하도록 수정했다. 복구 시 자원은 차감하지 않는다. 관련 회귀 테스트를 추가한 뒤 전체 테스트와 Windows 빌드를 다시 통과했다.

첫 실행 검증 전후 실제 저장 파일 2개의 해시가 같음을 확인했다. 테스트용 게임 프로세스는 자동 저장 없이 종료했다. 최종 수정본의 별도 Windows 화면 자동화 재실행은 사용자의 물리 Escape 입력으로 중단되었으며, 이후 컴퓨터 조작은 수행하지 않았다. 최종 수정본의 실제 Game View 렌더링과 부트스트랩은 Play Mode 테스트로 검증했다. 실제 게임패드 장치를 사용한 수동 검사는 수행하지 않았다.

## 실행 방법 및 결과 파일

```powershell
.\Build-BreathIsland.ps1
```

- `Builds/localization-setup.log`
- `Builds/editmode-results.xml`, `Builds/editmode-tests.log`
- `Builds/playmode-results.xml`, `Builds/playmode-tests.log`
- `Builds/build.log`, `Builds/Windows/BreathIsland.exe`

Play Mode HUD 테스트에는 실제 Game View가 필요하므로 `-batchmode`를 사용하지 않는다. 최초 배치 실행에서 HUD 테스트가 실패한 원인은 OnGUI가 호출되지 않았기 때문이며, 렌더링 검증을 생략하지 않고 실행 방식을 수정했다.
