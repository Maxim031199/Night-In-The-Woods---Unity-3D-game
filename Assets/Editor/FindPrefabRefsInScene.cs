// Assets/Editor/FindPrefabRefsInScene.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FindPrefabRefsInScene : EditorWindow
{
    Object targetAsset;

    [MenuItem("Tools/Refs/Find Prefab References In Active Scene")]
    static void Open() => GetWindow<FindPrefabRefsInScene>("Find Prefab Refs").Show();

    void OnGUI()
    {
        EditorGUILayout.HelpBox("Pick the prefab asset (MainMenuLayout) and click Scan.", MessageType.Info);
        targetAsset = EditorGUILayout.ObjectField("Target asset", targetAsset, typeof(Object), false);

        using (new EditorGUI.DisabledScope(!targetAsset))
        {
            if (GUILayout.Button("Scan Active Scene"))
                Scan(SceneManager.GetActiveScene(), targetAsset);
        }
    }

    static void Scan(Scene scene, Object asset)
    {
        int hits = 0;
        foreach (var go in scene.GetRootGameObjects())
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                var comps = t.GetComponents<Component>();
                foreach (var c in comps)
                {
                    if (!c) continue;
                    var so = new SerializedObject(c);
                    var prop = so.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                            prop.objectReferenceValue == asset)
                        {
                            hits++;
                            Debug.LogWarning(
                                $"[Ref] {GetPath(t.gameObject)}  →  {c.GetType().Name}.{prop.name}",
                                t.gameObject);
                        }
                    }
                }
            }
        Debug.Log($"[Ref] Done. Found {hits} reference(s) to {asset.name} in scene '{scene.name}'.");
    }

    static string GetPath(GameObject go)
    {
        System.Collections.Generic.List<string> parts = new();
        for (var t = go.transform; t != null; t = t.parent) parts.Add(t.name);
        parts.Reverse();
        return string.Join("/", parts);
    }
}
#endif
