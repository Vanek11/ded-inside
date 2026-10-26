#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// Smoke-тест в batchmode: входит в Play и проверяет ядро сцены, всегда завершается.
/// Запуск: Unity -batchmode -executeMethod PlayModeSmoke.Run
public static class PlayModeSmoke
{
    static double startAt = -1.0;
    static int failed;

    public static void Run()
    {
        startAt = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
        EditorApplication.isPlaying = true;
    }

    static void Tick()
    {
        double elapsed = EditorApplication.timeSinceStartup - startAt;
        // ждём входа в play до 20 сек; дальше проверяем в любом случае (fail safe)
        if (!EditorApplication.isPlaying && elapsed < 20.0) return;
        if (elapsed < 15.0) return;

        var player = GameObject.Find("Player");
        var coffin = Object.FindFirstObjectByType<DI.CoffinBody>();
        var game = DI.DIGameManager.I;

        if (!EditorApplication.isPlaying)
            Debug.LogError("[SMOKE] FAIL: play mode так и не начался за 20 сек");

        if (player == null) { Debug.LogError("[SMOKE] FAIL: Player отсутствует"); failed++; }
        if (coffin == null) { Debug.LogError("[SMOKE] FAIL: Гроб отсутствует"); failed++; }
        if (game == null) { Debug.LogError("[SMOKE] FAIL: DIGameManager отсутствует"); failed++; }

        if (coffin != null)
        {
            if (coffin.transform.position.y < -10f)
            { Debug.LogError("[SMOKE] FAIL: гроб провалился под мир"); failed++; }
            else
                Debug.Log($"[SMOKE] гроб: y={coffin.transform.position.y:0.00}, крышка закрыта={coffin.LidClosed}, дед={(coffin.Grandpa != null ? "на месте" : "НЕТ!")}");
            if (coffin.Grandpa == null) failed++;
        }

        if (failed == 0) Debug.Log("[SMOKE] OK — ядро сцены в play живо");

        EditorApplication.isPlaying = false;
        EditorApplication.update -= Tick;
        EditorApplication.Exit(failed);
    }
}
#endif
