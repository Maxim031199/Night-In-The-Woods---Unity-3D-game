using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SlenderManAI : MonoBehaviour
{
    [SerializeField] public Transform player;
    [SerializeField] public float teleportDistance = TeleportDistance;
    [SerializeField] public float teleportCooldown = TeleportCooldown;
    [SerializeField] public float returnCooldown = ReturnCooldown;
    [Range(0f, 1f)] public float chaseProbability = ChaseProbability;
    [SerializeField] public float rotationSpeed = RotationSpeed;

    [SerializeField] public AudioClip teleportSound;
    AudioSource audioSource;

    [SerializeField] public GameObject staticObject;
    [SerializeField] public float staticActivationRange = StaticActivationRange;

    [Header("Grounding")]
    [SerializeField] public LayerMask groundMask = ~0;
    [SerializeField] public float groundRayHeight = GroundRayHeight;
    [SerializeField] public float footRadius = FootRadius;
    [SerializeField] public float extraFootClearance = ExtraFootClearance;

    Vector3 baseTeleportSpot;
    float timer;

    CharacterController cc;
    Rigidbody rb;
    Collider col;

    private const float DefaultFootOffset = 0.9f;
    private const float TeleportDistance = 10f;
    private const float TeleportCooldown = 5f;
    private const float ReturnCooldown = 10f;
    private const float ChaseProbability = 0.65f;
    private const float RotationSpeed = 5f;
    private const float StaticActivationRange = 5f;
    private const float GroundRayHeight = 60f;
    private const float FootRadius = 0.28f;
    private const float ExtraFootClearance = 0.12f;

    private const float GroundSnapEpsilon = 0.0001f;
    private const float Zero = 0f;
    private const float YAxisZero = 0f;
    private const float RayHeightToCastDistance = 2f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Start()
    {
        baseTeleportSpot = transform.position;
        timer = teleportCooldown;

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (staticObject)
            staticObject.SetActive(false);
    }

    void Update()
    {
        if (RuntimeGameState.Current != RuntimeState.Playing)
            return;

        if (!player) return;

        timer -= Time.deltaTime;
        if (timer <= Zero)
            DecideTeleportAction();

        RotateTowardsPlayer();

        float sqr = (transform.position - player.position).sqrMagnitude;
        bool shouldStatic = sqr <= staticActivationRange * staticActivationRange;

        if (staticObject && staticObject.activeSelf != shouldStatic)
            staticObject.SetActive(shouldStatic);
    }

    void DecideTeleportAction()
    {
        if (Random.value <= chaseProbability)
            TeleportNearPlayer();
        else
            TeleportToBaseSpot();
    }

    void TeleportNearPlayer()
    {
        Vector2 dir2 = Random.insideUnitCircle.normalized * teleportDistance;
        Vector3 candidate = player.position + new Vector3(dir2.x, YAxisZero, dir2.y);
        TeleportSafely(candidate);
        timer = teleportCooldown;
    }

    void TeleportToBaseSpot()
    {
        TeleportSafely(baseTeleportSpot);
        timer = returnCooldown;
    }

    void TeleportSafely(Vector3 target)
    {
        Vector3 snapped = SnapToGround(target, groundRayHeight, groundMask);
        float foot = GetFootOffset();
        snapped.y += foot + extraFootClearance;

        if (cc)
        {
            bool was = cc.enabled;
            cc.enabled = false;
            transform.position = snapped;
            cc.enabled = was;
        }
        else if (rb)
        {
            if (col) col.enabled = false;
            rb.position = snapped;
            rb.linearVelocity = Vector3.zero;
            if (col) col.enabled = true;
        }
        else
        {
            transform.position = snapped;
        }

        if (teleportSound)
            audioSource.PlayOneShot(teleportSound);
    }

    float GetFootOffset()
    {
        if (cc) return Mathf.Max(Zero, cc.height * 0.5f + cc.center.y) * transform.lossyScale.y;
        if (col) return col.bounds.extents.y;
        return DefaultFootOffset;
    }

    Vector3 SnapToGround(Vector3 point, float rayHeight, LayerMask mask)
    {
        Vector3 from = point + Vector3.up * rayHeight;
        float castDist = rayHeight * RayHeightToCastDistance;

        if (Physics.SphereCast(from, footRadius, Vector3.down, out RaycastHit hit, castDist, mask, QueryTriggerInteraction.Ignore))
            point.y = hit.point.y;

        return point;
    }

    void RotateTowardsPlayer()
    {
        Vector3 to = player.position - transform.position;
        to.y = YAxisZero;

        if (to.sqrMagnitude > GroundSnapEpsilon)
        {
            Quaternion target = Quaternion.LookRotation(to);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }
    }
}