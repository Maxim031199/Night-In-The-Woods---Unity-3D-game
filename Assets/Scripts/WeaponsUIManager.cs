using UnityEngine;
using UnityEngine.UI;

public class WeaponsUIManager : MonoBehaviour
{
    [SerializeField] private GameObject pistolPanel, shotgunPanel, sprayPanel;
    [SerializeField] private Text pistolTotalAmmo, pistolCurrentAmmo, shotgunTotalAmmo, shotgunCurrentAmmo;
    private bool panelOn = false;
    void OnEnable()
    {
        GameEvents.WeaponChanged += OnWeaponChanged;
        OnWeaponChanged(SaveScript.weaponID); // initial sync
    }
    void OnDisable()
    {
        GameEvents.WeaponChanged -= OnWeaponChanged;
    }
    void OnWeaponChanged(int id)
    {
        // toggle panels
        pistolPanel?.SetActive(id == 4);
        shotgunPanel?.SetActive(id == 5);
        sprayPanel?.SetActive(id == 6);

        if (pistolCurrentAmmo) pistolCurrentAmmo.text = SaveScript.currentAmmo[4].ToString();
        if (shotgunCurrentAmmo) shotgunCurrentAmmo.text = SaveScript.currentAmmo[5].ToString();
        if (pistolTotalAmmo) pistolTotalAmmo.text = SaveScript.ammoAmts[0].ToString();
        if (shotgunTotalAmmo) shotgunTotalAmmo.text = SaveScript.ammoAmts[1].ToString();
    }

    void Start()
    {
        pistolPanel.SetActive(false);
        shotgunPanel.SetActive(false);
        sprayPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(SaveScript.weaponID == 4)
        {
            if(panelOn == false)
            {
                panelOn = true;
                pistolPanel.SetActive(true);
            }
        }
        if (SaveScript.weaponID == 5)
        {
            if (panelOn == false)
            {
                panelOn = true;
                shotgunPanel.SetActive(true);
            }
        }

        if (SaveScript.weaponID == 6)
        {
            if (panelOn == false)
            {
                panelOn = true;
                sprayPanel.SetActive(true);
            }
        }
        if (SaveScript.inventoryOpen == true)
        {
            panelOn = false;
            pistolPanel.SetActive(false);
            shotgunPanel.SetActive(false);
            sprayPanel.SetActive(false);
        }
    }
    private void OnGUI()
    {
        pistolTotalAmmo.text = SaveScript.ammoAmts[0].ToString();
        shotgunTotalAmmo.text = SaveScript.ammoAmts[1].ToString();
        pistolCurrentAmmo.text = SaveScript.currentAmmo[4].ToString();
        shotgunCurrentAmmo.text = SaveScript.currentAmmo[5].ToString();


    }
}
