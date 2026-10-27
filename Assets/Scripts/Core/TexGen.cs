using System.Collections.Generic;
using UnityEngine;

namespace DI
{
    /// Процедурные текстуры: бесшовный value-noise и древесные полосы.
    /// Генерируются один раз, кэшируются по ключу.
    public static class TexGen
    {
        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        public static Texture2D Get(string key) => cache.TryGetValue(key, out var t) ? t : null;

        /// Бесшовный шум двух цветов: a — впадины, b — вершины.
        public static Texture2D Noise(string key, Color a, Color b, float freq, int octaves)
        {
            if (cache.TryGetValue(key, out var t)) return t;
            const int size = 128;
            t = new Texture2D(size, size, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Repeat };

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    float n = 0f, amp = 0.5f, f = freq;
                    for (int o = 0; o < octaves; o++)
                    {
                        n += VNoise(u * f, v * f, key.GetHashCode() + o * 131) * amp;
                        amp *= 0.5f;
                        f *= 2f;
                    }
                    pixels[y * size + x] = Color.Lerp(a, b, Mathf.Clamp01(n));
                }
            t.SetPixels(pixels);
            t.Apply();
            cache[key] = t;
            return t;
        }

        /// Древесные полосы вдоль оси U.
        public static Texture2D Wood(string key, Color a, Color b)
        {
            if (cache.TryGetValue(key, out var t)) return t;
            const int size = 128;
            t = new Texture2D(size, size, TextureFormat.RGB24, false) { wrapMode = TextureWrapMode.Repeat };

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size, v = y / (float)size;
                    float warp = VNoise(u * 3f, v * 3f, key.GetHashCode()) * 0.8f;
                    float stripe = Mathf.Abs(Mathf.Repeat((u * 7f + warp) * 2f, 2f) - 1f);
                    float grain = VNoise(u * 24f, v * 24f, key.GetHashCode() + 77) * 0.25f;
                    pixels[y * size + x] = Color.Lerp(a, b, Mathf.Clamp01(stripe * 0.8f + grain));
                }
            t.SetPixels(pixels);
            t.Apply();
            cache[key] = t;
            return t;
        }

        static float VNoise(float u, float v, int seed)
        {
            int ix = Mathf.FloorToInt(u), iy = Mathf.FloorToInt(v);
            float fu = Mathf.SmoothStep(0f, 1f, Mathf.Repeat(u, 1f));
            float fv = Mathf.SmoothStep(0f, 1f, Mathf.Repeat(v, 1f));
            float a = Hash(ix, iy, seed), b = Hash(ix + 1, iy, seed);
            float c = Hash(ix, iy + 1, seed), d = Hash(ix + 1, iy + 1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, fu), Mathf.Lerp(c, d, fu), fv);
        }

        static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 974711;
                h = (h ^ (h >> 13)) * 1274126177;
                h = h ^ (h >> 16);
                return (h & 0x7fffffff) / (float)int.MaxValue;
            }
        }
    }
}
