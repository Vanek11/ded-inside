#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DI;

/// Меню «DeadInside»: строит и сохраняет сцены уровней кодом, прописывает билд.
public static class SceneSetup
{
    [MenuItem("DeadInside/Построить сцену «Деревня-1 · Пригорок»")]
    public static void BuildVillage1() => BuildDialog<LevelVillage1>("Assets/Scenes/Village1.unity", "Деревня-1");

    [MenuItem("DeadInside/Построить сцену «Деревня-2 · Огород»")]
    public static void BuildVillage2() => BuildDialog<LevelVillage2>("Assets/Scenes/Village2.unity", "Деревня-2");

    [MenuItem("DeadInside/Построить сцену «Деревня-3 · Сеновал»")]
    public static void BuildVillage3() => BuildDialog<LevelVillage3>("Assets/Scenes/Village3.unity", "Деревня-3");

    [MenuItem("DeadInside/Построить сцену меню")]
    public static void BuildMenu()
    {
        if (!EditorUtility.DisplayDialog("DeadInside",
                "Построить сцену меню заново? Несохранённые изменения будут потеряны.",
                "Строить", "Отмена"))
            return;
        BuildMenuScene();
    }

    // --- batch-точки входа: Unity -executeMethod SceneSetup.BuildXxxAuto ---

    public static void BuildVillage1Auto() => Build<LevelVillage1>("Assets/Scenes/Village1.unity");
    public static void BuildVillage2Auto() => Build<LevelVillage2>("Assets/Scenes/Village2.unity");
    public static void BuildVillage3Auto() => Build<LevelVillage3>("Assets/Scenes/Village3.unity");
    public static void BuildAllAuto()
    {
        Build<LevelVillage1>("Assets/Scenes/Village1.unity");
        Build<LevelVillage2>("Assets/Scenes/Village2.unity");
        Build<LevelVillage3>("Assets/Scenes/Village3.unity");
        BuildMenuScene();
    }

    static void BuildDialog<T>(string path, string label) where T : MonoBehaviour
    {
        if (!EditorUtility.DisplayDialog("DeadInside",
                $"Построить сцену «{label}» заново? Несохранённые изменения будут потеряны.",
                "Строить", "Отмена"))
            return;
        Build<T>(path);
    }

    static void Build<T>(string path) where T : MonoBehaviour
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var levelGo = new GameObject("Level");
        var level = levelGo.AddComponent<T>();
        var mb = level.GetType().GetMethod("EditorBuild");
        mb?.Invoke(level, null); // в edit mode Awake не вызывается — строим явно

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, path);
        ApplyBuildSettings();
        Debug.Log($"[DeadInside] Сцена {path} построена и сохранена");
    }

    static void BuildMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("Menu");
        go.AddComponent<MenuBootstrap>().EditorBuild();
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Menu.unity");
        ApplyBuildSettings();
        Debug.Log("[DeadInside] Меню построено");
    }

    static void ApplyBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Village1.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Village2.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Village3.unity", true)
        };
    }
}
#endif
