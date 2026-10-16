using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace BreathIsland.Tests
{
    // Offline rendering only. Never captures the desktop or touches player saves.
    internal sealed class PortfolioMovieWriter : IDisposable
    {
        private readonly Camera camera;
        private readonly RenderTexture target;
        private readonly RenderTexture originalTarget;
        private readonly Texture2D pixels;
        private readonly Process encoder;
        private readonly string folder;
        public PortfolioMovieWriter(Camera camera, string folder, string name)
        {
            this.camera = camera; this.folder = folder;
            Directory.CreateDirectory(folder);
            target = new RenderTexture(1280, 720, 24);
            originalTarget = camera.targetTexture;
            camera.targetTexture = target;
            pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            encoder = Process.Start(new ProcessStartInfo {
                FileName = Path.GetFullPath("Builds/Portfolio/Tools/imageio_ffmpeg/binaries/ffmpeg-win-x86_64-v7.1.exe"),
                Arguments = "-hide_banner -loglevel error -y -f rawvideo -pixel_format rgb24 -video_size 1280x720 -framerate 60 -i pipe:0 -vf vflip -an -c:v libx264 -preset fast -crf 18 -pix_fmt yuv420p \"" + Path.GetFullPath(Path.Combine(folder, name + ".mp4")) + "\"",
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true
            });
        }
        public void Frame()
        {
            var prior = RenderTexture.active; var oldTarget = camera.targetTexture;
            try {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); pixels.Apply();
                byte[] bytes = pixels.GetRawTextureData();
                encoder.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
            }
            finally { camera.targetTexture = oldTarget; RenderTexture.active = prior; }
        }
        public void Still(string name)
        {
            var prior = RenderTexture.active; var oldTarget = camera.targetTexture;
            var large = new RenderTexture(1920, 1080, 24);
            var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            try {
                camera.targetTexture = large; Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = large;
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG());
            }
            finally { camera.targetTexture = oldTarget; RenderTexture.active = prior; large.Release(); UnityEngine.Object.Destroy(large); UnityEngine.Object.Destroy(image); }
        }
        public void Dispose()
        {
            encoder.StandardInput.Close();
            if (!encoder.WaitForExit(30000)) { encoder.Kill(); throw new IOException("Portfolio encoder timed out"); }
            int code = encoder.ExitCode; encoder.Dispose();
            camera.targetTexture = originalTarget;
            target.Release(); UnityEngine.Object.Destroy(target); UnityEngine.Object.Destroy(pixels);
            if (code != 0) throw new IOException("Portfolio encoder exit " + code);
        }
    }
}
