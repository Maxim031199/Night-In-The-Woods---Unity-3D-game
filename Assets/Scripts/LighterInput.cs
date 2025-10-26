using UnityEngine;
using UnityEngine.InputSystem;

public class LighterInput : MonoBehaviour
{
    [SerializeField] private InputActionReference lighterAction; 
    private bool isOn;

    void OnEnable()
    {
        var action = lighterAction ? lighterAction.action : null;
        if (action == null) return;

        action.performed += OnToggle;
        action.Enable();
    }

    void OnDisable()
    {
        var action = lighterAction ? lighterAction.action : null;
        if (action == null) return;

        action.performed -= OnToggle;
        action.Disable();
    }

    private void OnToggle(InputAction.CallbackContext _)
    {
        isOn = !isOn;
        GameEvents.ToggleLighter(isOn);
    }
}
