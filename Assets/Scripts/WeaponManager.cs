using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponSelect { Knife, Cleaver, Bat, Axe, Pistol, Shotgun, SprayCan, Bottle, BottleWithCloth }

    [Header("Data (SO)")]
    [SerializeField] private WeaponData[] weaponData;

    [Header("Setup")]
    [SerializeField] public WeaponSelect chosenWeapon = WeaponSelect.Knife;
    [SerializeField] public GameObject[] weapons;
    [SerializeField] public GameObject sprayPanel;

    [Header("Animator")]
    [SerializeField] public Animator anim;
    [SerializeField] public string weaponIdParam = "WeaponID";
    [SerializeField] public string weaponChangedParam = "WeaponChanged";
    [SerializeField] public string attackTriggerParam = "Attack";
    [SerializeField] public float changedResetDelay = 0.5f;

    [Header("Audio")]
    private AudioSource audioPlayer;
    bool spraySoundOn;

    [Header("Weapon Offsets")]
    [SerializeField] public Vector3 defaultOffset = new(0.02f, -0.193f, 0.66f);
    [SerializeField] public Vector3 shotgunOffset = new(0.02f, -0.193f, 0.46f);

    public int CurrentWeaponIndex => weaponID;

    int weaponID = 0;
    const int SprayIndex = 6, LighterItemIndex = 2;

    void Start()
    {
        if (!anim) anim = GetComponentInChildren<Animator>();
        audioPlayer = GetComponent<AudioSource>();

        if (!audioPlayer) audioPlayer = gameObject.AddComponent<AudioSource>();
        audioPlayer.playOnAwake = false;
        audioPlayer.spatialBlend = 0f; 

        weaponID = Mathf.Clamp((int)chosenWeapon, 0, (weapons?.Length ?? 1) - 1);
        SaveScript.weaponID = weaponID;
        ChangeWeapons();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || GameUIState.InventoryOpen || Time.timeScale == 0f) return;

        if (weapons != null && weapons.Length > 0)
        {
            if (kb.xKey.wasPressedThisFrame)
            {
                SaveScript.weaponID = Mathf.Min(SaveScript.weaponID + 1, weapons.Length - 1);
                ChangeWeapons();
            }
            if (kb.zKey.wasPressedThisFrame)
            {
                SaveScript.weaponID = Mathf.Max(SaveScript.weaponID - 1, 0);
                ChangeWeapons();
            }
        }

        int id = SaveScript.weaponID;

        const int PistolIndex = 4;
        const int ShotgunIndex = 5;
        const int SprayIndexLocal = 6;

        if (id == SprayIndexLocal)
        {
            bool held = Mouse.current != null ? Mouse.current.leftButton.isPressed : Input.GetMouseButton(0);
            bool released = Mouse.current != null ? Mouse.current.leftButton.wasReleasedThisFrame : Input.GetMouseButtonUp(0);

            var spray = sprayPanel ? sprayPanel.GetComponent<SprayScript>() : null;

            if (held && spray != null && spray.sprayAmount > 0.0f)
            {
                if (!spraySoundOn)
                {
                    spraySoundOn = true;
                    anim?.SetTrigger(attackTriggerParam);
                    StartCoroutine(StartSpraySound());
                }
                return;
            }

            if ((released && spraySoundOn) || (spray != null && spray.sprayAmount <= 0.0f))
            {
                anim?.SetTrigger("Release");
                spraySoundOn = false;
                if (audioPlayer)
                {
                    audioPlayer.Stop();
                    audioPlayer.loop = false;
                }
            }

            return;
        }

        bool attackPressed = Mouse.current != null
            ? Mouse.current.leftButton.wasPressedThisFrame
            : Input.GetMouseButtonDown(0);
        if (!attackPressed) return;

        if (id < 0) return;

        bool isGun = (weaponData != null && id < weaponData.Length && weaponData[id] != null)
            ? weaponData[id].isGun
            : (id == PistolIndex || id == ShotgunIndex);

        if (!isGun)
        {
            anim?.SetTrigger(attackTriggerParam);

            // use swingSfx for melee swing
            var swingClip = (weaponData != null && id < weaponData.Length && weaponData[id] != null)
                ? weaponData[id].swingSfx
                : null;

            if (audioPlayer && swingClip)
            {
                audioPlayer.PlayOneShot(swingClip);
            }
            return;
        }

        int ammo = (SaveScript.currentAmmo != null && id < SaveScript.currentAmmo.Length)
            ? SaveScript.currentAmmo[id]
            : 0;

        if (ammo > 0)
        {
            anim?.SetTrigger(attackTriggerParam);

            var fireClip = (weaponData != null && id < weaponData.Length && weaponData[id] != null)
                ? weaponData[id].fireSfx
                : null;

            if (audioPlayer && fireClip)
            {
                audioPlayer.clip = fireClip;
                audioPlayer.Play();
            }

            SaveScript.currentAmmo[id] = Mathf.Max(0, ammo - 1);
        }
        else
        {
            var emptyClip = (weaponData != null && id < weaponData.Length && weaponData[id] != null)
                ? weaponData[id].emptySfx
                : null;

            if (audioPlayer && emptyClip) audioPlayer.PlayOneShot(emptyClip);
        }
    }

    public void EquipFromInventory(int index)
    {
        if (weapons == null || weapons.Length == 0) return;

        int clamped = Mathf.Clamp(index, 0, weapons.Length - 1);
        SaveScript.weaponID = clamped;
        ChangeWeapons();
    }

    void ChangeWeapons()
    {
        if (weapons == null || weapons.Length == 0) return;

        foreach (var w in weapons) if (w) w.SetActive(false);

        int id = SaveScript.weaponID;
        if (id >= 0 && id < weapons.Length && weapons[id])
            weapons[id].SetActive(true);

        chosenWeapon = (WeaponSelect)id;

        if (anim)
        {
            int animId = (weaponData != null && id < weaponData.Length && weaponData[id] != null)
                         ? weaponData[id].animatorId : id;
            anim.SetInteger(weaponIdParam, animId);
            anim.SetBool(weaponChangedParam, true);
            StopAllCoroutines();
            StartCoroutine(WeaponReset());
        }

        ApplyWeaponOffset();
        GameEvents.RaiseWeaponChanged(id);
        bool hasLighter = SaveScript.itemsPickedUp != null && LighterItemIndex < SaveScript.itemsPickedUp.Length && SaveScript.itemsPickedUp[LighterItemIndex];
        GameEvents.ToggleLighter(id == SprayIndex && hasLighter);
    }

    IEnumerator WeaponReset()
    {
        yield return new WaitForSeconds(changedResetDelay);
        anim?.SetBool(weaponChangedParam, false);
    }

    void ApplyWeaponOffset()
    {
        int id = SaveScript.weaponID;
        if (weaponData != null && id < weaponData.Length && weaponData[id] != null)
            transform.localPosition = weaponData[id].localOffset;
        else
            transform.localPosition = (chosenWeapon == WeaponSelect.Shotgun) ? shotgunOffset : defaultOffset;
    }

    IEnumerator StartSpraySound()
    {
        yield return new WaitForSeconds(0.3f);

        int id = SaveScript.weaponID;
        var loop = (weaponData != null && id < weaponData.Length && weaponData[id] != null)
            ? (weaponData[id].loopSfx != null ? weaponData[id].loopSfx : weaponData[id].fireSfx)
            : null;

        if (audioPlayer && loop)
        {
            audioPlayer.loop = true;
            audioPlayer.clip = loop;
            audioPlayer.Play();
        }
    }
}
