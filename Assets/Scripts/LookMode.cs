using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

public static class GameUIState
{
    public static bool InventoryOpen;
}


public class LookMode : MonoBehaviour
{
    [Header("Post-process")]
    [SerializeField] private PostProcessVolume volume;
    [SerializeField] private PostProcessProfile standard;
    [SerializeField] private PostProcessProfile nightVision;
    [SerializeField] private PostProcessProfile inventory;

    [Header("Overlays / UI")]
    [SerializeField] private GameObject nightVisionOverlay;
    [SerializeField] private GameObject flashlightOverlay;
    [SerializeField] private GameObject inventoryUI;

    [Header("Flashlight")]
    [SerializeField] private Light flashLight;
    [SerializeField] private bool disableFlashlightWhenNVOn = false;

    [Header("Camera")]
    [SerializeField] private float defaultFOV = DefaultFOV;

    [Header("SFX (optional)")]
    [SerializeField] private AudioClip nvOnClip, nvOffClip, flOnClip, flOffClip;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [SerializeField] float toggleCooldown = ToggleCooldown;

    [Header("Night Vision Boot")]
    public AudioClip nvBootClip;
    public float nvBootDelaySeconds = NvBootDelaySeconds;

    [Header("Inventory SFX")]
    [SerializeField] private AudioClip invOnClip;
    [SerializeField] private AudioClip invOffClip;

    [Header("Inventory Options")]
    [SerializeField] private bool disableNVWhenInventoryOn = true;
    [SerializeField] private bool disableFlashlightWhenInventoryOn = true;
    [SerializeField] private bool pauseOnInventory = true;
    [SerializeField] private bool pauseAudioOnInventory = true;
    [SerializeField] private bool hardPauseAllAudioSources = true;
    [SerializeField] private MonoBehaviour[] pauseWhileInventory;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string gameplayActionMap = "Player";
    [SerializeField] private string uiActionMap = "UI";

    private bool nightVisionOn = false;
    private bool flashLightOn = false;
    private bool inventoryOn = false;

    private bool prevNV = false;
    private bool prevFlash = false;

    private float nextNVToggleAllowed = 0f;
    private float nextFLToggleAllowed = 0f;
    private float nextInvToggleAllowed = 0f;

    private bool nvBooting = false;

    private AudioSource audioSrc;
    private Camera cam;
    private NightVisionScript nvUI;
    private FlashLightScript flUI;

    private readonly List<AudioSource> pausedSources = new();
    private readonly Dictionary<AudioSource, bool> prevIgnoreListenerPause = new();


    private const float DefaultFOV = 60f;
    private const float ToggleCooldown = 0.25f;
    private const float NvBootDelaySeconds = 4f;

    private const float PausedTimeScale = 0f;
    private const float RunningTimeScale = 1f;
    private const float Zero = 0f;

