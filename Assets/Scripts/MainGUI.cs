using UnityEngine;
using UnityEngine.UI;

public class MainGUI : MonoBehaviour
{
    [SerializeField] private Text healthAmt;
    [SerializeField] private Text staminaAmt;
    [SerializeField] private Text infectionAmt;

    

    void Update()
    {
        healthAmt.text = SaveScript.health + "%";
        staminaAmt.text = SaveScript.stamina.ToString("F0") + "%";
        infectionAmt.text = SaveScript.infection.ToString("F0") + "%";
    }
}
