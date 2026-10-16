# 개선 내용과 근거

## A. 마을·주민·지면

- 문제/요구: NPC와 마을 자체의 품질, 초록 바닥에 박힌 듯한 풀, 집의 구멍·깜빡임, 사람과 집의 비율.
- 변경: NPC 캐릭터·텍스처·대기 동작·시선 반응, 창틀·화단·벤치 등 장식, 지면 셰이더와 제공 잔디 모델, 닫힌 지붕 메시, 캐릭터 시각 크기 조정.
- 구조: NpcPresentation은 표시 방향만 조절하며 상호작용 앵커·대화·호감도를 바꾸지 않는다. 마을 장식은 새 시설 기능이 아니다.
- 근거: d2aec1d, 5dc5dfd, 174771c. Runtime/Presentation/NpcPresentation.cs, GroundCover.cs; Editor/HouseShellAuthoring.cs. 파일 기준 경로는 Assets/BreathIsland/ 아래다.
- 확인: 당시 테스트·Editor 렌더 기록. 모든 각도의 집 깜빡임·관통이 해소됐다고 주장하지 않는다. NPC 일정·새 대사는 추가하지 않았다.

## B. 바다와 낚시의 시각 반응

- 문제/요구: 파란 바닥 같은 바다, 수면에 반응하는 찌, 물속 물고기, 낚싯대의 역동적인 투척·휨·감기와 물고기 도약.
- 변경: 파도 높이/법선, 굴절·수중 표시, 파도에 맞춘 찌 높이와 입질 잠김, 어종별 재질·꼬리 동작, 물보라·파문, 낚싯대/릴/줄/손 표시.
- 구조: OceanSurface.Height와 셰이더가 같은 파도 식을 사용한다. FishingPresentation은 낚시 상태를 읽어 표현한다. 수면은 물리 유체 시뮬레이션이 아니다. 장식 물고기는 생태계 AI나 새 어획 규칙이 아니다.
- 근거: 572fe2b, b6a4884, 8b3f9ed, 206db3f, 5dc5dfd, 36178e1. Code/OceanSurface.cs, OceanWater.shader, FishingPresentation.cs, FishingRodView.cs.
- 규칙 구분: e3b4aaf에서 입질 후 3초 내 챔질 규칙을 추가했다. 이 변경까지 포함해 '모든 게임 규칙을 그대로 유지했다'고 쓰면 틀린다. 이후 수면/물고기 표시 변경에서는 확률·보상·장력 수치를 유지했다. 낚시 중 부두 이동과 시점 조작도 별도 승인된 조작 변경이다.
- 직접 확인: 사용자 '낚시 자체는 정상', '손은 보이지 않음 나머지는 정상' 기록은 당시 빌드에 한정한다. 후속 손 개선에는 재차 품질 지적이 있었다. 최종 손/시점/캐스팅의 자연스러움까지 사용자 승인됐다고 쓰지 않는다.

## C. 걷기·도구·활의 읽히는 동작

- 문제/요구: 키를 누르고 이동해도 동작이 멈춤, 떠 보임, 도구 몸통 겹침, 칼 날 방향, 활의 조준·당김·전방 발사.
- 변경: 걷기/대기/공중 대기 반복 클립 명시, 시각 높이와 접촉 여백 보정, 손 본 크기를 상쇄한 장비 크기, 바깥쪽 도구 운반 자세, 직업별 공격 자세, 활 줄·화살의 당김과 발사.
- 근거: 174771c, 36178e1, 5222f51. CharacterMotionAuthoring, CharacterPresentation, EquipmentView, ToolSwing, WeaponActionView, Code/BowShotView.cs.
- 확인: 관련 자동 검사와 Editor 렌더. 외형 변화와 입력 변화의 전체 최신 Windows 검수, 모든 각도의 관통 및 주관적 자연스러움은 별도다. 과거 정지 화면 한 장으로 연속 걷기 개선을 증명하지 않는다.

## D. 직업 외형과 특수 무기의 구분

- 문제/요구: 장착 무기와 캐릭터 모습이 연결되지 않음. 최초 활 외형은 기본 Ranger와 동일해 사용자가 다시 지적함.
- 변경: 검=Knight, 지팡이=Mage, 활=Rogue_Hooded. 일반/특수 무기는 직업 외형을 공유한다. 도구를 들면 장착 직업을 유지하고 해제하면 기본 Ranger로 돌아간다.
- 구조: JobAppearanceView는 기존 뼈에 스킨만 연결해 Animator·손 소켓·앉기 참조를 유지한다. 손 장비 아래 스킨을 외형 원본 목록에서 제외한다. 저장 형식 추가 없음.
- 근거: 5222f51, ec53ea9. Code/JobAppearanceView.cs, TrainingPresentationAuthoring.cs, IslandLifeLoopTests.cs.
- 확인: 최종 전체 테스트 115/47, Mono 성공. 사용자 '확인 완료'는 직전 궁수 외형 결과를 확인한 것으로 기록한다. 자세한 조작 절차가 제시되지 않았으므로 해제·도구·재실행 각각의 수동 검증으로 세분화하지 않는다.
- 특수 무기: 36178e1에서 별도 무기 모델을 선별, a0ddc79에서 검 타격 불꽃·활 착탄 별빛·지팡이 전기 고리를 연결. Code/WeaponSkillView.cs, SkillFlipbook.cs. CC0 팩 원화를 직접 제작한 것처럼 쓰지 않는다.
- 미적용: Godot용 tornado 팩은 Unity에 이식하지 않았다. '토네이도 스킬 구현'이라고 표기하지 않는다.

## E. 훈련 피드백

- 임시 직육면체를 프로젝트에서 작성한 허수아비 프리팹으로 교체했다.
- 마지막 유효 피격 뒤 탐험 업데이트 시간 3초가 지나면 체력 100으로 복구한다. 추가 피격 시 다시 3초, 체력 0에서도 회복. 일시정지 중에는 탐험 업데이트가 멈춘다.
- 근거: 5222f51, Code/TrainingCombatSystem.cs. 새 적 AI·전투 콘텐츠 확장으로 소개하지 않는다.

## 검증 자료의 해석

Evidence/editmode-results.xml, playmode-results.xml은 2026-10-15 최종 실행의 사본이다. evidence.json에는 이 자료 조사 시점의 SHA256과 수량을 기록한다. 자동 테스트 162개는 프로젝트 전체 테스트 수이며 이번 비주얼 기능만의 신규 테스트 수가 아니다. 이후 최종 자료 촬영을 위해 Editor Play Mode 촬영 테스트 2개를 실행했고, 실내 촬영 보완 후 해당 테스트 1개를 재실행했다. 모두 통과했다. `06_FINAL_DELIVERY.md`, `Evidence/final-capture-results.xml`, `Evidence/final-life-results.xml`을 확인한다. 전체 회귀 테스트와 Windows 빌드는 이번 촬영 작업에서 다시 수행하지 않았다.

Evidence/build-success.txt는 기존 빌드 로그의 성공 부분 발췌다. 원본 전체 로그·실행 파일의 해시는 evidence.json에 있다. 코드 사본은 ec53ea9 기준이고 게임 실행용 묶음이 아니다.

최근 전체 검증은 본문에 짧게 쓰고, 실패→수정 과정은 설명이 필요할 때만 사용한다. 원본 기록은 Documentation/VALIDATION.md에 남아 있다. 전체 화면 비율, 다른 GPU, 장시간 성능, 실물 패드는 미검증으로 남긴다.
