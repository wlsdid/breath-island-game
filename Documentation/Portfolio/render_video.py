"""Render the disclosed indoor demo from two real Windows recordings.

Usage: python render_video.py <ffmpeg.exe> <home.mp4> <restore.mp4> <output.mp4>
No game process, save data, or source recording is modified.
"""
import pathlib
import subprocess
import sys
import tempfile

ROOT = pathlib.Path(__file__).resolve().parents[2]
FONT = "Assets/BreathIsland/Resources/UI/Fonts/GowunDodum-Regular.ttf"
TITLE = "Assets/BreathIsland/Resources/UI/Fonts/Gaegu-Bold.ttf"


def main():
    ffmpeg, home, restore, output = map(lambda p: str(pathlib.Path(p).resolve()), sys.argv[1:])
    pathlib.Path(output).parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="video-", dir=ROOT / "Builds/Portfolio") as tmp:
        folder = pathlib.Path(tmp)
        count = 0

        def text(label, y, size=25, font=FONT, enable=None):
            nonlocal count
            count += 1
            path = folder / f"text-{count}.txt"
            path.write_text(label, encoding="utf-8")
            relative = path.relative_to(ROOT).as_posix()
            result = f"drawtext=fontfile='{font}':textfile='{relative}':fontcolor=0xFAF2DC:fontsize={size}:x=(w-tw)/2:y={y}"
            return result + (f":enable='{enable}'" if enable else "")

        def run(args):
            subprocess.run([ffmpeg, "-hide_banner", "-loglevel", "error", "-y", *args], cwd=ROOT, check=True)

        def encode(args, filters, name):
            path = folder / f"{name}.mp4"
            run([*args, "-an", "-vf", filters, "-r", "30", "-c:v", "libx264", "-preset", "medium", "-crf", "21", "-pix_fmt", "yuv420p", str(path)])
            return path

        def card(name, seconds, lines):
            return encode(["-f", "lavfi", "-i", "color=c=0x183F43:s=1280x720:r=30", "-t", str(seconds)],
                          ",".join(text(line, y, size, font) for line, y, size, font in lines), name)

        parts = [card("intro", 12, [
            ("숨결섬 · 실내 생활 시연", 170, 60, TITLE),
            ("가방 → 제작 → 배치·회전 → 집 편집 → 저장 복원", 290, 28, FONT),
            ("본인 역할: 기획 · 디자인 판단 · 직접 플레이 검수", 370, 26, FONT),
            ("AI 지원: 코드 구현 · 자동 검증 · 문서·영상 정리", 415, 26, FONT),
            ("Windows 실제 실행 / 별도 시연 저장 / 대기 구간 생략", 500, 25, FONT),
        ])]
        framing = "scale=1152:648,pad=1280:720:64:0:color=0x183F43,setsar=1"
        labels = [
            (0, 30, "가방에서 망치로 의자 제작 · 나무 30 → 27 / 의자 1 → 2"),
            (30, 80, "가구 선택 → 배치 미리보기 → 90도 회전 → 확정"),
            (80, 125, "집 편집 · 가구와 겹치는 벽은 차단 / 빈 위치에서 미리보기"),
            (125, 160, "미리보기 취소 · 저장 상태와 재료 수량 유지"),
            (160, 190, "내부 벽 적용 · 나무 1개 소비 / 확정 상태 저장"),
        ]
        filters = [framing] + [text(label, 675, 23, enable=f"gte(t,{start})*lt(t,{end})") for start, end, label in labels]
        parts.append(encode(["-ss", "80", "-i", home, "-t", "190"], ",".join(filters), "home"))
        parts.append(card("transition", 5, [
            ("재실행 후 저장 복원", 270, 54, TITLE),
            ("별도로 촬영한 구간 · 연속 플레이가 아닙니다", 370, 27, FONT),
        ]))
        parts.append(encode(["-i", restore, "-t", "19"], framing + "," + text("재실행된 실내 · 슬롯 1 불러오기 → 이동 / 의자와 벽 유지", 675, 23), "restore"))
        parts.append(card("outro", 10, [
            ("검증 범위와 남은 한계", 180, 54, TITLE),
            ("제작 · 회전 배치 · 집 편집 취소/적용 · 저장 복원", 290, 28, FONT),
            ("Edit Mode 98/98 · Play Mode 32/32 + 후속 회귀 11/11", 360, 25, FONT),
            ("Windows Mono 빌드 성공 · 실물 게임패드 미검증", 405, 25, FONT),
            ("바다·채집 영상은 이번 편집에서 제외 · 자세한 근거는 VALIDATION.md", 500, 24, FONT),
        ]))
        concat = folder / "concat.txt"
        concat.write_text("\n".join(f"file '{p.as_posix()}'" for p in parts), encoding="utf-8")
        run(["-f", "concat", "-safe", "0", "-i", str(concat), "-c", "copy", "-movflags", "+faststart", output])


if __name__ == "__main__":
    main()
