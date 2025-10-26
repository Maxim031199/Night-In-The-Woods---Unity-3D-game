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
    [SerializeField] GameLogic gameLogic;

    bool shown;
    int totalPages;


    private const float TimeScalePaused = 0f;
    private const float TimeScaleRunning = 1f;
    private const bool IncludeInactive = true;


    static GameLogic FindGameLogicInScene()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var gl = roots[i].GetComponentInChildren<GameLogic>(IncludeInactive);
            if (gl) return gl;
        }
        return null;
    }


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
        GameUIState.InventoryOpen = true;  //same flag to force cursor 

        Time.timeScale = TimeScalePaused;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (winCanvas) winCanvas.SetActive(true);
        else Debug.LogWarning("[WinScreen] WinCanvas not assigned.");
    }

    public void ExitToMenu()
    {
        Time.timeScale = TimeScaleRunning;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(menuSceneName);
    }
}
