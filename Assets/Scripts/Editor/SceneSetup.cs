#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using DI;

/// Меню «DeadInside» в редакторе: строит и сохраняет сцену уровня кодом.
public static class SceneSetup
{
    [MenuItem("DeadInside/Построить и сохранить сцену «Деревня-1»")]
    public static void BuildVillage1()
    {
        if (!EditorUtility.DisplayDialog("DeadInside",
                "Построить сцену «Деревня-1» заново? Несохранённые изменения текущей сцены будут потеряны.",
                "Строить", "Отмена"))
            return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var levelGo = new GameObject("Level");
        var level = levelGo.AddComponent<LevelVillage1>();
        level.EditorBuild(); // в edit mode Awake не вызывается — строим явно

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Village1.unity");
        EditorBuildSettings.scenes = new[]
            { new EditorBuildSettingsScene("Assets/Scenes/Village1.unity", true) };

        Debug.Log("[DeadInside] Сцена Village1 построена и сохранена. Жми Play!");
    }

    /// Для batchmode: строит сцену без диалогов (Unity -executeMethod SceneSetup.BuildVillage1Auto).
    public static void BuildVillage1Auto()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var levelGo = new GameObject("Level");
        var level = levelGo.AddComponent<LevelVillage1>();
        level.EditorBuild(); // в edit mode Awake не вызывается — строим явно

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Village1.unity");
        EditorBuildSettings.scenes = new[]
            { new EditorBuildSettingsScene("Assets/Scenes/Village1.unity", true) };

        Debug.Log("[DeadInside] AUTO: сцена Village1 построена и сохранена.");
    }
}
#endif
