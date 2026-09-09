using UnityEngine;

namespace BreathIsland
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class ProceduralSeaAudio : MonoBehaviour
    {
        private void Awake()
        {
            var source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = .12f;
            source.spatialBlend = 0f;
            source.clip = CreateSeaLoop();
            source.Play();
        }

        private static AudioClip CreateSeaLoop()
        {
            const int sampleRate = 22050;
            const int seconds = 6;
            float[] samples = new float[sampleRate * seconds];
            var random = new System.Random(4815);
            float smoothedNoise = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float time = i / (float)sampleRate;
                float white = (float)(random.NextDouble() * 2.0 - 1.0);
                smoothedNoise = Mathf.Lerp(smoothedNoise, white, .008f);
                float swell = .55f + .45f * Mathf.Sin(time * Mathf.PI * .42f);
                samples[i] = smoothedNoise * swell * .42f + Mathf.Sin(time * Mathf.PI * 2f * 83f) * .006f;
            }
            var clip = AudioClip.Create("Procedural Sea Ambience", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

