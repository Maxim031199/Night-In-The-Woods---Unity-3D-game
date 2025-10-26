using UnityEngine;
using UnityEngine.UI;


public class WeaponsUIManager : MonoBehaviour
{
    [SerializeField] private GameObject pistolPanel, shotgunPanel, sprayPanel;
    [SerializeField] private Text pistolTotalAmmo, pistolCurrentAmmo, shotgunTotalAmmo, shotgunCurrentAmmo;


    private const int PistolIndex = 4;
    private const int ShotgunIndex = 5;
    private const int SprayIndex = 6;

    private const int PistolAmmoType = 0;
    private const int ShotgunAmmoType = 1;

    private bool panelOn = false;

    void OnEnable()
    {
        GameEvents.WeaponChanged += OnWeaponChanged;
        OnWeaponChanged(SaveScript.weaponID);
    }

    void OnDisable()
    {
        GameEvents.WeaponChanged -= OnWeaponChanged;
    }

    void OnWeaponChanged(int id)
    {
        pistolPanel?.SetActive(id == PistolIndex);
        shotgunPanel?.SetActive(id == ShotgunIndex);
        sprayPanel?.SetActive(id == SprayIndex);

        if (pistolCurrentAmmo) pistolCurrentAmmo.text = SaveScript.currentAmmo[PistolIndex].ToString();
        if (shotgunCurrentAmmo) shotgunCurrentAmmo.text = SaveScript.currentAmmo[ShotgunIndex].ToString();
        if (pistolTotalAmmo) pistolTotalAmmo.text = SaveScript.ammoAmts[PistolAmmoType].ToString();
        if (shotgunTotalAmmo) shotgunTotalAmmo.text = SaveScript.ammoAmts[ShotgunAmmoType].ToString();
    }

    void Start()
    {
        pistolPanel.SetActive(false);
        shotgunPanel.SetActive(false);
        sprayPanel.SetActive(false);
    }

    void Update()
    {
        if (SaveScript.weaponID == PistolIndex)
        {
            if (!panelOn)
            {
                panelOn = true;
                pistolPanel.SetActive(true);
            }
        }
        if (SaveScript.weaponID == ShotgunIndex)
        {
            if (!panelOn)
            {
                panelOn = true;
                shotgunPanel.SetActive(true);
            }
        }

        if (SaveScript.weaponID == SprayIndex)
        {
            if (!panelOn)
            {
                panelOn = true;
                sprayPanel.SetActive(true);
            }
        }
        if (SaveScript.inventoryOpen)
        {
            panelOn = false;
            pistolPanel.SetActive(false);
            shotgunPanel.SetActive(false);
            sprayPanel.SetActive(false);
        }
    }

    private void OnGUI()
    {
        pistolTotalAmmo.text = SaveScript.ammoAmts[PistolAmmoType].ToString();
        shotgunTotalAmmo.text = SaveScript.ammoAmts[ShotgunAmmoType].ToString();
        pistolCurrentAmmo.text = SaveScript.currentAmmo[PistolIndex].ToString();
        shotgunCurrentAmmo.text = SaveScript.currentAmmo[ShotgunIndex].ToString();
    }
}
