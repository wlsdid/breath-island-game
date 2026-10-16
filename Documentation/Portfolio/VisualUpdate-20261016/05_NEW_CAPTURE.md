# 최신 비주얼 촬영 자료

게임 코드 기준: `ec53ea90d303f1f75a79000e850f6233420d86b2`. 촬영일: 2026-10-16.

## 촬영 방식

`LatestCapture/`는 이번에 새로 촬영한 자료다. Unity 6000.5.4f1 Editor Play Mode에서 실제 런타임 모델·애니메이션·수면·스킬 표현을 렌더링했다. 정지 화면을 이어 붙인 슬라이드 영상이 아니다. 카메라와 공격·낚시 명령을 촬영용 코드로 제어했으며 **Windows 직접 조작 영상은 아니다**. 영상 상단에도 이 구분을 표시한다.

준비된 마을 시연 데이터와 임시 저장 경로를 사용했다. 특수 무기는 촬영 데이터에서 해금했다. 고정 30fps로 렌더링하며 일반 게임 지급량·해금 조건·경제·저장 버전은 변경하지 않았다. UI를 제외한 표현 중심 촬영이므로 메뉴 조작이나 전체 플레이 흐름의 검증 근거로 사용하지 않는다.

## Claude에서 사용할 자료

- `LatestCapture/breath-island-visual-update.mp4`: 45초, 1280×720, 30fps, 무음. 한국어 장면 설명과 촬영 방식 표기 포함.
- `LatestCapture/*.png`: 1920×1080 원본 렌더. 생성형 보정·합성·색 보정 없음.
- `LatestCapture/Clips/`: 자막 없는 원본 구간 영상 6개. 재편집 시에도 Editor 자동 연출임을 표기한다.
- `LatestCapture/capture-manifest.json`: 파일 해시, 해상도, 촬영 설정과 구간 정보.
- `Evidence/latest-capture-results.xml`: 이번 촬영 전용 테스트 결과. 기존 전체 회귀 테스트 결과와 별개다.

| 구간 | 내용 | 정지 이미지 |
|---|---|---|
| 00:00–00:07 | 마을의 건물·길·녹지와 해안 연결 | 01-village.png |
| 00:07–00:13 | 움직이는 수면과 해안 | 02-coast.png |
| 00:13–00:24 | 낚싯대 캐스팅·입질·챔질·감기 | 03-cast.png, 04-fishing.png |
| 00:24–00:31 | 기사 외형·검 공격·특수 검 스킬 | sword_special-outfit.png, sword_special-skill.png |
| 00:31–00:38 | 궁수 외형·활 당기기·발사·특수 활 스킬 | bow_special-outfit.png, bow_special-skill.png |
| 00:38–00:45 | 마법사 외형·지팡이 공격·특수 지팡이 스킬 | staff_special-outfit.png, staff_special-skill.png |

## 편집 기준

현재 비주얼을 소개할 때는 이 폴더의 자료를 우선 사용한다. 기존 `Images/` 17장은 과거 단계와 비교하는 개발 기록으로 유지한다. 카메라·조명·해상도가 다른 이미지를 같은 조건의 전후 비교라고 설명하지 않는다.

기존 3분 56초 실내 영상은 Windows 직접 플레이 기록으로 보존한다. 새 영상은 그 영상의 최신판이나 연속 플레이 후반부가 아니라 **별도의 비주얼 소개 영상**이다. 낚시 성공·판매·저장 복원까지 수행한 영상으로 설명하지 않는다. 이번 촬영은 화면 비율별 UI, 입력 전달, 전체 핵심 루프의 새 검증을 대신하지 않는다.

원본 구간 MP4는 `Builds/Portfolio/LatestCapture-20261016/`에 남긴다. 재촬영은 `PortfolioMovieCapture.CaptureLatestVisuals`를 명시적으로 선택하고, 편집은 `Documentation/Portfolio/render_visual_update.py`로 재현한다. 촬영 테스트는 Explicit로 표시되어 일반 전체 테스트에서 자동 실행되지 않는다.

파일 검사는 전체 디코딩과 0.5초 간격 프레임 비교로 수행한다. 검사 결과는 `LatestCapture/motion-check.txt`에 기록한다. 이 검사는 영상 손상·정지 여부에 관한 것이며 게임 기능 검증을 뜻하지 않는다.
