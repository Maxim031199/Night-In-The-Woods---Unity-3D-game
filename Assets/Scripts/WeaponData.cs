using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData" , menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Animator")]
    public int animatorId;
    public string attackTrigger = "Attack";
    public string releaseTrigger = "Release";

    [Header("Offsets")]
    public Vector3 localOffset = new(0.02f, -0.193f, 0.66f);

    [Header("Type/Stats")]
    public bool isGun = false;
    public int initialAmmo = 0;
    public float fireCooldown = 0f;

    [Header("Combat")]
    public int damage = 10;                 // melee/gun damage per hit
    public string reactTrigger = "";       
    

    [Header("Audio - Guns")]
    public AudioClip fireSfx;
    public AudioClip emptySfx;
    public AudioClip loopSfx;     // spray

    [Header("Audio - Melee")]
    public AudioClip swingSfx;     // sound at button press
    public AudioClip hitSfx;       // when we hit a zombie

    [Header("UI (optional)")]
    public string displayName;
    public Sprite icon;
}
