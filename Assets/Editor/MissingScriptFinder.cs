using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingScriptFinder : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow<MissingScriptFinder>("Missing Scripts");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in Active Scene"))
            FindMissingScriptsInScene();

        if (GUILayout.Button("Remove Missing Scripts in Active Scene (Destructive)"))
            RemoveMissingScriptsInScene();
    }

    static void FindMissingScriptsInScene()
    {
        int hits = 0;
        foreach (var go in EnumerateAllSceneGameObjects(includeInactive: true))
        {
            var comps = go.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    hits++;
                    string path = GetHierarchyPath(go);
                    Debug.LogWarning($"[MissingScripts] Object: {path}  (Component slot #{i})", go);
                    EditorGUIUtility.PingObject(go);
                }
            }
        }
        Debug.Log($"[MissingScripts] Finished. Found {hits} missing script slot(s).");
    }

    static void RemoveMissingScriptsInScene()
    {
        if (!EditorUtility.DisplayDialog("Remove missing scripts?",
            "This will remove ALL missing-script components in the active scene.\nConsider committing first.",
            "Remove", "Cancel")) return;

        int removed = 0;
        foreach (var go in EnumerateAllSceneGameObjects(includeInactive: true))
        {
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }
        Debug.Log($"[MissingScripts] Removed {removed} missing script component(s).");
    }

    static IEnumerable<GameObject> EnumerateAllSceneGameObjects(bool includeInactive)
    {
        // Get every Transform in the active scene (faster & reliable)
#if UNITY_2023_1_OR_NEWER
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(
                         includeInactive ? true : false))
                yield return t.gameObject;
        }
#else
        // Older Unity fallback
        foreach (var t in Object.FindObjectsOfType<Transform>())
        {
            if (!includeInactive && !t.gameObject.activeInHierarchy) continue;
            yield return t.gameObject;
        }
#endif
    }

    static string GetHierarchyPath(GameObject go)
    {
        var parts = new System.Collections.Generic.List<string>();
        for (var t = go.transform; t != null; t = t.parent)
            parts.Add(string.IsNullOrEmpty(t.name) ? "<unnamed>" : t.name);
        parts.Reverse();
        return string.Join("/", parts);
    }
}
