using UnityEngine;

public class LighterScript : MonoBehaviour
{
    [SerializeField] private GameObject lighterObj;
    [SerializeField] private bool startOn = false;

    void Awake()
    {
        if (!lighterObj) lighterObj = gameObject;
        if (lighterObj) lighterObj.SetActive(startOn);
    }

    void OnEnable() => GameEvents.LighterToggled += OnToggle;
    void OnDisable() => GameEvents.LighterToggled -= OnToggle;

    private void OnToggle(bool on)
    {
        if (lighterObj) lighterObj.SetActive(on);
    }
}
