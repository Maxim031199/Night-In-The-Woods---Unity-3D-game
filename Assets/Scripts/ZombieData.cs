using UnityEngine;

[CreateAssetMenu(fileName = "ZombieData", menuName = "Scriptable Objects/ZombieData")]
public class ZombieData : ScriptableObject
{
    [Header("Animator")]
    public int layerIndex = 1;
    public float rotateSpeed = 2.5f;
    public float attackDistance = 1.7f;
    public string layerName = "";

    [Header("Movement / Awareness")]
    public float walkSpeed = 1.0f;
    public float alertRangeMin = 5.1f;
    public float alertRangeMax = 35f;
    public bool randomState = true;
    public float randomTiming = 5f;

    [Header("Health / VFX / SFX")]
    public int maxHealth = 100;
    public GameObject bloodSplatPrefab;
    public AudioClip hitSfx;
    public AudioClip deathSfx;

    [Header("Player Damage")]
    public int playerDamage = 3;
}
