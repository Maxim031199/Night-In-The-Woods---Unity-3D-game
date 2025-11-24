using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Roots")]
    [SerializeField] GameObject pauseCanvas;
    [SerializeField] GameObject pauseRoot;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject creditsPanel;

    [Header("Scenes")]
    [SerializeField] string menuSceneName = "Menu";

    [Header("Input (New Input System)")]
    [SerializeField] InputActionReference backAction;
    [SerializeField] string backBinding = "<Keyboard>/escape";
    [SerializeField] string backActionName = "Back";

    [Header("Timing")]
    private const float PausedTimeScale = 0f;
    private const float UnpausedTimeScale = 1f;

    private InputAction _back;

    void Awake()
    {
        if (pauseCanvas) pauseCanvas.SetActive(false);
        ShowOnly(pauseRoot);
    }

    void OnEnable()
    {
        _back = backAction != null
            ? backAction.action
            : new InputAction(name: backActionName, type: InputActionType.Button, binding: backBinding);

        _back.performed += OnBackPerformed;
        _back.Enable();
    }

    void OnDisable()
    {
        if (_back != null)
        {
            _back.performed -= OnBackPerformed;
            _back.Disable();
            if (backAction == null) _back.Dispose();
        }
    }

    void OnBackPerformed(InputAction.CallbackContext _)
    {
        if (GameUIState.InventoryOpen ||
        RuntimeGameState.Current == RuntimeState.Win ||
        RuntimeGameState.Current == RuntimeState.GameOver)
            return;
        TogglePause();
    }

    public void Resume() => SetPaused(false);
    public void OpenOptions() => ShowOnly(optionsPanel);
    public void OpenCredits() => ShowOnly(creditsPanel);
    public void Back() => ShowOnly(pauseRoot);

    public void QuitToMenu()
    {
        SetPaused(false);
        RuntimeGameState.SetState(RuntimeState.None);
        GameUIState.PauseOpen = false;
        SceneManager.LoadScene(menuSceneName);
    }

    public void TogglePause()
    {
        bool wantPause = !(pauseCanvas != null && pauseCanvas.activeSelf);
        SetPaused(wantPause);
    }

    void SetPaused(bool paused)
    {
        if (paused)
        {
            if (RuntimeGameState.Current != RuntimeState.Playing &&
                RuntimeGameState.Current != RuntimeState.Paused)
            {
                return;
            }

            RuntimeGameState.SetState(RuntimeState.Paused);
            GameUIState.PauseOpen = true;
        }
        else
        {
            if (RuntimeGameState.Current == RuntimeState.Paused)
            {
                RuntimeGameState.SetState(RuntimeState.Playing);
            }
            GameUIState.PauseOpen = false;
        }

        Time.timeScale = paused ? PausedTimeScale : UnpausedTimeScale;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        if (pauseCanvas) pauseCanvas.SetActive(paused);
        if (paused) ShowOnly(pauseRoot);
    }


    void ShowOnly(GameObject panelToShow)
    {
        if (pauseRoot) pauseRoot.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (panelToShow) panelToShow.SetActive(true);
    }
}