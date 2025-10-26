using UnityEngine;
using UnityEngine.AI;

public class ZombieScript : MonoBehaviour
{
    public enum ZombieType { shuffle, dizzy, alert }
    public enum ZombieState { Idle, Walking, Eating }

    [Header("Data (optional)")]
    [SerializeField] private ZombieData data;

    [Header("Style & State")]
    public ZombieType zombieStyle;
    public ZombieState chooseState;
    [SerializeField] private float yAdjustment = 0.0f;

    [Header("Targets & Player Lookup")]
    [SerializeField] private string targetTag = "Target";
    [SerializeField] private string playerObjectName = "Player";
    [SerializeField] private string chaseMusicObjectName = "ChaseMusic";

    [Header("Animator Mapping")]
    
    [SerializeField] private int animatorLayerOffset = 1;

    [Header("Combat & Awareness (fallbacks used if Data is null)")]
    [SerializeField] private float fallbackAttackDistance = 2.0f;
    [SerializeField] private float fallbackRotateSpeed = 2.5f;
    [SerializeField] private float alertRangeMin = 5.1f;
    [SerializeField] private float alertRangeMax = 35f;

    [Header("Movement (fallbacks used if Data is null)")]
    
    [SerializeField] private float[] fallbackWalkSpeeds = { 0.15f, 1.0f, 0.75f };

    [Header("Random State Switching")]
    public bool randomState = false;
    [SerializeField] private float randomTiming = 5f;

    [Header("Waypoints")]
    [SerializeField] private float waypointSwitchDistance = 1.5f;

    [Header("Chase Music")]
    [SerializeField] private float chaseMusicTargetVolume = 0.4f;
    [SerializeField] private float chaseMusicFadePerSecond = 0.5f;

    [Header("Cleanup")]
    [SerializeField] private float destroyDelaySeconds = 20f;

    
    private static class AnimParams
    {
        public const string IsDead = "isDead";
        public const string Attacking = "attacking";
    }

    private Animator anim;
    private AnimatorStateInfo animInfo;
    private NavMeshAgent agent;
    private int currentState;
    private GameObject[] targets;
    private int currentTarget = 0;

    
    private float distanceToTarget;
    private float distanceToPlayer;
    private GameObject player;
    private float zombieAlertRange;
    private bool awareOfPlayer = false;
    private bool adding = true;
    private AudioSource chaseMusicPlayer;
    private float attackDistance;
    private float rotateSpeed;
    private AudioSource zombieSound;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        zombieSound = GetComponent<AudioSource>();

        targets = GameObject.FindGameObjectsWithTag(targetTag);
        var playerGO = GameObject.Find(playerObjectName);
        if (playerGO != null) player = playerGO;

        var chaseMusicGO = GameObject.Find(chaseMusicObjectName);
        if (chaseMusicGO != null) chaseMusicPlayer = chaseMusicGO.GetComponent<AudioSource>();