    [SerializeField] private GameObject pointer;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!volume) volume = GetComponent<PostProcessVolume>();

        if (nightVisionOverlay)
        {
            nightVisionOverlay.SetActive(false);
            nvUI = nightVisionOverlay.GetComponent<NightVisionScript>();
        }
        if (flashlightOverlay)
        {
            flashlightOverlay.SetActive(false);
            flUI = flashlightOverlay.GetComponent<FlashLightScript>();
        }

        if (!flashLight)
        {
            var go = GameObject.Find("FlashLight");
            if (go) flashLight = go.GetComponent<Light>();
        }
        if (flashLight) flashLight.enabled = false;

        if (inventoryUI) inventoryUI.SetActive(false);

        audioSrc = GetComponent<AudioSource>();
        if (!audioSrc) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        audioSrc.spatialBlend = Zero;
        audioSrc.ignoreListenerPause = true;

        ForceNightVision(false, playSfx: false);
        ForceFlashlight(false, playSfx: false);
        ForceInventory(false);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.nKey.wasPressedThisFrame && Time.unscaledTime >= nextNVToggleAllowed)
        {
            if (!nvBooting && !inventoryOn)
            {
                if (nightVisionOn) ForceNightVision(false, true);
                else
                {
                    StartCoroutine(NightVisionFirstBootRoutine());
                }
            }
            nextNVToggleAllowed = Time.unscaledTime + toggleCooldown;
        }

        if (kb.fKey.wasPressedThisFrame && Time.unscaledTime >= nextFLToggleAllowed)
        {
            if (!inventoryOn)
            {
                if (flashLightOn) ForceFlashlight(false, true);
                else ForceFlashlight(true, true);
            }
            nextFLToggleAllowed = Time.unscaledTime + toggleCooldown;
        }

        if (kb.iKey.wasPressedThisFrame && Time.unscaledTime >= nextInvToggleAllowed)
        {
            ForceInventory(!inventoryOn);
            nextInvToggleAllowed = Time.unscaledTime + toggleCooldown;
        }

        if (nightVisionOn && nvUI != null && nvUI.batteryPower <= Zero)
            ForceNightVision(false, true);
        if (flashLightOn && flUI != null && flUI.batteryPower <= Zero)
            ForceFlashlight(false, true);

        if (SaveScript.inventoryOpen)
        {
            Cursor.visible = true;
            if (pointer) pointer.SetActive(false);
        }
        else
        {
            Cursor.visible = false;
            if (pointer) pointer.SetActive(true);
        }
    }

    System.Collections.IEnumerator NightVisionFirstBootRoutine()
    {
        nvBooting = true;

        if (disableFlashlightWhenNVOn && flashLightOn)
            ForceFlashlight(false, false);

        float wait = nvBootClip ? nvBootClip.length : nvBootDelaySeconds;
        if (nvBootClip) { audioSrc.Stop(); audioSrc.PlayOneShot(nvBootClip, sfxVolume); }
        if (wait > Zero) yield return new WaitForSecondsRealtime(wait);

        if (nvUI != null && nvUI.batteryPower <= Zero)
        {
            nvBooting = false;
            yield break;
        }

        ForceNightVision(true, false);
        nvBooting = false;
    }

    void ApplyCurrentProfile()
    {
        if (!volume) return;
        if (inventoryOn && inventory) volume.profile = inventory;
        else if (nightVisionOn && nightVision) volume.profile = nightVision;
        else if (standard) volume.profile = standard;
    }

    void ForceInventory(bool on)
    {
        if (on)
        {
            prevNV = nightVisionOn;
            prevFlash = flashLightOn;

            if (disableNVWhenInventoryOn && nightVisionOn) ForceNightVision(false, false);
            if (disableFlashlightWhenInventoryOn && flashLightOn) ForceFlashlight(false, false);

            inventoryOn = true;
            GameUIState.InventoryOpen = true;

            ApplyCurrentProfile();

            if (inventoryUI) inventoryUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pauseOnInventory) Time.timeScale = PausedTimeScale;
            if (pauseAudioOnInventory) AudioListener.pause = true;
            if (hardPauseAllAudioSources) HardPauseAllAudio();

            if (pauseWhileInventory != null)
                foreach (var mb in pauseWhileInventory) if (mb) mb.enabled = false;

            if (playerInput && !string.IsNullOrEmpty(uiActionMap))
                playerInput.SwitchCurrentActionMap(uiActionMap);

            if (invOnClip) { audioSrc.Stop(); audioSrc.PlayOneShot(invOnClip, sfxVolume); }
        }
        else
        {
            inventoryOn = false;
            GameUIState.InventoryOpen = false;

            if (disableNVWhenInventoryOn && prevNV) ForceNightVision(true, false);
            if (disableFlashlightWhenInventoryOn && prevFlash) ForceFlashlight(true, false);

            ApplyCurrentProfile();

            if (inventoryUI) inventoryUI.SetActive(false);
            if (cam) cam.fieldOfView = defaultFOV;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (hardPauseAllAudioSources) HardResumeAllAudio();
            if (pauseAudioOnInventory) AudioListener.pause = false;
            if (pauseOnInventory) Time.timeScale = RunningTimeScale;

            if (pauseWhileInventory != null)
                foreach (var mb in pauseWhileInventory) if (mb) mb.enabled = true;

            if (playerInput && !string.IsNullOrEmpty(gameplayActionMap))
                playerInput.SwitchCurrentActionMap(gameplayActionMap);

            if (invOffClip) { audioSrc.Stop(); audioSrc.PlayOneShot(invOffClip, sfxVolume); }
        }
    }

    void HardPauseAllAudio()
    {
        pausedSources.Clear();
        prevIgnoreListenerPause.Clear();

        var all = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var src in all)
        {
            if (src == audioSrc) continue;

            prevIgnoreListenerPause[src] = src.ignoreListenerPause;
            src.ignoreListenerPause = false;

            if (src.isPlaying)
            {
                src.Pause();
                pausedSources.Add(src);
            }
        }
    }

    void HardResumeAllAudio()
    {
        foreach (var src in pausedSources)
        {
            if (src) src.UnPause();
        }
        foreach (var kv in prevIgnoreListenerPause)
        {
            if (kv.Key) kv.Key.ignoreListenerPause = kv.Value;
        }
        pausedSources.Clear();
        prevIgnoreListenerPause.Clear();
    }

    void ForceNightVision(bool on, bool playSfx)
    {
        if (on && disableFlashlightWhenNVOn && flashLightOn)
            ForceFlashlight(false, false);

        nightVisionOn = on;
        ApplyCurrentProfile();

        if (nightVisionOverlay) nightVisionOverlay.SetActive(on);
        if (!on && cam) cam.fieldOfView = defaultFOV;

        if (playSfx)
        {
            if (on && nvOnClip) { audioSrc.Stop(); audioSrc.PlayOneShot(nvOnClip, sfxVolume); }
            if (!on && nvOffClip) { audioSrc.Stop(); audioSrc.PlayOneShot(nvOffClip, sfxVolume); }
        }
    }

    void ForceFlashlight(bool on, bool playSfx)
    {
        flashLightOn = on;

        if (flashlightOverlay) flashlightOverlay.SetActive(on);
        if (flashLight) flashLight.enabled = on;

        if (flUI != null)
        {
            if (on) flUI.StartDrain();
            else flUI.StopDrain();
        }

        if (playSfx)
        {
            if (on && flOnClip) { audioSrc.Stop(); audioSrc.PlayOneShot(flOnClip, sfxVolume); }
            if (!on && flOffClip) { audioSrc.Stop(); audioSrc.PlayOneShot(flOffClip, sfxVolume); }
        }
    }
}
