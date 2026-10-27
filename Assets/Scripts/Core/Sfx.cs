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

        /// Меню-полечка: 2 такта ля-минор, квадрат-мелодия, треугольный бас, аккорды.
        /// Зацикленный клип, генерируется в рантайме.
        public static AudioClip Polka()
        {
            if (cache.TryGetValue("polka", out var cached) && cached != null) return cached;

            float bpm = 132f, beat = 60f / bpm;
            float dur = beat * 8f; // 2 такта 4/4
            int n = (int)(dur * Rate);
            var data = new float[n];

            void Note(float startBeat, float lenBeats, float freq, float vol, int wave)
            {
                int s = (int)(startBeat * beat * Rate);
                int e = (int)((startBeat + lenBeats) * beat * Rate);
                for (int i = s; i < Mathf.Min(e, n); i++)
                {
                    float t = (i - s) / (float)Rate;
                    float env = Mathf.Min(1f, t * 250f) * Mathf.Exp(-3f * t / lenBeats);
                    float w = wave == 0
                        ? Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t))
                        : Mathf.PingPong(freq * t, 1f) * 2f - 1f;
                    data[i] += w * env * vol;
                }
            }

            float E5 = 659.26f, D5 = 587.33f, C5 = 523.25f, B4 = 493.88f, A4 = 440f;
            float A2 = 110f, E3 = 164.81f;

            Note(0f, .45f, E5, .16f, 0); Note(.5f, .45f, E5, .16f, 0);
            Note(1f, .45f, D5, .16f, 0); Note(1.5f, .45f, C5, .16f, 0);
            Note(2f, .45f, D5, .16f, 0); Note(2.5f, .45f, C5, .16f, 0);
            Note(3f, .9f, B4, .16f, 0);
            Note(4f, .45f, C5, .16f, 0); Note(4.5f, .45f, C5, .16f, 0);
            Note(5f, .45f, B4, .16f, 0); Note(5.5f, .45f, A4, .16f, 0);
            Note(6f, .45f, B4, .16f, 0); Note(6.5f, .45f, A4, .16f, 0);
            Note(7f, .95f, A4, .15f, 0);

            Note(0f, .4f, A2, .26f, 1); Note(2f, .4f, A2, .26f, 1);
            Note(4f, .4f, A2, .26f, 1); Note(6f, .4f, A2, .26f, 1);
            Note(1f, .35f, E3, .2f, 1); Note(3f, .35f, E3, .2f, 1);
            Note(5f, .35f, E3, .2f, 1); Note(7f, .35f, E3, .2f, 1);

            for (float b = .5f; b < 7.9f; b += 1f)
                foreach (float f in new[] { 220f, 261.63f, 329.63f })
                    Note(b, .16f, f, .045f, 0);

            var clip = AudioClip.Create("polka", n, 1, Rate, false);
            clip.SetData(data, 0);
            cache["polka"] = clip;
            return clip;
        }
    }
}
