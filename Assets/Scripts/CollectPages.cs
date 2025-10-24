using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private int totalPages = 8;
    [SerializeField] private float reachRadius = 1.5f;
    [SerializeField] private bool resetCounterAtStartOnThisObject = false;

    [Header("Slender Difficulty")]
    [SerializeField] private SlenderManAI slenderAI;

    // --- internal ---
    private Transform player;
    private bool inReach;
    public static int PagesCollected;

    // --- New Input System (same pattern as SaveScript) ---
    private GameInputActions input;   // generated C# class from your .inputactions
    private bool pickupQueued;        // set by the input callback

    void Awake()
    {
        player = Camera.main ? Camera.main.transform : null;

        if (promptUI) promptUI.SetActive(false);
        if (counterUIObj) counterUIObj.SetActive(false);

        if (resetCounterAtStartOnThisObject)
        {
            PagesCollected = 0;
            UpdateCounterUI();
        }

        input = new GameInputActions();          // create actions (no inspector refs needed)
    }

    void OnEnable()
    {
        input.Player.Enable();
        input.Player.pickup.performed += OnPickupPerformed;   // E key by your binding
    }

    void OnDisable()
    {
        input.Player.pickup.performed -= OnPickupPerformed;
        input.Player.Disable();
    }

    private void OnPickupPerformed(InputAction.CallbackContext _)
    {
        // Queue; we'll validate range & UI state in Update()
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
            pickupQueued = false; // consume once
            // Optional gate if you don’t want pickups while inventory is open:
            if (inReach && !GameUIState.InventoryOpen) Collect();
        }
    }

    private void Collect()
    {
        PagesCollected = Mathf.Clamp(PagesCollected + 1, 0, totalPages);
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

        int diff = PlayerPrefs.GetInt("difficulty", 0); // 0 VH, 1 H, 2 M, 3 E
        float dTeleportDist, dTeleCool, dReturnCool, dProb, dRot, dStaticRange;

        switch (diff)
        {
            case 0: dTeleportDist = 6f; dTeleCool = -1.0f; dReturnCool = -1.0f; dProb = 0.12f; dRot = 1.5f; dStaticRange = 1.0f; break;
            case 1: dTeleportDist = 5f; dTeleCool = -0.8f; dReturnCool = -0.8f; dProb = 0.10f; dRot = 1.2f; dStaticRange = 0.8f; break;
            case 2: dTeleportDist = 4f; dTeleCool = -0.6f; dReturnCool = -0.6f; dProb = 0.08f; dRot = 1.0f; dStaticRange = 0.6f; break;
            default: dTeleportDist = 2.5f; dTeleCool = -0.3f; dReturnCool = -0.4f; dProb = 0.05f; dRot = 0.6f; dStaticRange = 0.4f; break;
        }

        slenderAI.teleportDistance += dTeleportDist;
        slenderAI.teleportCooldown = Mathf.Max(0.25f, slenderAI.teleportCooldown + dTeleCool);
        slenderAI.returnCooldown = Mathf.Max(0.25f, slenderAI.returnCooldown + dReturnCool);
        slenderAI.chaseProbability = Mathf.Clamp01(slenderAI.chaseProbability + dProb);
        slenderAI.rotationSpeed += dRot;
        slenderAI.staticActivationRange += dStaticRange;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, reachRadius);
    }
#endif
}
