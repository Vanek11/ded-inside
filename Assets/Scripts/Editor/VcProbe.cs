#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class VcProbe
{
    public static void Probe()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Village1.unity", OpenSceneMode.Single);
        var pt = new Vector3(2.8f, 1.5f, -18.5f);
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r.bounds.Contains(pt))
                Debug.Log($"[HIT] {r.transform.root.name}/{r.transform.name} bounds={r.bounds} mat={r.sharedMaterial?.name}");
        }
        // и вторая гипотеза — правее/выше
        var pt2 = new Vector3(3.5f, 2f, -19f);
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r.bounds.Contains(pt2))
                Debug.Log($"[HIT2] {r.transform.root.name}/{r.transform.name} bounds={r.bounds} mat={r.sharedMaterial?.name}");
        }
        EditorApplication.Exit(0);
    }
}
#endif
