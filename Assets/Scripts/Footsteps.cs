using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
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

    private const bool playOnAwakeDisabled = false;
    private const bool loopDisabled = false;
    private const float spatialBlend2D = 0f;
    private const float resetDistance = 0f;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        if (!controller) controller = GetComponent<FPController>();

        audioSource.playOnAwake = playOnAwakeDisabled;
        audioSource.loop = loopDisabled;
        audioSource.spatialBlend = spatialBlend2D;

        lastPosition = transform.position;
    }

    void Update()
    {
        if (controller == null) return;

        if (!wasGrounded && characterController.isGrounded)
        {
            if (landClips != null && landClips.Length > 0 && Mathf.Abs(characterController.velocity.y) > minLandVelocity)
                PlayOne(landClips);

            distanceAccumulator = resetDistance;
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
        delta.y = resetDistance;
        lastPosition = currentPosition;

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = resetDistance;
        float speed = horizontalVelocity.magnitude;

        if (speed < minMoveSpeed)
        {
            distanceAccumulator = resetDistance;
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
        if (jumpClips != null && jumpClips.Length > 0)
            PlayOne(jumpClips);
    }

    void PlayOne(AudioClip[] set)
    {
        if (set == null || set.Length == 0) return;

        int index = UnityEngine.Random.Range(0, set.Length);
        float randomPitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);

        audioSource.pitch = randomPitch;
        audioSource.PlayOneShot(set[index]);
    }
}
