using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieScript : MonoBehaviour
{
    public enum ZombieType
    {
        shuffle,
        dizzy,
        alert
    }
    public enum ZombieState
    {
        Idle,
        Walking,
        Eating
    }

    [SerializeField] private ZombieData data;

    public ZombieType zombieStyle;
    public ZombieState chooseState;
    public float yAdjustment = 0.0f;
    private Animator anim;
    private AnimatorStateInfo animInfo;
    private NavMeshAgent agent;
    public bool randomState = false;
    public float randomTiming = 5f;
    private int newState = 0;
    private int currentState;
    private GameObject[] targets;
    private float[] walkSpeed = { 0.15f, 1.0f, 0.75f };
    private float distanceToTarget;
    private int currentTarget = 0;
    private float distanceToPlayer;
    private GameObject player;
    private float zombieAlertRange = 20f;
    private bool awareOfPlayer = false;
    private bool adding = true;
    private AudioSource chaseMusicPlayer;
    private float attackDistance = 2.0f;
    private float rotateSpeed = 2.5f;
    private AudioSource zombieSound;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        zombieSound = GetComponent<AudioSource>();
        targets = GameObject.FindGameObjectsWithTag("Target");
        player = GameObject.Find("Player");
        chaseMusicPlayer = GameObject.Find("ChaseMusic").GetComponent<AudioSource>();
        if (data != null)
        {
            if (!string.IsNullOrEmpty(data.layerName))
            {
                string n = data.layerName.ToLowerInvariant();
                if (n == "shuffle") zombieStyle = ZombieType.shuffle;
                else if (n == "dizzy") zombieStyle = ZombieType.dizzy;
                else if (n == "alert") zombieStyle = ZombieType.alert;
            }

            // Use animator layer by its NAME
            if (!string.IsNullOrEmpty(data.layerName) && anim)
            {
                int active = anim.GetLayerIndex(data.layerName);
                if (active >= 0)
                {
                    for (int i = 0; i < anim.layerCount; i++)
                        anim.SetLayerWeight(i, i == active ? 1f : 0f);
                }
                else
                {
                    // fallback to old behavior if name not found
                    anim.SetLayerWeight(((int)zombieStyle + 1), 1);
                }
            }
            else
            {
                // old behavior
                anim.SetLayerWeight(((int)zombieStyle + 1), 1);
            }

            // Pull tunables from SO
            rotateSpeed = data.rotateSpeed;
            attackDistance = data.attackDistance;
            zombieAlertRange = Random.Range(data.alertRangeMin, data.alertRangeMax);
            randomState = data.randomState;
            randomTiming = data.randomTiming;

            // Movement speed
            agent.speed = data.walkSpeed;
        }
        else
        {
            // Original defaults 
            zombieAlertRange = Random.Range(5.1f, 35f);
            anim.SetLayerWeight(((int)zombieStyle + 1), 1);
            agent.speed = walkSpeed[(int)zombieStyle];
        }


        if (zombieStyle == ZombieType.shuffle)
            transform.position = new Vector3(transform.position.x, transform.position.y + yAdjustment, transform.position.z);

        anim.SetTrigger(chooseState.ToString());
        currentState = (int)chooseState;

        if (randomState)
            InvokeRepeating("SetAnimState", randomTiming, randomTiming);

        if (targets != null && targets.Length > 0)
            agent.destination = targets[0].transform.position; ;
    }

    // Update is called once per frame
    void Update()
    {
        if (anim.GetBool("isDead") == false)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= attackDistance)
            {
                agent.isStopped = true;
                anim.SetBool("attacking", true);

                Vector3 pos = (player.transform.position - transform.position).normalized;

                Quaternion posRotation = Quaternion.LookRotation(new Vector3(pos.x, 0, pos.z));

                transform.rotation = Quaternion.Slerp(transform.rotation, posRotation, rotateSpeed * Time.deltaTime);
            }
            else
            {
                anim.SetBool("attacking", false);

                if (SaveScript.zombiesChasing.Count > 0)
                {
                    if (chaseMusicPlayer.volume < 0.4f)
                    {
                        if (chaseMusicPlayer.isPlaying == false)
                        {
                            chaseMusicPlayer.Play();
                        }
                        chaseMusicPlayer.volume += 0.5f * Time.deltaTime;
                    }
                }
                if (SaveScript.zombiesChasing.Count == 0)
                {
                    if (chaseMusicPlayer.volume > 0.0f)
                    {
                        chaseMusicPlayer.volume -= 0.5f * Time.deltaTime;
                    }
                    if (chaseMusicPlayer.volume == 0.0f)
                    {
                        chaseMusicPlayer.Stop();
                    }
                }

                distanceToTarget = Vector3.Distance(transform.position, targets[currentTarget].transform.position);

                animInfo = anim.GetCurrentAnimatorStateInfo((int)zombieStyle);

                if (distanceToPlayer < zombieAlertRange && chooseState == ZombieState.Walking)
                {
                    agent.destination = player.transform.position;
                    awareOfPlayer = true;
                    if (adding == true)
                    {
                        if (SaveScript.zombiesChasing.Contains(this.gameObject))
                        {
                            adding = false;
                            return;
                        }
                        else
                        {
                            SaveScript.zombiesChasing.Add(this.gameObject);
                            adding = false;
                        }
                    }
                }
                if (distanceToPlayer > zombieAlertRange)
                {
                    awareOfPlayer = false;
                    if (SaveScript.zombiesChasing.Contains(this.gameObject))
                    {
                        SaveScript.zombiesChasing.Remove(this.gameObject);
                        adding = true;
                    }
                }

                if (animInfo.IsTag("motion"))
                {
                    if (anim.IsInTransition((int)zombieStyle))
                    {
                        agent.isStopped = true;
                    }
                }

                if (chooseState == ZombieState.Walking)
                {
                    if (distanceToTarget < 1.5f)
                    {
                        if (currentTarget < targets.Length - 1)
                        {
                            currentTarget = Random.Range(0, targets.Length);
                        }
                    }
                }
            }
        }
        else
        {
            if (SaveScript.zombiesChasing.Contains(this.gameObject))
            {
                SaveScript.zombiesChasing.Remove(this.gameObject);
                adding = true;
            }

            if (SaveScript.zombiesChasing.Count == 0)
            {
                if (chaseMusicPlayer.volume > 0.0f)
                {
                    chaseMusicPlayer.volume -= 0.5f * Time.deltaTime;
                }
                if (chaseMusicPlayer.volume == 0.0f)
                {
                    chaseMusicPlayer.Stop();
                }
            }

            Destroy(gameObject, 20);
        }
    }

    void SetAnimState()
    {
        if (awareOfPlayer == false)
        {
            newState = Random.Range(0, 3);
            if (newState != currentState)
            {
                chooseState = (ZombieState)newState;
                currentState = (int)chooseState;
                anim.SetTrigger(chooseState.ToString());
            }
        }
        if (awareOfPlayer == true)
        {
            chooseState = ZombieState.Walking;
        }
        zombieSound.Play();
    }

    public void WalkOn()
    {
        agent.isStopped = false;
        agent.destination = targets[currentTarget].transform.position;
    }

    public void WalkOff()
    {
        agent.isStopped = true;
    }
}
