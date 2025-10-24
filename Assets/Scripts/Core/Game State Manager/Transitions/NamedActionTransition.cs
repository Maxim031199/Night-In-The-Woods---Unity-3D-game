using UnityEngine;

public class NamedActionTransition : TransitionBase
{
    [SerializeField] public string actionName;
    bool actionDone;

    public void DoAction() => actionDone = true;

    public override bool ShouldTransition()
    {
        if (base.ShouldTransition() && actionDone)
        {
            actionDone = false;
            return true;
        }
        actionDone = false;
        return false;
    }
}
