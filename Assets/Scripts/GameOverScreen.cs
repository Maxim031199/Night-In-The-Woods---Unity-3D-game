using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject gameOverCanvas;

    [Header("Navigation")]
    [SerializeField] string menuSceneName = "Menu";

    [Header("Refs")]
    [SerializeField] PlayerStatus playerStatus;

    bool shown;

    void Awake()
    {
        if (gameOverCanvas)
            gameOverCanvas.SetActive(false);
    }

    void OnEnable()
    {
        if (!playerStatus)
            playerStatus = FindFirstObjectByType<PlayerStatus>(FindObjectsInactive.Include);

        if (playerStatus)
        {
            playerStatus.OnDead += ShowGameOver;
            playerStatus.OnInfectionMax += ShowGameOver;
        }
    }

    void OnDisable()
    {
        if (playerStatus)
        {
            playerStatus.OnDead -= ShowGameOver;
            playerStatus.OnInfectionMax -= ShowGameOver;
        }
    }

    void Update()
    {
        if (shown || playerStatus == null) return;
        if (playerStatus.Health <= 0 || playerStatus.Infection >= 100f)
            ShowGameOver();
    }

    void ShowGameOver()
    {
        if (shown) return;
        shown = true;

        RuntimeGameState.SetState(RuntimeState.GameOver);
        GameUIState.InventoryOpen = false;
        GameUIState.PauseOpen = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (gameOverCanvas)
            gameOverCanvas.SetActive(true);
    }

    public void Retry()
    {
        RuntimeGameState.SetState(RuntimeState.Playing);
        GameUIState.InventoryOpen = false;
        GameUIState.PauseOpen = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        RuntimeGameState.SetState(RuntimeState.None);
        GameUIState.InventoryOpen = false;
        GameUIState.PauseOpen = false;

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}