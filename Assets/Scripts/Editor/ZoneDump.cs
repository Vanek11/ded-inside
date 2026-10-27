#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text;

public static class ZoneDump
{
    public static void Dump()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Village1.unity", OpenSceneMode.Single);
        var sb = new StringBuilder();
        foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            var p = t.position;
            if (p.x > -1f && p.x < 6f && p.z > -24f && p.z < -12f && p.y > 0.5f)
                sb.AppendLine($"{t.name} @ ({p.x:0.0},{p.y:0.0},{p.z:0.0})");
        }
        Debug.Log("[ZONE]\n" + sb.ToString());
        EditorApplication.Exit(0);
    }
}
#endif
