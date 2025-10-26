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
        TogglePause();
    }

    // button hooks
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

    // core code part we did
    void SetPaused(bool paused)
    {
        Time.timeScale = paused ? PausedTimeScale : UnpausedTimeScale;
        SaveScript.inventoryOpen = paused;
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
