using UnityEngine;

public class TransitionBase : MonoBehaviour
{
    protected GameState sourceState;
    [SerializeField] protected GameState targetState;
    public GameState TargetState { get { return targetState; } }

    protected virtual void Awake()
    {
        sourceState = GetComponentInParent<GameState>();
        if (sourceState == null)
        {
            Debug.LogError($"Source state of {name} is null");
        }
        if (targetState == null)
        {
            Debug.LogError($"Target state of {name} is null");
        }
    }

    public virtual bool ShouldTransition()
    {
        return sourceState.isCurrentState && !sourceState.wasTransitionedInto;
    }
}
