#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DI;

/// Меню «DeadInside»: строит сцену главного меню и прописывает обе сцены в билд.
public static class MenuSetup
{
    [MenuItem("DeadInside/Построить сцену меню")]
    public static void BuildMenu()
    {
        if (!EditorUtility.DisplayDialog("DeadInside",
                "Построить сцену меню заново? Несохранённые изменения будут потеряны.",
                "Строить", "Отмена"))
            return;
        Build();
    }

    /// Для batchmode: Unity -executeMethod MenuSetup.BuildMenuAuto
    public static void BuildMenuAuto() => Build();

    static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var go = new GameObject("Menu");
        go.AddComponent<MenuBootstrap>().EditorBuild();

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Menu.unity");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Village1.unity", true)
        };
        Debug.Log("[DeadInside] Меню построено и сохранено; Village1 остаётся в билде");
    }
}
#endif
