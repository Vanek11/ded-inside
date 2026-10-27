using System.Collections.Generic;
using UnityEngine;

namespace DI
{
    /// Процедурные звуки: генерируются кодом при первом запросе, без ассетов.
    /// Скрип, стук, всплеск, срыв крышки, гусь, корова, касса.
    public static class Sfx
    {
        const int Rate = 44100;
        static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

        public static void PlayAt(string id, Vector3? pos = null)
        {
            var clip = Get(id);
            if (clip == null) return;
            var go = new GameObject($"sfx_{id}");
            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            if (pos.HasValue)
            {
                go.transform.position = pos.Value;
                src.spatialBlend = 1f;
                src.minDistance = 3f;
                src.maxDistance = 45f;
            }
            src.Play();
            Object.Destroy(go, clip.length + 0.2f);
        }

        static AudioClip Get(string id)
        {
            if (cache.TryGetValue(id, out var c) && c != null) return c;
            c = Build(id);
            if (c != null) cache[id] = c;
            return c;
        }

        static AudioClip Gen(string name, float dur, System.Func<float, float> gen)
        {
            int n = (int)(dur * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(gen(i / (float)Rate), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Build(string id)
        {
            switch (id)
            {
                case "creak": // скрип дерева под нагрузкой
                    return Gen("creak", 0.7f, t =>
                    {
                        float f = 95f + 25f * Mathf.Sin(2f * Mathf.PI * 9f * t);
                        float saw = Mathf.Repeat(f * t, 1f) * 2f - 1f;
                        return saw * 0.3f * Mathf.Sin(Mathf.PI * t / 0.7f)
                               * (0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 31f * t));
                    });
                case "thump": // глухой стук тела
                    return Gen("thump", 0.25f, t =>
                        Mathf.Sin(2f * Mathf.PI * 72f * t) * Mathf.Exp(-20f * t) * 0.9f);
                case "splash": // всплеск
                    return Gen("splash", 0.5f, t =>
                        Mathf.Sin(t * 917.3f) * Mathf.Sin(t * 1319.7f) * Mathf.Exp(-7f * t) * 0.5f);
                case "snap": // треск сорванных петель
                    return Gen("snap", 0.15f, t =>
                        Mathf.Sin(t * 777.7f) * Mathf.Sin(t * 2111.1f) * Mathf.Exp(-25f * t) * 0.8f);
                case "goose": // «га-га!»
                    return Gen("goose", 0.5f, t =>
                    {
                        float f = t < 0.25f ? 520f : 430f;
                        float tt = t < 0.25f ? t : t - 0.25f;
                        float sq = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * f * tt));
                        float env = Mathf.Exp(-10f * tt) * (tt < 0.22f ? 1f : 0f);
                        return sq * env * 0.4f;
                    });
                case "moo": // «Мууу»
                    return Gen("moo", 0.9f, t =>
                    {
                        float f = 115f - 25f * t + 6f * Mathf.Sin(2f * Mathf.PI * 5f * t);
                        float saw = Mathf.Repeat(f * t, 1f) * 2f - 1f;
                        return saw * 0.35f * Mathf.Clamp01(4f * t) * Mathf.Clamp01(4f * (0.9f - t));
                    });
                case "cash": // звенящая касса
                    return Gen("cash", 0.6f, t =>
                    {
                        float a = Mathf.Sin(2f * Mathf.PI * 880f * t) * Mathf.Exp(-6f * t);
                        float b = t > 0.15f
                            ? Mathf.Sin(2f * Mathf.PI * 1318f * t) * Mathf.Exp(-5f * (t - 0.15f)) * 0.8f
                            : 0f;
                        return (a + b) * 0.5f;
                    });
                default:
                    return null;
            }
        }
    }
}
