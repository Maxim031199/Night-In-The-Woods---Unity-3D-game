using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class ZombieDamage : MonoBehaviour
{
    [SerializeField] private ZombieData data;
    [SerializeField] private GameObject bloodSplat;
    [SerializeField] private Collider[] hitColliders;

    Animator zombieAnim;
    AudioSource damagePlayer;
    NavMeshAgent agent;

    [Header("Hit / Death")]
    [SerializeField, Min(0f)] float meleeHitCooldown = 0.15f;
    [SerializeField, Min(0f)] float deathDestroyDelay = DefaultDeathDestroyDelay;

    float nextHitTime;
    int health;
    bool death;


    private const int ZeroInt = 0;
    private const float DefaultDeathDestroyDelay = 5f;

    private static class AnimParams
    {
        public const string React = "react";
        public const string IsDead = "isDead";
        public const string Dead = "dead";
    }

    void OnValidate()
    {
        if (!data) Debug.LogError($"{name}: ZombieData is required.", this);
    }

    void Awake()
    {
        if (!data) { enabled = false; return; }
    }

    void Start()
    {
        zombieAnim = GetComponentInParent<Animator>();
        damagePlayer = GetComponent<AudioSource>();
        agent = GetComponentInParent<NavMeshAgent>();

        health = data.maxHealth;
        if (!bloodSplat && data.bloodSplatPrefab) bloodSplat = data.bloodSplatPrefab;
    }

    public void gunDamage(Vector3 hitPoint, int damage)
    {
        if (death) return;

        if (bloodSplat) Instantiate(bloodSplat, hitPoint, Quaternion.identity);

        health -= Mathf.Max(ZeroInt, damage);

        if (health > ZeroInt)
        {
            if (data.hitSfx && damagePlayer) damagePlayer.PlayOneShot(data.hitSfx);
            if (zombieAnim) zombieAnim.SetTrigger(AnimParams.React);
            return;
        }

        DeathSequence();
    }

    void OnTriggerEnter(Collider other)
    {
        if (death || Time.time < nextHitTime) return;

        var hit = other.GetComponent<WeaponHit>();
        if (!hit || hit.data == null) return;

        nextHitTime = Time.time + meleeHitCooldown;

        // Damage
        health -= Mathf.Max(ZeroInt, hit.data.damage);

        // Blood and VFX
        var pos = other.ClosestPoint(transform.position);
        if (bloodSplat) Instantiate(bloodSplat, pos, other.transform.rotation);

        // SFX for our use the weapons impact clip
        if (hit.data.hitSfx && damagePlayer) damagePlayer.PlayOneShot(hit.data.hitSfx);

        // React animation per weapon 
        var trig = hit.data.reactTrigger;
        if (!string.IsNullOrEmpty(trig)) zombieAnim.SetTrigger(trig);

        if (health <= ZeroInt) DeathSequence();
    }

    void DeathSequence()
    {
        if (death) return;
        death = true;

        if (agent) agent.isStopped = true;
        if (hitColliders != null)
            foreach (var c in hitColliders) if (c) c.enabled = false;

        if (zombieAnim)
        {
            zombieAnim.SetBool(AnimParams.IsDead, true);
            zombieAnim.SetTrigger(AnimParams.Dead);
        }

        if (data.deathSfx && damagePlayer) damagePlayer.PlayOneShot(data.deathSfx);

        // Let the death anim play and after that then destroy
        Invoke(nameof(OnDeathAnimComplete), deathDestroyDelay);
    }

    public void OnDeathAnimComplete()
    {
        // destroy the whole zombie root 
        Destroy(transform.root.gameObject);
    }
}