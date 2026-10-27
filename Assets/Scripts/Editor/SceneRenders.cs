#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DI;

/// Автоскриншоты уровней: рендерит камеры вдоль дороги в Screenshots/*.png,
/// чтобы править визуал без запуска игры. Unity -executeMethod SceneRenders.RenderAllAuto
public static class SceneRenders
{
    public static void RenderAllAuto()
    {
        foreach (var name in new[] { "Village1", "Village2", "Village3" })
            RenderScene(name);
        EditorApplication.Exit(0);
    }

    [MenuItem("DeadInside/Отрендерить скриншоты уровней")]
    public static void RenderMenu()
    {
        foreach (var name in new[] { "Village1", "Village2", "Village3" })
            RenderScene(name);
    }

    static void RenderScene(string name)
    {
        EditorSceneManager.OpenScene($"Assets/Scenes/{name}.unity", OpenSceneMode.Single);

        var camGo = new GameObject("RenderCam");
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.2f;
        cam.farClipPlane = 500f;
        cam.clearFlags = CameraClearFlags.Skybox;
        Art.SetupPostFx(cam); // тот же постэффект, что у игровой камеры

        // обзорные точки вдоль маршрута
        var shots = new (Vector3 pos, Vector3 look)[]
        {
            (new Vector3(10f, 2.4f, -11f), new Vector3(-2f, 2.2f, -22f)), // дом сбоку
            (new Vector3(7f, 3.5f, 8f),    new Vector3(0f, 1f, 26f)),     // дорога и холмы
            (new Vector3(6f, 2.6f, 34f),   new Vector3(0f, -0.4f, 44f)),  // пруд и мост
            (new Vector3(0f, 25f, 46f),    new Vector3(0f, 0f, 40f)),     // пруд сверху
            (new Vector3(-7f, 1.2f, 42f),  new Vector3(0f, 0f, 42f)),     // пруд с воды
            (new Vector3(7f, 4.2f, 66f),   new Vector3(0f, 2f, 80f)),     // кладбище
            (new Vector3(0f, 60f, 30f),    new Vector3(0f, 0f, 30.1f)),   // карта сверху
            (new Vector3(30f, 8f, 42f),    new Vector3(0f, -1f, 42f)),    // пруд сбоку свет
        };

        System.IO.Directory.CreateDirectory("Screenshots");
        int i = 0;
        foreach (var (pos, look) in shots)
        {
            camGo.transform.position = pos;
            camGo.transform.LookAt(look);

            var rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            System.IO.File.WriteAllBytes($"Screenshots/{name}_{i}.png", tex.EncodeToPNG());
            RenderTexture.active = null;
            rt.Release();
            Object.DestroyImmediate(tex);
            i++;
        }
        Object.DestroyImmediate(camGo);
        Debug.Log($"[Renders] {name}: 4 скриншота сохранены");
    }
}
#endif
