using UnityEngine;
using UnityEngine.InputSystem;


public class FPController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] public InputActionReference moveAction;
    [SerializeField] public InputActionReference lookAction;
    [SerializeField] public InputActionReference jumpAction;
    [SerializeField] public InputActionReference sprintAction;
    [SerializeField] public InputActionReference crouchAction;

    [Header("Refs")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Footsteps footsteps;

    [Header("Move")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6.5f;
    [SerializeField] private float jumpPower = 5f;
    [SerializeField] private float gravity = -14f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 120f;
    [SerializeField] private float pitchClamp = 80f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private float standHeight = 1.8f;
    [SerializeField] private float crouchLerp = 12f;
    [SerializeField] private float crouchSpeedMultiplier = 0.6f;

    [Header("Zoom")]
    [SerializeField] private float normalFOV = 60f;

    private CharacterController cc;
    private float yVel;
    private float pitch;
    private float targetHeight;
    public static float FPSstamina = 100;
    private float runSpeedAmt;


    private const float InitialGroundVelocity = -1f;
    private const float CrouchHeightClampMin = 0.9f;
    private const float CrouchHeightClampOffset = 0.2f;
    private const float CenterHeightFactor = 0.5f;
    private const float SmoothingBase = 1f;
    private const float Zero = 0f;
    private const float LowStaminaThreshold = 20f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!playerCamera) playerCamera = GetComponentInChildren<Camera>();
        if (!footsteps) footsteps = GetComponent<Footsteps>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerCamera) playerCamera.fieldOfView = normalFOV;

        standHeight = Mathf.Max(standHeight, crouchHeight);
        crouchHeight = Mathf.Clamp(crouchHeight, CrouchHeightClampMin, standHeight - CrouchHeightClampOffset);
        cc.height = standHeight;
        cc.center = new Vector3(cc.center.x, standHeight * CenterHeightFactor, cc.center.z);
        targetHeight = standHeight;
        runSpeedAmt = sprintSpeed;
    }

    void OnEnable()
    {
        if (moveAction) moveAction.action.Enable();
        if (lookAction) lookAction.action.Enable();
        if (jumpAction) jumpAction.action.Enable();
        if (sprintAction) sprintAction.action.Enable();
        if (crouchAction) crouchAction.action.Enable();
    }

    void OnDisable()
    {
        if (moveAction) moveAction.action.Disable();
        if (lookAction) lookAction.action.Disable();
        if (jumpAction) jumpAction.action.Disable();
        if (sprintAction) sprintAction.action.Disable();
        if (crouchAction) crouchAction.action.Disable();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        Vector2 look = lookAction ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;


        transform.Rotate(Vector3.up * (look.x * lookSensitivity * dt));


        pitch = Mathf.Clamp(pitch - look.y * lookSensitivity * dt, -pitchClamp, pitchClamp);
        if (playerCamera) playerCamera.transform.localRotation = Quaternion.Euler(pitch, Zero, Zero);

        bool isCrouching =
            (crouchAction && crouchAction.action.IsPressed()) ||
            (Keyboard.current != null && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed));

        Vector2 move = moveAction ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        Vector3 wish = (transform.right * move.x + transform.forward * move.y).normalized;

        float speed = (sprintAction && sprintAction.action.IsPressed()) ? sprintSpeed : walkSpeed;


        if (FPSstamina < LowStaminaThreshold)
            sprintSpeed = walkSpeed;
        else
            sprintSpeed = runSpeedAmt;

        if (isCrouching) speed *= crouchSpeedMultiplier;

        if (cc.isGrounded)
        {
            yVel = InitialGroundVelocity;
            if (jumpAction && jumpAction.action.WasPressedThisFrame() && !isCrouching)
            {
                yVel = jumpPower;
                if (footsteps) footsteps.PlayJump();
            }
        }
        else
        {
            yVel += gravity * dt;
        }

        cc.Move((wish * speed + Vector3.up * yVel) * dt);

        targetHeight = isCrouching ? crouchHeight : standHeight;
        float k = SmoothingBase - Mathf.Exp(-crouchLerp * dt);
        cc.height = Mathf.Lerp(cc.height, targetHeight, k);
        Vector3 c = cc.center;
        c.y = Mathf.Lerp(c.y, cc.height * CenterHeightFactor, k);
        cc.center = c;
    }
}
