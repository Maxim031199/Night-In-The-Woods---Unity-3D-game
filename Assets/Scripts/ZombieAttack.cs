using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    [SerializeField] private ZombieData data;  // MUST be assigned
    [SerializeField] private int damageAmt = 3; // will be overridden by data
    bool canDamage = true;
    Collider col;
    Animator bloodEffect;
    AudioSource hitSound;
    void Awake()
    {
        if (!data) { enabled = false; return; }
    }
    void Start()
    {
        col = GetComponent<Collider>();
        bloodEffect = GameObject.Find("Blood")?.GetComponent<Animator>();
        hitSound = GetComponent<AudioSource>();
        damageAmt = data.playerDamage;
    }

    // Update is called once per frame
    void Update()
    {
        if (col.enabled == false)
            canDamage = true;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canDamage == true)
            {
                canDamage = false;
                if(SaveScript.health > 0)
                    SaveScript.health -= damageAmt;
                if(SaveScript.infection < 100)
                    SaveScript.infection += damageAmt;
                bloodEffect.SetTrigger("Blood");
                hitSound.Play();
            }
        }
           
    }
}
