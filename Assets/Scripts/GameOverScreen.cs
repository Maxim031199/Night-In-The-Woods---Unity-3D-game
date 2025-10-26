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
    [SerializeField] WinScreen winScreen;

    bool shown;

    void Awake()
    {
        if (gameOverCanvas) gameOverCanvas.SetActive(false);
    }

    void OnEnable()
    {
        if (!playerStatus)
        {
            playerStatus = FindFirstObjectByType<PlayerStatus>(FindObjectsInactive.Include);
            if (!playerStatus) playerStatus = FindAnyObjectByType<PlayerStatus>(FindObjectsInactive.Include);
        }

        if (playerStatus)
        {
            playerStatus.OnDead += HandleLose;
            playerStatus.OnInfectionMax += HandleLose;
        }
    }

    void OnDisable()
    {
        if (playerStatus)
        {
            playerStatus.OnDead -= HandleLose;
            playerStatus.OnInfectionMax -= HandleLose;
        }
    }

    void Update()
    {
        if (shown || playerStatus == null) return;
        if (playerStatus.Health <= 0 || playerStatus.Infection >= 100f) HandleLose();
    }

    void HandleLose()
    {
        if (shown) return;

        if (winScreen != null)
        {
            var field = typeof(WinScreen).GetField("shown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                bool winShown = (bool)field.GetValue(winScreen);
                if (winShown) return;
            }
        }

        shown = true;
        GameUIState.InventoryOpen = true;           //same check to force cursor 
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (gameOverCanvas) gameOverCanvas.SetActive(true);
        else Debug.LogWarning("[GameOverScreen] Assign GameOverCanvas.");
    }

    public void TriggerGameOver() => HandleLose();

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
