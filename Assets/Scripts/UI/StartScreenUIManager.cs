using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenUIManager : MonoBehaviour


{

    [SerializeField] private GameObject mainMenuRoot;   
    [SerializeField] private GameObject difficultyPanel; 
    
    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button extraButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button exitButton;
    [SerializeField] Button backButton;

    [Header("Panels")]
    [SerializeField] GameObject buttonsPanel;
    [SerializeField] GameObject creditsPanel;

    void Awake()
    {
        var root = GetComponentInParent<Canvas>(true)?.transform;
        if (!root) return;

        if (!mainMenuRoot) mainMenuRoot = root.Find("MainMenu")?.gameObject;
        if (!difficultyPanel) difficultyPanel = root.Find("Difficulty")?.gameObject;
        if (!buttonsPanel) buttonsPanel = root.Find("MainMenu/ButtonsPanel")?.gameObject;
        if (!creditsPanel) creditsPanel = root.Find("CreditsPanel")?.gameObject;

        if (!startButton) startButton = root.Find("MainMenu/ButtonsPanel/PlayButton")?.GetComponent<Button>();
        if (!creditsButton) creditsButton = root.Find("MainMenu/ButtonsPanel/CreditsButton")?.GetComponent<Button>();
        if (!backButton) backButton = root.Find("CreditsPanel/Button")?.GetComponent<Button>();
        if (!exitButton) exitButton = root.Find("MainMenu/ButtonsPanel/ExitButton")?.GetComponent<Button>();
    }

    void Start()
    {

        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        AddButtonsListeners();
        ToggleCreditsScreen(false);
        AssignNamedActionTransition();
    }

    private void AddButtonsListeners()
    {
        startButton.onClick.AddListener(ShowDifficulty);
        creditsButton.onClick.AddListener(() => ToggleCreditsScreen(true));
        backButton.onClick.AddListener(() => ToggleCreditsScreen(false));
        exitButton.onClick.AddListener(() => Application.Quit());
    }

    private void ShowDifficulty()
    {
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (difficultyPanel) difficultyPanel.SetActive(true);
    }

    public void BackFromDifficulty()
    {
        if (difficultyPanel) difficultyPanel.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(true);
    }

    private void AssignNamedActionTransition()
    {
        var transitions = FindObjectsByType<NamedActionTransition>(FindObjectsSortMode.None);
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        foreach (var transition in transitions)
        {
            var selectedButton = buttons.FirstOrDefault(x => x.name.Equals(transition.actionName));
            if (selectedButton != null)
            {
                selectedButton.onClick.AddListener(transition.DoAction);
            }
        }
    }

    private void ToggleCreditsScreen(bool showCredits)
    {
        creditsPanel.gameObject.SetActive(showCredits);
        buttonsPanel.gameObject.SetActive(!showCredits);

        EventSystem.current.SetSelectedGameObject(showCredits
            ? backButton.gameObject : creditsButton.gameObject);
    }
}
