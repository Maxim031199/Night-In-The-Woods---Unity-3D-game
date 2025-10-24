using UnityEngine;
using UnityEngine.InputSystem;   // <-- New Input System
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    [Header("Panels / Roots")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject extrasPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Input (New Input System)")]

    

    private InputAction _back;

    void Awake()
    {
        ShowOnly(menuRoot);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


    }

    void OnEnable()
    {
        _back = new InputAction("Back", InputActionType.Button, "<Keyboard>/escape");
        _back.performed += OnBackPerformed;
        _back.Enable();
    }

    void OnDisable()
    {
        if (_back != null)
        {
            _back.performed -= OnBackPerformed;
            _back.Disable();
            _back.Dispose();
            _back = null;
        }
    }

    private void OnBackPerformed(InputAction.CallbackContext _)
    {
        GoBack();
    }

    // ------- Top-level buttons -------
    public void OpenOptions() => ShowOnly(optionsPanel);
    public void OpenExtras() => ShowOnly(extrasPanel);
    public void OpenCredits() => ShowOnly(creditsPanel);
    public void PlayGame() => ShowOnly(difficultyPanel);
    public void GoBack() => ShowOnly(menuRoot);

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ------- Difficulty buttons -------
    public void StartVeryHard() => StartGameWithDifficulty(0);
    public void StartHard() => StartGameWithDifficulty(1);
    public void StartMedium() => StartGameWithDifficulty(2);
    public void StartEasy() => StartGameWithDifficulty(3);

    private void StartGameWithDifficulty(int diffIndex)
    {
        PlayerPrefs.SetInt("difficulty", diffIndex);
        PlayerPrefs.Save();

        var gsm = FindFirstObjectByType<GameStateManager>();
        gsm?.EnterStateByName("InGameState");   // name of your GameState child


        if (loadingPanel) loadingPanel.SetActive(true);
        SceneManager.LoadScene(gameSceneName); // sync load
    }

    // ------- Helpers -------
    private void ShowOnly(GameObject panelToShow)
    {
        if (menuRoot) menuRoot.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (extrasPanel) extrasPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (difficultyPanel) difficultyPanel.SetActive(false);
        if (loadingPanel) loadingPanel.SetActive(false);

        if (panelToShow) panelToShow.SetActive(true);
    }
}
