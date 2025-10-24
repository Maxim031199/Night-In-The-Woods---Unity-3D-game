using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public StateSO stateData;
    public bool isCurrentState;
    public GameState previousState;
    GameState nextState;
    List<TransitionBase> transitions = new();
    public bool wasTransitionedInto;
    public bool inTransition;

    void Start()
    {
        transitions.AddRange(GetComponentsInChildren<TransitionBase>());
    }

    void Update()
    {
        if (!isCurrentState) { return; }

        nextState = null;
        foreach (var transition in transitions.Where(x => x.ShouldTransition()))
        {
            if (transition.TargetState != null)
            {
                nextState = transition.TargetState;
                break;
            }
        }

        if (nextState != null && !inTransition)
        {
            inTransition = true;
            StateExit();
            inTransition = false;
        }

        if (wasTransitionedInto)
        {
            wasTransitionedInto = false;
        }
    }

    private void StateExit()
    {
        isCurrentState = false;
        GameEvents.OnStateExit?.Invoke(this);
        nextState.StateEnter(this);
    }

    private void StateEnter(GameState previousState)
    {
        this.previousState = previousState;
        isCurrentState = true;
        GameEvents.OnStateEnter?.Invoke(this);
    }
}
