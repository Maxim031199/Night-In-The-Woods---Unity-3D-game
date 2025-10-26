using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class SaveScript : MonoBehaviour
{

    public enum WeaponId { Knife = 0, Cleaver = 1, Bat = 2, Axe = 3, Pistol = 4, Shotgun = 5, Spray = 6, Bottle = 7, Molotov = 8 }
    private const int WeaponsCount = 9;
    private const int ItemsCount = 13;


    private const int PistolIndex = (int)WeaponId.Pistol;
    private const int ShotgunIndex = (int)WeaponId.Shotgun;
    private const int SprayIndex = (int)WeaponId.Spray;


    private const float PistolSphereRadius = 0.01f;
    private const float PistolMaxRange = 100f;

    private const float ShotgunSphereRadius = 0.3f;
    private const float ShotgunMaxRange = 50f;

    private const int AllLayersMask = ~0;
    private const float PausedTimeScale = 0f;         
    private const float Zero = 0f;
    private const float MoveSqrMagThreshold = 0.0001f;


    private const float InfectionLowThreshold = 50f;
    private const float InfectionLowThresholdEdge = 49f;
    private const float InfectionMaxThreshold = 100f;


    [SerializeField] public static int weaponID = 0;
    [SerializeField] public static bool[] weaponPickedUp = new bool[WeaponsCount];
    [SerializeField] public static int itemID = 0;
    [SerializeField] public static bool[] itemsPickedUp = new bool[ItemsCount];
    [SerializeField] public static int[] weaponAmts = new int[WeaponsCount];
    [SerializeField] public static int[] itemAmts = new int[ItemsCount];

    [SerializeField] int pistolDamage = 25;
    [SerializeField] int shotgunDamage = 60;

    [SerializeField] public static bool change = false;

    [SerializeField] public static int[] ammoAmts = new int[2];          // pistol, shotgun 
    [SerializeField] public static int[] currentAmmo = new int[WeaponsCount];

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
    [SerializeField] float drainPerSecond = 10f;    // when sprinting and moving
    [SerializeField] float regenPerSecond = 3.35f;  // when not draining
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
        if (!inventoryOpen && Time.timeScale > PausedTimeScale)
            attackQueued = true;
    }

    void Start()
    {
        stamina = FPController.FPSstamina;
        health = 100;

        // Starting pickups 
        weaponPickedUp[0] = true;

        itemsPickedUp[0] = true;
        itemsPickedUp[1] = true;
        itemsPickedUp[2] = true;

        itemAmts[0] = 1;
        itemAmts[1] = 1;

        // Starter ammo 
        ammoAmts[0] = 20;
        ammoAmts[1] = 13;

        // Current ammo per weapon
        for (int i = 0; i < currentAmmo.Length; i++) currentAmmo[i] = 0;
        currentAmmo[PistolIndex] = 16;
        currentAmmo[ShotgunIndex] = 7;
        currentAmmo[SprayIndex] = 0;
    }

    void Update()
    {
        if (Time.timeScale == PausedTimeScale || inventoryOpen)
        {
            stamina = Mathf.Min(maxStamina, stamina + regenPerSecond * Time.unscaledDeltaTime);
            FPController.FPSstamina = stamina;
            return;
        }

        bool isMoving = moveValue.sqrMagnitude > MoveSqrMagThreshold;

        if (isSprinting && isMoving)
            stamina -= drainPerSecond * Time.deltaTime;
        else if (stamina < maxStamina)
            stamina += regenPerSecond * Time.deltaTime;

        stamina = Mathf.Clamp(stamina, Zero, maxStamina);
        FPController.FPSstamina = stamina; 

        if (infection < InfectionLowThreshold)
            infection += 0.4f * Time.deltaTime;
        if (infection > InfectionLowThresholdEdge && infection < InfectionMaxThreshold)
            infection += 0.8f * Time.deltaTime;

       
        if (weaponID == PistolIndex && currentAmmo[PistolIndex] > 0 && attackQueued)
        {
            if (Physics.SphereCast(transform.position, PistolSphereRadius, transform.forward,
                                   out gunHit, PistolMaxRange, AllLayersMask, QueryTriggerInteraction.Ignore))
            {
                var zg = gunHit.transform.GetComponentInParent<ZombieGunDamage>();
                if (zg != null)
                {
                    zg.SendGunDamage(gunHit.point, pistolDamage);
                }
            }
            attackQueued = false;
        }

        
        if (weaponID == ShotgunIndex && currentAmmo[ShotgunIndex] > 0 && attackQueued)
        {
            shotgunHits = Physics.SphereCastAll(transform.position, ShotgunSphereRadius, transform.forward,
                                                ShotgunMaxRange, AllLayersMask, QueryTriggerInteraction.Ignore);

            var damaged = new System.Collections.Generic.HashSet<ZombieDamage>();

            for (int i = 0; i < shotgunHits.Length; i++)
            {
                var zg = shotgunHits[i].transform.GetComponentInParent<ZombieGunDamage>();
                if (zg == null) continue;

                var zd = zg.zombieDamageObj ? zg.zombieDamageObj.GetComponent<ZombieDamage>() : null;
                if (zd == null || !damaged.Add(zd)) continue;

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
