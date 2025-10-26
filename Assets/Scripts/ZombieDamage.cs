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
    [SerializeField] float meleeHitCooldown = 0.15f;
    float nextHitTime;
    int health;
    bool death;
    private float deathDestroyDelay = 5f;

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

        health -= Mathf.Max(0, damage);

        if (health > 0)
        {
            if (data.hitSfx && damagePlayer) damagePlayer.PlayOneShot(data.hitSfx);
            zombieAnim.SetTrigger("react");
            return;
        }

        DeathSequence();
    }


    void OnTriggerEnter(Collider other)
    {
        if (death || Time.time < nextHitTime) return;

        var hit = other.GetComponent<WeaponHit>();   // holds WeaponData
        if (!hit || hit.data == null) return;

        nextHitTime = Time.time + meleeHitCooldown;

        // Damage
        health -= Mathf.Max(0, hit.data.damage);

        // Blood and VFX
        var pos = other.ClosestPoint(transform.position);
        if (bloodSplat) Instantiate(bloodSplat, pos, other.transform.rotation);

        // SFX for our use the weapons impact clip
        if (hit.data.hitSfx && damagePlayer) damagePlayer.PlayOneShot(hit.data.hitSfx);

        // React animation per weapon 
        var trig = hit.data.reactTrigger;
        if (!string.IsNullOrEmpty(trig)) zombieAnim.SetTrigger(trig);

        if (health <= 0) DeathSequence();
    }



    void DeathSequence()
    {
        if (death) return;
        death = true;

        if (agent) agent.isStopped = true;
        if (hitColliders != null)
            foreach (var c in hitColliders) if (c) c.enabled = false;

        zombieAnim.SetBool("isDead", true);
        zombieAnim.SetTrigger("dead");

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