        if (data != null)
        {
            if (!string.IsNullOrEmpty(data.layerName))
            {
                string n = data.layerName.ToLowerInvariant();
                if (n == "shuffle") zombieStyle = ZombieType.shuffle;
                else if (n == "dizzy") zombieStyle = ZombieType.dizzy;
                else if (n == "alert") zombieStyle = ZombieType.alert;
            }

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
                    anim.SetLayerWeight(animatorLayerOffset + (int)zombieStyle, 1f);
                }
            }
            else
            {
                anim.SetLayerWeight(animatorLayerOffset + (int)zombieStyle, 1f);
            }

            rotateSpeed = data.rotateSpeed;
            attackDistance = data.attackDistance;
            zombieAlertRange = Random.Range(data.alertRangeMin, data.alertRangeMax);
            randomState = data.randomState;
            randomTiming = data.randomTiming;

            agent.speed = data.walkSpeed;
        }
        else
        {
            rotateSpeed = fallbackRotateSpeed;
            attackDistance = fallbackAttackDistance;
            zombieAlertRange = Random.Range(alertRangeMin, alertRangeMax);
            anim.SetLayerWeight(animatorLayerOffset + (int)zombieStyle, 1f);

            int styleIndex = (int)zombieStyle;
            if (fallbackWalkSpeeds != null && styleIndex >= 0 && styleIndex < fallbackWalkSpeeds.Length)
                agent.speed = fallbackWalkSpeeds[styleIndex];
        }

        if (zombieStyle == ZombieType.shuffle)
            transform.position = new Vector3(transform.position.x, transform.position.y + yAdjustment, transform.position.z);

        anim.SetTrigger(chooseState.ToString());
        currentState = (int)chooseState;

        if (randomState)
            InvokeRepeating(nameof(SetAnimState), randomTiming, randomTiming);

        if (targets != null && targets.Length > 0)
            agent.destination = targets[0].transform.position;
    }

    void Update()
    {
        if (!anim.GetBool(AnimParams.IsDead))
        {
            if (!player) return;

            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= attackDistance)
            {
                agent.isStopped = true;
                anim.SetBool(AnimParams.Attacking, true);

                Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
                Quaternion lookRot = Quaternion.LookRotation(new Vector3(dirToPlayer.x, 0, dirToPlayer.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotateSpeed * Time.deltaTime);
            }
            else
            {
                anim.SetBool(AnimParams.Attacking, false);

                if (SaveScript.zombiesChasing.Count > 0 && chaseMusicPlayer)
                {
                    if (chaseMusicPlayer.volume < chaseMusicTargetVolume)
                    {
                        if (!chaseMusicPlayer.isPlaying) chaseMusicPlayer.Play();
                        chaseMusicPlayer.volume += chaseMusicFadePerSecond * Time.deltaTime;
                        chaseMusicPlayer.volume = Mathf.Min(chaseMusicPlayer.volume, chaseMusicTargetVolume);
                    }
                }

                if (SaveScript.zombiesChasing.Count == 0 && chaseMusicPlayer)
                {
                    if (chaseMusicPlayer.volume > 0.0f)
                    {
                        chaseMusicPlayer.volume -= chaseMusicFadePerSecond * Time.deltaTime;
                        chaseMusicPlayer.volume = Mathf.Max(chaseMusicPlayer.volume, 0f);
                    }
                    if (Mathf.Approximately(chaseMusicPlayer.volume, 0f))
                        chaseMusicPlayer.Stop();
                }

                if (targets != null && targets.Length > 0)
                {
                    distanceToTarget = Vector3.Distance(transform.position, targets[currentTarget].transform.position);
                }

                int layerIndex = animatorLayerOffset + (int)zombieStyle;
                animInfo = anim.GetCurrentAnimatorStateInfo(Mathf.Clamp(layerIndex, 0, anim.layerCount - 1));

                if (distanceToPlayer < zombieAlertRange && chooseState == ZombieState.Walking)
                {
                    agent.destination = player.transform.position;
                    awareOfPlayer = true;

                    if (adding)
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
                    if (anim.IsInTransition(Mathf.Clamp(layerIndex, 0, anim.layerCount - 1)))
                        agent.isStopped = true;
                }

                if (chooseState == ZombieState.Walking && targets != null && targets.Length > 0)
                {
                    if (distanceToTarget < waypointSwitchDistance)
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

            if (SaveScript.zombiesChasing.Count == 0 && chaseMusicPlayer)
            {
                if (chaseMusicPlayer.volume > 0.0f)
                {
                    chaseMusicPlayer.volume -= chaseMusicFadePerSecond * Time.deltaTime;
                    chaseMusicPlayer.volume = Mathf.Max(chaseMusicPlayer.volume, 0f);
                }
                if (Mathf.Approximately(chaseMusicPlayer.volume, 0f))
                    chaseMusicPlayer.Stop();
            }

            Destroy(gameObject, destroyDelaySeconds);
        }
    }

    void SetAnimState()
    {
        if (!awareOfPlayer)
        {
            int statesCount = System.Enum.GetValues(typeof(ZombieState)).Length;
            int newState = Random.Range(0, statesCount);

            if (newState != currentState)
            {
                chooseState = (ZombieState)newState;
                currentState = (int)chooseState;
                anim.SetTrigger(chooseState.ToString());
            }
        }
        else
        {
            chooseState = ZombieState.Walking;
        }

        if (zombieSound) zombieSound.Play();
    }

    public void WalkOn()
    {
        agent.isStopped = false;
        if (targets != null && targets.Length > 0)
            agent.destination = targets[currentTarget].transform.position;
    }

    public void WalkOff()
    {
        agent.isStopped = true;
    }
}
