using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    [SerializeField] private ZombieData data;
    [SerializeField] private int damageAmt = 3;

    private const int MinHealth = 0;
    private const float MaxInfection = 100f;
    private const string PlayerTag = "Player";
    private const string BloodTrigger = "Blood";
    private const string BloodObjectName = "Blood";

    private bool canDamage = true;
    private Collider col;
    private Animator bloodEffect;
    private AudioSource hitSound;

    void Awake()
    {
        if (!data) { enabled = false; return; }
    }

    void Start()
    {
        col = GetComponent<Collider>();
        bloodEffect = GameObject.Find(BloodObjectName)?.GetComponent<Animator>();
        hitSound = GetComponent<AudioSource>();
        damageAmt = data.playerDamage;
    }

    void Update()
    {
        if (!col.enabled) canDamage = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(PlayerTag)) return;
        if (!canDamage) return;

        canDamage = false;

        if (SaveScript.health > MinHealth)
            SaveScript.health -= damageAmt;

        if (SaveScript.infection < MaxInfection)
            SaveScript.infection += damageAmt;

        if (bloodEffect) bloodEffect.SetTrigger(BloodTrigger);
        if (hitSound) hitSound.Play();
    }
}
