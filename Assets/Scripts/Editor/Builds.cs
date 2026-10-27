#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

/// Batch-билды: Unity -batchmode -executeMethod Builds.BuildWindowsAuto
public static class Builds
{
    public static void BuildWindowsAuto()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Windows/dedinside.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"[DeadInside] Билд готов: {report.summary.outputPath} ({report.summary.totalSize / 1048576} MB)");
        else
            Debug.LogError($"[DeadInside] Билд не удался: {report.summary.result}");
    }

    [MenuItem("DeadInside/Собрать Windows-билд")]
    public static void BuildWindowsMenu() => BuildWindowsAuto();
}
#endif
