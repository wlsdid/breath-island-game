# 작업 규칙

- 코드는 기능별 폴더와 클래스로 분리한다. 대형 단일 파일을 만들지 않는다.
- `Assets/BreathIsland/Runtime/BreathIslandGame.cs`는 Composition Root와 프레임 라우팅만 담당한다.
- 게임 규칙은 해당 기능 시스템에, 화면 코드는 `Runtime/UI`에, 공유 상태 연결은 `Runtime/Core`에 둔다.
- 저장 형식, 경제 밸런스, 콘텐츠 범위, 플랫폼 동작, 기존 구조 변경처럼 구현 결과를 바꾸는 요청이 불명확하면 작업 전에 사용자에게 질문한다.
- 기존 동작을 변경하는 리팩터링은 관련 Edit Mode 테스트와 Windows 검증 빌드로 확인한다.
- 자세한 경계는 `Documentation/ARCHITECTURE.md`를 따른다.
