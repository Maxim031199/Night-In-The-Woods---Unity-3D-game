using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SprayScript : MonoBehaviour
{
    [SerializeField] protected Image sprayFill;
    [SerializeField] public float sprayAmount = 1.0f;
    [SerializeField] public float drainTime = 0.1f;


    // Update is called once per frame
    void Update()
    {
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed)
        {
            sprayAmount = Mathf.Max(0f, sprayAmount - drainTime * Time.deltaTime);
            if (sprayFill) sprayFill.fillAmount = sprayAmount;
        }
    }
}
