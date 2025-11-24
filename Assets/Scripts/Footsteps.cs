using UnityEngine;

public class Footsteps : MonoBehaviour
{
    [SerializeField] private FPController controller;


    [Header("Step Sounds")]
    [SerializeField] private AudioClip[] stepClips;
    [SerializeField] private float walkStepDistance = 2.1f;
    [SerializeField] private float sprintDistanceMultiplier = 0.8f;
    [SerializeField] private float crouchDistanceMultiplier = 1.4f;

    [Header("Jump / Land")]
    [SerializeField] private AudioClip[] jumpClips;
    [SerializeField] private AudioClip[] landClips;
    [SerializeField] private float minLandVelocity = 3f;

    [Header("Tuning")]
    [SerializeField] private float minMoveSpeed = 0.2f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private CharacterController characterController;
    private AudioSource audioSource;

    private Vector3 lastPosition;
    private float distanceAccumulator;
    private bool wasGrounded;

    const bool PlayOnAwakeDisabled = false;
    const bool LoopDisabled = false;
    const float SpatialBlend2D = 0f;
    const float ResetDistance = 0f;
    const int FirstClipIndex = 0;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        if (!controller) controller = GetComponent<FPController>();

        if (audioSource)
        {
            audioSource.playOnAwake = PlayOnAwakeDisabled;
            audioSource.loop = LoopDisabled;
            audioSource.spatialBlend = SpatialBlend2D;
        }

        lastPosition = transform.position;

        if (!characterController) Debug.LogError("Footsteps needs a CharacterController on the same GameObject.");
        if (!audioSource) Debug.LogError("Footsteps needs an AudioSource on the same GameObject.");
    }

    void Update()
    {
        if (controller == null || characterController == null || audioSource == null) return;

        if (!wasGrounded && characterController.isGrounded)
        {
            if (landClips != null && landClips.Length > 0 && Mathf.Abs(characterController.velocity.y) > minLandVelocity)
                PlayOne(landClips);

            distanceAccumulator = ResetDistance;
            lastPosition = transform.position;
        }
        wasGrounded = characterController.isGrounded;

        if (!characterController.isGrounded || stepClips == null || stepClips.Length == 0)
        {
            lastPosition = transform.position;
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;
        delta.y = ResetDistance;
        lastPosition = currentPosition;

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = ResetDistance;
        float speed = horizontalVelocity.magnitude;

        if (speed < minMoveSpeed)
        {
            distanceAccumulator = ResetDistance;
            return;
        }

        distanceAccumulator += delta.magnitude;

        float stepDistance = walkStepDistance;
        bool sprinting = controller.sprintAction && controller.sprintAction.action.IsPressed();
        bool crouching = controller.crouchAction && controller.crouchAction.action.IsPressed();

        if (sprinting) stepDistance *= sprintDistanceMultiplier;
        if (crouching) stepDistance *= crouchDistanceMultiplier;

        if (distanceAccumulator >= stepDistance)
        {
            distanceAccumulator -= stepDistance;
            PlayOne(stepClips);
        }
    }

    public void PlayJump()
    {
        if (audioSource == null) return;
        if (jumpClips != null && jumpClips.Length > 0)
            PlayOne(jumpClips);
    }

    void PlayOne(AudioClip[] set)
    {
        if (audioSource == null || set == null || set.Length == 0) return;

        int index = Random.Range(FirstClipIndex, set.Length);
        float randomPitch = Random.Range(pitchRange.x, pitchRange.y);

        audioSource.pitch = randomPitch;
        audioSource.PlayOneShot(set[index]);
    }
}