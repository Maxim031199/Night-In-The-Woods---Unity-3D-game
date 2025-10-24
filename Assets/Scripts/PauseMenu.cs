using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Roots")]
    [SerializeField] GameObject pauseCanvas;   // whole Pause UI canvas (enable/disable this)
    [SerializeField] GameObject pauseRoot;     // main panel (Resume/Options/Exit)
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject creditsPanel;

    [Header("Scenes")]
    [SerializeField] string menuSceneName = "Menu";

    [Header("Input (New Input System)")]
    [SerializeField] InputActionReference backAction; // optional; if empty we bind <Keyboard>/escape
    InputAction _back;

    void Awake()
    {
        if (pauseCanvas) pauseCanvas.SetActive(false);  // hidden on start
        ShowOnly(pauseRoot);
    }

    void OnEnable()
    {
        _back = backAction != null
            ? backAction.action
            : new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");

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
        TogglePause();
    }

    // ===== public button hooks =====
    public void Resume() => SetPaused(false);
    public void OpenOptions() => ShowOnly(optionsPanel);
    public void OpenCredits() => ShowOnly(creditsPanel);
    public void Back() => ShowOnly(pauseRoot);
    public void QuitToMenu()
    {
        SetPaused(false);
        SceneManager.LoadScene(menuSceneName);
    }

    public void TogglePause()
    { 
        bool wantPause = !(pauseCanvas != null && pauseCanvas.activeSelf);
        SetPaused(wantPause);
    }

    // ===== core =====
    void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        SaveScript.inventoryOpen = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        if (pauseCanvas) pauseCanvas.SetActive(paused);
        if (paused) ShowOnly(pauseRoot); // always land on root when opening
    }

    void ShowOnly(GameObject panelToShow)
    {
        if (pauseRoot) pauseRoot.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (panelToShow) panelToShow.SetActive(true);
    }
}
