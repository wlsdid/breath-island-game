"""Encode new animated Editor renders; does not claim Windows gameplay footage."""
import hashlib
import json
import pathlib
import shutil
import subprocess

ROOT = pathlib.Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Builds/Portfolio/FinalCapture-20261016'
DEST = ROOT / 'Documentation/Portfolio/VisualUpdate-20261016/FinalCapture'
FFMPEG = ROOT / 'Builds/Portfolio/Tools/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe'
FONT = 'Assets/BreathIsland/Resources/UI/Fonts/GowunDodum-Regular.ttf'
SHOTS = [
    ('01-village', '마을 · 지면과 건물, 해안으로 이어지는 풍경', 7),
    ('02-coast', '바다 · 움직이는 수면과 빛, 해안 표현', 6),
    ('04-walk-jump', '섬 산책 · 걷기와 점프, 착지', 8),
    ('03-fishing', '낚시 · 캐스팅 → 입질 → 챔질 → 감기', 11),
    ('sword_special', '검 · 기사 외형과 특수 무기 스킬', 7),
    ('bow_special', '활 · 궁수 외형, 당기기와 발사, 특수 스킬', 7),
    ('staff_special', '지팡이 · 마법사 외형과 특수 무기 스킬', 7),
    ('05-chair', '실내 생활 · 의자로 걸어가기, 앉기와 일어나기', 8),
    ('06-storage', '보관 상자 · 나무 5개 넣기, 2개 꺼내기', 8),
    ('07-rest', '침대 · 다음 날까지 쉬기', 6),
]


def run(args):
    return subprocess.run([str(FFMPEG), '-hide_banner', '-loglevel', 'error', '-y', *args], cwd=ROOT, check=True)


def main():
    DEST.mkdir(parents=True, exist_ok=True)
    work = SOURCE / 'edit'
    work.mkdir(exist_ok=True)
    parts = []
    for name, caption, seconds in SHOTS:
        label = work / (name + '.txt')
        label.write_text(caption, encoding='utf-8')
        output = work / (name + '.mp4')
        filters = (
            "drawbox=x=0:y=650:w=iw:h=70:color=0x183F43@0.94:t=fill:enable='lt(t,2)',"
            f"drawtext=fontfile='{FONT}':textfile='{label.relative_to(ROOT).as_posix()}':fontcolor=0xFAF2DC:fontsize=27:x=28:y=672:enable='lt(t,2)',"
            f"fade=t=in:st=0:d=0.18,fade=t=out:st={seconds-0.18}:d=0.18"
        )
        run(['-i', str(SOURCE / (name + '.mp4')), '-vf', filters, '-an', '-c:v', 'libx264', '-preset', 'medium', '-crf', '19', '-pix_fmt', 'yuv420p', str(output)])
        parts.append(output)
    listing = work / 'concat.txt'
    listing.write_text(''.join("file '" + p.as_posix() + "'\n" for p in parts), encoding='utf-8')
    movie = DEST / 'breath-island-portfolio-final.mp4'
    run(['-f', 'concat', '-safe', '0', '-i', str(listing), '-c', 'copy', '-movflags', '+faststart', str(movie)])
    run(['-i', str(movie), '-f', 'null', '-'])
    frame_report = subprocess.run([str(FFMPEG), '-hide_banner', '-loglevel', 'error', '-i', str(movie), '-vf', 'fps=2', '-f', 'framemd5', '-'], check=True, capture_output=True, text=True).stdout
    hashes = [line.split(',')[-1].strip() for line in frame_report.splitlines() if line and not line.startswith('#')]
    assert len(hashes) == sum(x[2] for x in SHOTS)*2, 'Unexpected duration'
    # Menus may remain still intentionally; uniqueness alone cannot test smoothness.
    (DEST / 'motion-check.txt').write_text('Full video decode: passed\nSampled every 0.5 seconds: ' + str(len(hashes)) + '\nDistinct sampled frames: ' + str(len(set(hashes))) + '\nThis checks frame changes, not gameplay correctness.\n', encoding='utf-8')
    life_listing = work / 'life-concat.txt'
    life_parts = [p for p in parts if p.stem in ('04-walk-jump','05-chair','06-storage','07-rest')]
    life_listing.write_text(''.join("file '" + p.as_posix() + "'\n" for p in life_parts), encoding='utf-8')
    run(['-f','concat','-safe','0','-i',str(life_listing),'-c','copy','-movflags','+faststart',str(DEST / 'breath-island-life.mp4')])
    for source in sorted(SOURCE.glob('*.png')):
        shutil.copy2(source, DEST / source.name)
    clips = DEST / 'Clips'
    clips.mkdir(exist_ok=True)
    for name, _, _ in SHOTS:
        shutil.copy2(SOURCE / (name + '.mp4'), clips / (name + '.mp4'))
    manifest = {
        'date': '2026-10-16', 'game_commit': 'ec53ea90d303f1f75a79000e850f6233420d86b2',
        'capture': 'Unity 6000.5.4f1 Editor Play Mode; scripted camera and commands; real animated runtime rendering',
        'manual_windows_gameplay': False, 'audio': False,
        'video': {'file': movie.name, 'width': 1280, 'height': 720, 'fps': 60, 'seconds': sum(x[2] for x in SHOTS)},
        'stills': {'width': 1920, 'height': 1080, 'postprocessing': 'none'},
        'fixture': 'Prepared village seed; temporary save root; unlocked special weapons; fixed-step 60fps; no player saves',
        'shots': [{'file': name, 'caption': caption, 'seconds': seconds} for name, caption, seconds in SHOTS],
        'files': [{'file': p.relative_to(DEST).as_posix(), 'bytes': p.stat().st_size, 'sha256': hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(DEST.rglob('*')) if p.suffix in ('.mp4', '.png')]
    }
    (DEST / 'capture-manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(manifest['video']))


if __name__ == '__main__':
    main()
