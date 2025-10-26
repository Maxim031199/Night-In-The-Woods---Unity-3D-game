using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class SaveScript : MonoBehaviour
{


    [SerializeField] public static int weaponID = 0;
    [SerializeField] public static bool[] weaponPickedUp = new bool[9];
    [SerializeField] public static int itemID = 0;
    [SerializeField] public static bool[] itemsPickedUp = new bool[13];
    [SerializeField] public static int[] weaponAmts = new int[9];
    [SerializeField] public static int[] itemAmts = new int[13];
    [SerializeField] int pistolDamage = 25;
    [SerializeField] int shotgunDamage = 60;
    [SerializeField] public static bool change = false;
    [SerializeField] public static int[] ammoAmts = new int[2];
    [SerializeField] public static int[] currentAmmo = new int[9];
    [SerializeField] public static float stamina = 100f;
    [SerializeField] public static float infection;
    [SerializeField] public static int health;
    GameInputActions inputActions;
    [SerializeField] public static List<GameObject> zombiesChasing = new List<GameObject>();
    private RaycastHit gunHit;
    private RaycastHit[] shotgunHits;
    bool attackQueued = false;




    // runtime state from input callbacks
    Vector2 moveValue;
    bool isSprinting;

    // tuning 
    [Header("Stamina Tuning")]
    [SerializeField] float drainPerSecond = 10f;     // when sprinting and moving
    [SerializeField] float regenPerSecond = 3.35f;   // when not draining
    [SerializeField] float maxStamina = 100f;

    void Awake()
    {
        inputActions = new();   
    }
    void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;

        inputActions.Player.Sprint.started += OnSprintStarted;
        inputActions.Player.Sprint.canceled += OnSprintCanceled;
        inputActions.Player.Attack.performed += OnAttackPerformed;

        // if we expose actions here
    }
    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Sprint.started -= OnSprintStarted;
        inputActions.Player.Sprint.canceled -= OnSprintCanceled;

        inputActions.Player.Disable();
    }
    void OnMovePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        => moveValue = ctx.ReadValue<Vector2>();

    void OnMoveCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        => moveValue = Vector2.zero;

    void OnSprintStarted(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        => isSprinting = true;

    void OnSprintCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        => isSprinting = false;
    void OnAttackPerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!inventoryOpen && Time.timeScale > 0f) attackQueued = true; 
    }
    
        
    
    void Start()
    {
        stamina = FPController.FPSstamina;
        health = 100;

        weaponPickedUp[0] = true;
        itemsPickedUp[0] = true;
        itemsPickedUp[1] = true;
        itemsPickedUp[2] = true;

        itemAmts[0] = 1;
        itemAmts[1] = 1;

        ammoAmts[0] = 20;
        ammoAmts[1] = 13;

        for (int i = 0; i < currentAmmo.Length; i++) currentAmmo[i] = 0;
        currentAmmo[4] = 16; // pistol
        currentAmmo[5] = 7;  // shotgun
        currentAmmo[6] = 0;  // spray
    }


    void Update()
    {
        // Pause / inventory
        if (Time.timeScale == 0f || inventoryOpen)
        {
            stamina = Mathf.Min(maxStamina, stamina + regenPerSecond * Time.unscaledDeltaTime);
            FPController.FPSstamina = stamina; 
            return;
        }

        bool isMoving = moveValue.sqrMagnitude > 0.0001f;

        if (isSprinting && isMoving)
            stamina -= drainPerSecond * Time.deltaTime;
        else if (stamina < maxStamina)
            stamina += regenPerSecond * Time.deltaTime;

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        FPController.FPSstamina = stamina; // mirror for any legacy readers
        if (infection < 50)
            infection += 0.4f * Time.deltaTime;
        if (infection > 49 && infection < 100)
            infection += 0.8f * Time.deltaTime;


        // pistol
        if (weaponID == 4 && currentAmmo[4] > 0 && attackQueued)
        {
            if (Physics.SphereCast(transform.position, 0.01f, transform.forward,
                                   out gunHit, 100f, ~0, QueryTriggerInteraction.Ignore))
            {
                var zg = gunHit.transform.GetComponentInParent<ZombieGunDamage>();
                if (zg != null)
                {
                    zg.SendGunDamage(gunHit.point, pistolDamage);
                    
                }
            }     
            attackQueued = false;
        }

        // shotgun
        if (weaponID == 5 && currentAmmo[5] > 0 && attackQueued)
        {
            

            shotgunHits = Physics.SphereCastAll(transform.position, 0.3f, transform.forward,
                                                50f, ~0, QueryTriggerInteraction.Ignore);

            var damaged = new System.Collections.Generic.HashSet<ZombieDamage>();

            for (int i = 0; i < shotgunHits.Length; i++)
            {
                // stop depending on the child name
                var zg = shotgunHits[i].transform.GetComponentInParent<ZombieGunDamage>();
                if (zg == null) continue;

                var zd = zg.zombieDamageObj ? zg.zombieDamageObj.GetComponent<ZombieDamage>() : null;
                if (zd == null || !damaged.Add(zd)) continue;   // skip duplicates of the same zombie

                zd.gunDamage(shotgunHits[i].point, shotgunDamage);
            }

            attackQueued = false; 
        }

    }

    public static bool inventoryOpen
    {
        get => GameUIState.InventoryOpen;
        set => GameUIState.InventoryOpen = value;
    }
}
