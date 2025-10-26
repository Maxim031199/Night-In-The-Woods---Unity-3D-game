using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySwitch : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject weaponsPanel;
    [SerializeField] private GameObject itemsPanel;
    [SerializeField] private GameObject combinePanel;  

    [Header("First Selectable (optional)")]
    [SerializeField] private Selectable weaponsFirstSelect;
    [SerializeField] private Selectable itemsFirstSelect;

    [Header("SFX (optional)")]
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private AudioClip clickSfx;

    void Start()
    {
        SwitchWeaponsOn();            // start on weapons
        if (combinePanel) combinePanel.SetActive(false);  // hide combine by default
    }

    public void SwitchItemsOn()
    {
        if (weaponsPanel) weaponsPanel.SetActive(false);
        if (itemsPanel) itemsPanel.SetActive(true);
        if (combinePanel) combinePanel.SetActive(false);
        Focus(itemsFirstSelect); PlayClick();
    }

    public void SwitchWeaponsOn()
    {
        if (weaponsPanel) weaponsPanel.SetActive(true);
        if (itemsPanel) itemsPanel.SetActive(false);
        if (combinePanel) combinePanel.SetActive(false);
        Focus(weaponsFirstSelect); PlayClick();
    }

    void Focus(Selectable first)
    { if (first && EventSystem.current) { EventSystem.current.SetSelectedGameObject(null); EventSystem.current.SetSelectedGameObject(first.gameObject); } }

    void PlayClick()
    { if (uiAudio && clickSfx) uiAudio.PlayOneShot(clickSfx); }
}
