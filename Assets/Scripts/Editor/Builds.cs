#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

/// Batch-билды: Unity -batchmode -executeMethod Builds.BuildWindowsAuto
public static class Builds
{
    static string[] Scenes =>
        EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

    static void BuildTo(string path)
    {
        var opts = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = path,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"[DeadInside] Билд готов: {report.summary.outputPath} ({report.summary.totalSize / 1048576} MB)");
        else
            Debug.LogError($"[DeadInside] Билд не удался: {report.summary.result}");
    }

    /// Обычная версия — без админки.
    public static void BuildWindowsAuto() => BuildTo("Builds/Windows/dedinside.exe");

    /// Dev-версия с тестовой админкой (F1): define ADMIN на время сборки.
    public static void BuildWindowsDevAuto()
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "ADMIN");
        try
        {
            BuildTo("Builds/WindowsDev/dedinside.exe");
        }
        finally
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");
        }
    }

    [MenuItem("DeadInside/Собрать Windows-билд")]
    public static void BuildWindowsMenu() => BuildWindowsAuto();

    [MenuItem("DeadInside/Собрать Windows-билд (dev с админкой)")]
    public static void BuildWindowsDevMenu() => BuildWindowsDevAuto();
}
#endif
