using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NightVisionScript : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private Image zoomBar;
    [SerializeField] private Image batteryChunks;

    [Header("Input (New System)")]
    [SerializeField] private InputActionReference zoomAction;

    [Header("Zoom")]
    [SerializeField] private float minFOV = MinFOV;
    [SerializeField] private float maxFOV = MaxFOV;
    [SerializeField] private float zoomStep = ZoomStep;

    [Header("Battery")]
    [Range(0f, 1f)] public float batteryPower = 1.0f;
    [SerializeField] private float drainTime = DrainTime;
    [SerializeField] private float drainAmountPerTick = DrainAmountPerTick;

    [Header("UI on Enable")]
    [SerializeField] private bool setBarToDefaultOnEnable = false;
    [Range(0f, 1f)] public float defaultFillOnEnable = DefaultFillOnEnable;

    private const float MinFOV = 10f;
    private const float MaxFOV = 60f;
    private const float ZoomStep = 5f;
    private const float DrainTime = 2f;
    private const float DrainAmountPerTick = 0.25f;
    private const float DefaultFillOnEnable = 0.6f;
    private const float ScrollThreshold = 0.01f;
    private const float ZoomBarDivisor = 100f;
    float startFov;

    private InputAction _runtimeZoom;

    void Awake()
    {
        if (!zoomBar) zoomBar = GameObject.Find("ZoomBar")?.GetComponent<Image>();
        if (!batteryChunks) batteryChunks = GameObject.Find("BatteryChunks")?.GetComponent<Image>();
        if (!cam)
        {
            var go = GameObject.Find("FirstPersonCharacter");
            cam = go ? go.GetComponent<Camera>() : Camera.main;
        }
    }

    void OnEnable()
    {
        // hook input
        var action = zoomAction != null ? zoomAction.action : null;
        if (action == null)
        {
            _runtimeZoom = new InputAction("Zoom", InputActionType.Value, "<Mouse>/scroll");
            action = _runtimeZoom;
        }

        action.performed += OnZoomPerformed;
        action.Enable();

        if (setBarToDefaultOnEnable && zoomBar)
            zoomBar.fillAmount = Mathf.Clamp01(defaultFillOnEnable);
        else
            UpdateZoomBarImmediate();
    }


    void OnDisable()
    {
        var action = zoomAction != null ? zoomAction.action : _runtimeZoom;
        if (action != null)
        {
            action.performed -= OnZoomPerformed;
            action.Disable();
        }
        if (_runtimeZoom != null) { _runtimeZoom.Dispose(); _runtimeZoom = null; }
        cam.fieldOfView = startFov;
    }

    void Start()
    {
        startFov = cam.fieldOfView;
        UpdateZoomBarImmediate();
        if (batteryChunks) batteryChunks.fillAmount = Mathf.Clamp01(batteryPower);
        InvokeRepeating(nameof(BatteryDrain), drainTime, drainTime);
    }

    void Update()
    {
        if (batteryChunks) batteryChunks.fillAmount = Mathf.Clamp01(batteryPower);
    }

    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        if (!cam) return;
        float y = ctx.ReadValue<Vector2>().y;
        if (Mathf.Abs(y) < ScrollThreshold) return;

        if (y > 0f && cam.fieldOfView > minFOV)
            cam.fieldOfView = Mathf.Max(minFOV, cam.fieldOfView - zoomStep);
        else if (y < 0f && cam.fieldOfView < maxFOV)
            cam.fieldOfView = Mathf.Min(maxFOV, cam.fieldOfView + zoomStep);

        UpdateZoomBarImmediate();
    }

    void BatteryDrain()
    {
        if (batteryPower > 0f)
            batteryPower = Mathf.Max(0f, batteryPower - drainAmountPerTick);
    }

    void UpdateZoomBarImmediate()
    {
        if (!zoomBar || !cam) return;
        zoomBar.fillAmount = Mathf.Clamp01(cam.fieldOfView / ZoomBarDivisor);
    }
}