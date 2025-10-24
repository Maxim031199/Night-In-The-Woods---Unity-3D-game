#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class AutoFixNegativeScaleColliders
{
    static AutoFixNegativeScaleColliders()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Call the REAL fixer class/method
            FixNegativeScaleColliders.FixWholeScene();
        }
    }
}
#endif
