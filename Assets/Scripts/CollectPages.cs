using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CollectPages : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private GameObject counterUIObj;
    [SerializeField] private TMP_Text counterText;

    [Header("Audio")]
    [SerializeField] private AudioSource pickupSound;

    [Header("Gameplay")]
    [SerializeField] private GameObject monster;
    [SerializeField] private int totalPages = DefaultTotalPages;
    [SerializeField] private float reachRadius = DefaultReachRadius;
    [SerializeField] private bool resetCounterAtStartOnThisObject = false;

    [Header("Slender Difficulty")]
    [SerializeField] private SlenderManAI slenderAI;

    private Transform player;
    private bool inReach;
    public static int PagesCollected;

    private GameInputActions input;
    private bool pickupQueued;

    private const int DefaultTotalPages = 8;
    private const float DefaultReachRadius = 1.5f;
    private const int MinPages = 0;
    private const string DifficultyKey = "difficulty";
    private const int DefaultDifficulty = 0;
    private const float MinCooldown = 0.25f;

    void Awake()
    {
        player = Camera.main ? Camera.main.transform : null;

        if (promptUI) promptUI.SetActive(false);
        if (counterUIObj) counterUIObj.SetActive(false);

        if (resetCounterAtStartOnThisObject)
        {
            PagesCollected = MinPages;
            UpdateCounterUI();
        }

        input = new GameInputActions(); // create actions
    }

    void OnEnable()
    {
        input.Player.Enable();
        input.Player.pickup.performed += OnPickupPerformed; // E key
    }

    void OnDisable()
    {
        input.Player.pickup.performed -= OnPickupPerformed;
        input.Player.Disable();
    }

    private void OnPickupPerformed(InputAction.CallbackContext _)
    {
        pickupQueued = true;
    }

    void Update()
    {
        if (!player) return;

        // reach check
        bool nowInReach = (player.position - transform.position).sqrMagnitude <= reachRadius * reachRadius;
        if (nowInReach != inReach)
        {
            inReach = nowInReach;
            if (promptUI) promptUI.SetActive(inReach);
        }

        // consume queued input when valid
        if (pickupQueued)
        {
            pickupQueued = false;
            if (inReach && !GameUIState.InventoryOpen) Collect();
        }
    }

    private void Collect()
    {
        PagesCollected = Mathf.Clamp(PagesCollected + 1, MinPages, totalPages);
        UpdateCounterUI();

        ApplyDifficultyScaler();

        if (monster && !monster.activeSelf) monster.SetActive(true);
        if (pickupSound) pickupSound.Play();


        int idx = PagesCollected - 1;

        if (promptUI) promptUI.SetActive(false);
        gameObject.SetActive(false);
        inReach = false;
    }

    private void UpdateCounterUI()
    {
        if (counterText) counterText.text = $"{PagesCollected}/{totalPages} pages";
        if (counterUIObj) counterUIObj.SetActive(true);
    }

    private void ApplyDifficultyScaler()
    {
        if (!slenderAI) return;

        int diff = PlayerPrefs.GetInt(DifficultyKey, DefaultDifficulty);
        float dTeleportDist, dTeleCool, dReturnCool, dProb, dRot, dStaticRange;

        switch (diff)
        {
            case 0: dTeleportDist = 6f; dTeleCool = -1.0f; dReturnCool = -1.0f; dProb = 0.12f; dRot = 1.5f; dStaticRange = 1.0f; break;
            case 1: dTeleportDist = 5f; dTeleCool = -0.8f; dReturnCool = -0.8f; dProb = 0.10f; dRot = 1.2f; dStaticRange = 0.8f; break;
            case 2: dTeleportDist = 4f; dTeleCool = -0.6f; dReturnCool = -0.6f; dProb = 0.08f; dRot = 1.0f; dStaticRange = 0.6f; break;
            default: dTeleportDist = 2.5f; dTeleCool = -0.3f; dReturnCool = -0.4f; dProb = 0.05f; dRot = 0.6f; dStaticRange = 0.4f; break;
        }

        slenderAI.teleportDistance += dTeleportDist;
        slenderAI.teleportCooldown = Mathf.Max(MinCooldown, slenderAI.teleportCooldown + dTeleCool);
        slenderAI.returnCooldown = Mathf.Max(MinCooldown, slenderAI.returnCooldown + dReturnCool);
        slenderAI.chaseProbability = Mathf.Clamp01(slenderAI.chaseProbability + dProb);
        slenderAI.rotationSpeed += dRot;
        slenderAI.staticActivationRange += dStaticRange;
    }
}