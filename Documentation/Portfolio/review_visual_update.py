"""Decode every frame and prepare review sheets; no gameplay correctness claims."""
import json
import subprocess
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw
from render_visual_update import ROOT, DEST, FFMPEG, SHOTS

work = ROOT / 'Builds/Portfolio/FinalReview'
work.mkdir(parents=True, exist_ok=True)
report = {'checks': [], 'scope': 'Every frame decoded; all scenes reviewed at 1-second intervals and action sequences at finer intervals. This is presentation review, not manual Windows gameplay.'}
for name, _, seconds in SHOTS:
    clip = DEST / 'Clips' / (name + '.mp4')
    raw = subprocess.run([str(FFMPEG), '-v', 'error', '-i', str(clip), '-vf', 'scale=96:54,format=gray', '-f', 'rawvideo', '-'], check=True, capture_output=True).stdout
    frames = np.frombuffer(raw, np.uint8).reshape(-1, 54, 96)
    assert len(frames) == seconds * 60, (name, len(frames))
    deltas = np.abs(frames[1:].astype(np.float32) - frames[:-1]).mean(axis=(1, 2))
    top = np.argsort(deltas)[-5:][::-1]
    report['checks'].append({'clip': name, 'frames': len(frames), 'expectedFrames': seconds * 60,
                             'maxMeanPixelDelta': float(deltas.max()),
                             'largestChangesSeconds': [round((int(i)+1)/60, 3) for i in top]})

movie = DEST / 'breath-island-portfolio-final.mp4'
raw = subprocess.run([str(FFMPEG), '-v', 'error', '-i', str(movie), '-vf', 'fps=1,scale=320:180', '-f', 'rawvideo', '-pix_fmt', 'rgb24', '-'], check=True, capture_output=True).stdout
frames = np.frombuffer(raw, np.uint8).reshape(-1, 180, 320, 3)
assert len(frames) == 75
for page in range(3):
    sheet = Image.new('RGB', (1600, 1000), '#183f43'); draw = ImageDraw.Draw(sheet)
    for i, frame in enumerate(frames[page*25:(page+1)*25]):
        x, y = (i % 5)*320, (i//5)*200
        sheet.paste(Image.fromarray(frame), (x, y)); draw.text((x+8,y+182), f'{page*25+i:02d}s', fill='white')
    sheet.save(work / f'overview-{page+1}.jpg')
(DEST / 'review.json').write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps(report, indent=2))
