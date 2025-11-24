using UnityEngine;

public class PlayerAnimatorState : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float normalSpeed = 1f;

    void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();

        ApplyState(RuntimeGameState.Current);

        RuntimeGameState.OnStateChanged += HandleStateChanged;
    }

    void OnDestroy()
    {
        RuntimeGameState.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(RuntimeState newState)
    {
        ApplyState(newState);
    }

    void ApplyState(RuntimeState state)
    {
        if (!animator) return;

        if (RuntimeGameState.IsGameplayActive)
            animator.speed = normalSpeed;
        else
            animator.speed = 0f;
    }
}
