using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject winCanvas;

    [Header("Navigation")]
    [SerializeField] string menuSceneName = "Menu";

    [Header("Pages")]
    [SerializeField] int totalPagesOverride = 0;

    [Header("Refs")]
    [SerializeField] GameLogic gameLogic;   // assign in Inspector if possible

    bool shown;
    int totalPages;

    // Helper: find a GameLogic in the active scene (including inactive)
    static GameLogic FindGameLogicInScene()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var gl = roots[i].GetComponentInChildren<GameLogic>(true);
            if (gl) return gl;
        }
        return null;
    }

    // In editor, auto-wire if missing when values change
    void OnValidate()
    {
        if (!gameLogic)
            gameLogic = FindGameLogicInScene();
    }

    void Awake()
    {
        if (!gameLogic)
            gameLogic = FindGameLogicInScene();

        totalPages = (totalPagesOverride > 0)
            ? totalPagesOverride
            : (gameLogic ? gameLogic.totalPages : 0);

        if (winCanvas)
            winCanvas.SetActive(false);
        else
            Debug.LogWarning("[WinScreen] Assign WinCanvas.");
    }

    void Update()
    {
        if (shown || totalPages <= 0) return;

        if (CollectPages.PagesCollected >= totalPages)
            ShowWin();
    }

    void ShowWin()
    {
        if (shown) return;
        shown = true;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (winCanvas) winCanvas.SetActive(true);
        else Debug.LogWarning("[WinScreen] WinCanvas not assigned.");
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
