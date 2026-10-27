using UnityEngine;

namespace DI
{
    /// Материалы и постэффекты: покраска с процедурными текстурами,
    /// прозрачность, свечение, и настройка PostProcessing на камере.
    public static class Art
    {
        /// Покрасить рендерер: цвет + опциональная процедурная текстура + глянец/металл.
        public static void Paint(Renderer r, Color c, float gloss = 0.1f, float metallic = 0f,
            string tex = null, float texScale = 1.5f)
        {
            var m = r.material;
            m.color = c;
            m.SetFloat("_Glossiness", gloss);
            m.SetFloat("_Metallic", metallic);
            if (tex != null)
            {
                var t = Resolve(tex, c);
                if (t != null)
                {
                    m.mainTexture = t;
                    m.mainTextureScale = Vector2.one * texScale;
                }
            }
        }

        static Texture2D Resolve(string tex, Color c)
        {
            switch (tex)
            {
                case "grass": return TexGen.Noise("grass", new Color(0.36f, 0.47f, 0.25f), new Color(0.5f, 0.62f, 0.36f), 9f, 3);
                case "dirt": return TexGen.Noise("dirt", new Color(0.38f, 0.31f, 0.23f), new Color(0.52f, 0.44f, 0.34f), 11f, 3);
                case "stone": return TexGen.Noise("stone", new Color(0.5f, 0.5f, 0.52f), new Color(0.63f, 0.63f, 0.65f), 8f, 3);
                case "wood": return TexGen.Wood("wood", new Color(0.45f, 0.32f, 0.2f), new Color(0.6f, 0.45f, 0.29f));
                default: return null;
            }
        }

        /// Перевести материал в прозрачный режим (Standard).
        public static void Transparent(Renderer r, Color c, float gloss = 0.8f)
        {
            var m = r.material;
            m.SetFloat("_Mode", 3f);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_ALPHABLEND_ON");
            m.renderQueue = 3000;
            m.color = c;
            m.SetFloat("_Glossiness", gloss);
        }

        /// Светящееся окно/лампа (для bloom).
        public static void Emissive(Renderer r, Color c, float intensity = 1.8f)
        {
            var m = r.material;
            m.color = c;
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            m.SetColor("_EmissionColor", c * intensity);
        }

        /// Постэффекты на камеру: ACES, bloom, лёгкая виньетка, SSAO.
        /// Вызывается при построении сцены в редакторе (ссылки сериализуются).
        public static void SetupPostFx(Camera cam)
        {
            var layer = cam.gameObject.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
            if (layer != null) return;

#if UNITY_EDITOR
            const string resPath = "Packages/com.unity.postprocessing/PostProcessing/PostProcessResources.asset";
            var res = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.PostProcessing.PostProcessResources>(resPath);
            if (res == null)
            {
                Debug.LogWarning("[Art] PostProcessResources не найден, постэффекты пропущены");
                return;
            }

            layer = cam.gameObject.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
            layer.volumeLayer = 1 << 0; // Default
            layer.Init(res);

            var profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.PostProcessing.PostProcessProfile>();

            var bloom = profile.AddSettings<UnityEngine.Rendering.PostProcessing.Bloom>();
            bloom.enabled.Override(true);
            bloom.intensity.Override(1.1f);
            bloom.threshold.Override(0.9f);

            var cg = profile.AddSettings<UnityEngine.Rendering.PostProcessing.ColorGrading>();
            cg.enabled.Override(true);
            cg.tonemapper.Override(UnityEngine.Rendering.PostProcessing.Tonemapper.ACES);
            cg.saturation.Override(8f);
            cg.contrast.Override(10f);
            cg.temperature.Override(8f);

            var vig = profile.AddSettings<UnityEngine.Rendering.PostProcessing.Vignette>();
            vig.enabled.Override(true);
            vig.intensity.Override(0.22f);

            var ao = profile.AddSettings<UnityEngine.Rendering.PostProcessing.AmbientOcclusion>();
            ao.enabled.Override(true);
            ao.intensity.Override(0.55f);
            ao.radius.Override(0.4f);

            var volGo = new GameObject("PostVolume");
            var vol = volGo.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>();
            vol.isGlobal = true;
            vol.profile = profile;
#endif
        }
    }
}
