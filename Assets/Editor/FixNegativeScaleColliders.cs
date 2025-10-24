#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class FixNegativeScaleColliders
{
    // --- Public API so others (like auto-runner) can call it ---
    [MenuItem("Tools/Colliders/Fix Negative-Scale Colliders (Scene)")]
    public static void FixWholeScene()
    {
#if UNITY_2023_1_OR_NEWER
        var all = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var all = Object.FindObjectsOfType<Collider>(true);
#endif
        Undo.IncrementCurrentGroup();
        int fixedCount = 0;
        foreach (var col in all)
        {
            if (!col) continue;
            if (!HasNegativeSign(col.transform)) continue;
            FixOneCollider(col);
            fixedCount++;
        }
        Debug.Log($"[FixNegativeScaleColliders] Fixed {fixedCount} collider(s) in scene.");
    }

    [MenuItem("Tools/Colliders/Fix Negative-Scale Colliders (Selection)")]
    public static void FixSelection()
    {
        var cols = Selection.gameObjects
            .SelectMany(g => g.GetComponentsInChildren<Collider>(true))
            .ToArray();

        Undo.IncrementCurrentGroup();
        int fixedCount = 0;
        foreach (var col in cols)
        {
            if (!col) continue;
            if (!HasNegativeSign(col.transform)) continue;
            FixOneCollider(col);
            fixedCount++;
        }
        Debug.Log($"[FixNegativeScaleColliders] Fixed {fixedCount} collider(s) in selection: {fixedCount}");
    }

    // ---------- helpers ----------
    private static bool HasNegativeSign(Transform t)
    {
        Vector3 sign = Vector3.one;
        while (t != null)
        {
            var s = t.localScale;
            sign = new Vector3(sign.x * Mathf.Sign(s.x), sign.y * Mathf.Sign(s.y), sign.z * Mathf.Sign(s.z));
            t = t.parent;
        }
        return sign.x < 0 || sign.y < 0 || sign.z < 0;
    }

    private static Vector3 GetCumulativeSign(Transform t)
    {
        Vector3 sign = Vector3.one;
        while (t != null)
        {
            var ls = t.localScale;
            sign = new Vector3(
                sign.x * Mathf.Sign(ls.x),
                sign.y * Mathf.Sign(ls.y),
                sign.z * Mathf.Sign(ls.z)
            );
            t = t.parent;
        }
        if (sign.x == 0) sign.x = 1;
        if (sign.y == 0) sign.y = 1;
        if (sign.z == 0) sign.z = 1;
        return sign;
    }

    private static void FixOneCollider(Collider original)
    {
        var t = original.transform;
        var go = t.gameObject;

        // Child wrapper that cancels the sign
        var wrapper = new GameObject($"{original.GetType().Name}_Wrapper");
        Undo.RegisterCreatedObjectUndo(wrapper, "Create Collider Wrapper");

        var wt = wrapper.transform;
        wt.SetParent(t, false);
        wt.localPosition = Vector3.zero;
        wt.localRotation = Quaternion.identity;

        var cancelSign = GetCumulativeSign(t);
        wt.localScale = new Vector3(
            cancelSign.x < 0 ? -1f : 1f,
            cancelSign.y < 0 ? -1f : 1f,
            cancelSign.z < 0 ? -1f : 1f
        );

        // Mirror layer/tag/static flags so physics/baking behave identically
        wrapper.layer = go.layer;
        wrapper.tag = go.tag;
        var flags = GameObjectUtility.GetStaticEditorFlags(go);
        GameObjectUtility.SetStaticEditorFlags(wrapper, flags);

        // Duplicate collider settings, remove original
        var newCol = Undo.AddComponent(wrapper, original.GetType()) as Collider;
        EditorUtility.CopySerialized(original, newCol);
        Undo.DestroyObjectImmediate(original);
    }
}
#endif
