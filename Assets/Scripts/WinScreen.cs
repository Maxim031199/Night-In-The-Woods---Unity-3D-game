using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject winCanvas;

    [Header("Navigation")]
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Pages")]
    [SerializeField] private int totalPagesOverride = 0;

    [Header("Refs")]
    [SerializeField] private GameLogic gameLogic;

    private bool shown;
    private int totalPages;

    private const bool IncludeInactive = true;

    private static GameLogic FindGameLogicInScene()
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

    private void OnValidate()
    {
        if (!gameLogic)
            gameLogic = FindGameLogicInScene();
    }

    private void Awake()
    {
        if (!gameLogic)
            gameLogic = FindGameLogicInScene();

        totalPages = (totalPagesOverride > 0)
            ? totalPagesOverride
            : (gameLogic ? gameLogic.totalPages : 0);

        if (winCanvas)
            winCanvas.SetActive(false);
        else
            Debug.LogWarning("[WinScreen] winCanvas is not assigned!");
    }

    private void Update()
    {
        if (shown || totalPages <= 0)
            return;

        if (CollectPages.PagesCollected >= totalPages)
            ShowWin();
    }

    private void ShowWin()
    {
        if (shown)
            return;

        shown = true;

        RuntimeGameState.SetState(RuntimeState.Win);
        GameUIState.InventoryOpen = false;
        GameUIState.PauseOpen = true;   

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (winCanvas)
        {
            winCanvas.SetActive(true);
        }
        
    }

    public void ExitToMenu()
    {
        RuntimeGameState.SetState(RuntimeState.None);
        GameUIState.InventoryOpen = false;
        GameUIState.PauseOpen = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(menuSceneName);
    }
}
