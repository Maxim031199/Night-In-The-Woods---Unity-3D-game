using UnityEngine;
using UnityEngine.UI;


public class WeaponInventory : MonoBehaviour
{
    [Header("UI Data")]
    [SerializeField] private Sprite[] bigIcons;
    [SerializeField] private Image bigIcon;

    [SerializeField] private string[] titles;
    [SerializeField] private Text title;

    [TextArea(2, 6)]
    [SerializeField] private string[] descriptions;
    [SerializeField] private Text description;

    [Header("Inventory UI SFX (optional)")]
    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private AudioClip selectSfx;
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private float uiSfxCooldown = UiSfxCooldown;

    [Header("Links (required for equipping)")]
    [SerializeField] private WeaponManager weaponManager;

    [Header("Combine UI")]
    [SerializeField] private GameObject useButton;
    [SerializeField] private GameObject combineButton;
    [SerializeField] private GameObject combinePanel;
    [SerializeField] private GameObject combineUseButton;
    [SerializeField] private Image[] combineItems;

    [Header("Owned Items")]
    [SerializeField] private bool[] itemsOwned = new bool[DefaultItemsOwnedSize];

    [Header("Start")]
    [SerializeField] private int startIndex = 0;

    int currentIndex = -1;
    float nextUiSfxAllowed = 0f;


    private const int DefaultItemsOwnedSize = 8;
    private const int FirstCombineWeaponIndex = 6;
    private const int SecondCombineWeaponIndex = 7;
    private const int SlotAIndex = 2;
    private const int SlotBIndex = 3;

    private const float OwnedAlpha = 1f;
    private const float NotOwnedAlpha = 0.06f;
    private const float UiSfxCooldown = 0.05f;

    private const int MinIndex = 0;
    private const int Slot0 = 0;
    private const int Slot1 = 1;
    private const int EnsureMinOwnedSize = 4;
    private const int AtLeastOne = 1;

    private const float ColorOne = 1f;   // RGB=1 for white

    void Awake()
    {
        if (!weaponManager) weaponManager = FindFirstObjectByType<WeaponManager>();

        if (!uiAudio) uiAudio = GetComponent<AudioSource>();
        if (!uiAudio) uiAudio = gameObject.AddComponent<AudioSource>();
        uiAudio.playOnAwake = false;
        uiAudio.spatialBlend = 0f;
        uiAudio.ignoreListenerPause = true;

        EnsureItemsOwnedSize(EnsureMinOwnedSize);
    }

    void Start()
    {
        if (weaponManager != null && weaponManager.CurrentWeaponIndex >= MinIndex)
            startIndex = weaponManager.CurrentWeaponIndex;

        startIndex = Mathf.Clamp(startIndex, MinIndex, GetMaxCount() - 1);
        SelectIndex(startIndex);

        if (combinePanel) combinePanel.SetActive(false);
        if (combineButton) combineButton.SetActive(false);
    }

    public void SelectIndex(int index) => ChooseWeapon(index);

    public void NextWeapon()
    {
        int n = GetMaxCount();
        if (n == 0) return;
        ChooseWeapon((currentIndex + 1) % n);
    }

    public void PreviousWeapon()
    {
        int n = GetMaxCount();
        if (n == 0) return;
        ChooseWeapon((currentIndex - 1 + n) % n);
    }

    public void OnUsePressed()
    {
        PlayUiSfx(selectSfx);
        if (weaponManager && currentIndex >= 0)
            weaponManager.EquipFromInventory(currentIndex);
    }

    public void PlayClickSfx() => PlayUiSfx(clickSfx);

    public void CombineAction()
    {
        if (!combinePanel) return;
        UpdateCombineSlots();

        if (currentIndex == FirstCombineWeaponIndex)
        {
            ShowCombineSlot(Slot1, false);
            if (combineUseButton) combineUseButton.SetActive(IsOwned(SlotAIndex));
        }
        else if (currentIndex == SecondCombineWeaponIndex)
        {
            ShowCombineSlot(Slot1, true);
            if (combineUseButton) combineUseButton.SetActive(IsOwned(SlotAIndex) && IsOwned(SlotBIndex));
        }
        else
        {
            if (combineUseButton) combineUseButton.SetActive(true);
        }

        combinePanel.SetActive(true);
    }

    public void ChooseWeapon(int index)
    {
        int n = GetMaxCount();
        if (n == 0 || index < MinIndex || index >= n || index == currentIndex) return;

        PlayUiSfx(clickSfx);

        UpdateWeaponUI(index);
        currentIndex = index;

        if (combineButton) combineButton.SetActive(currentIndex >= FirstCombineWeaponIndex);

        if (currentIndex < FirstCombineWeaponIndex)
        {
            if (combinePanel) combinePanel.SetActive(false);
            if (combineButton) combineButton.SetActive(false);
        }

        if (currentIndex >= FirstCombineWeaponIndex) UpdateCombineSlots();
    }

    void UpdateWeaponUI(int index)
    {
        if (bigIcon)
        {
            var sprite = (bigIcons != null && index < bigIcons.Length) ? bigIcons[index] : null;
            bigIcon.sprite = sprite;
            bigIcon.enabled = sprite != null;
        }

        if (title) title.text = (titles != null && index < titles.Length) ? (titles[index] ?? string.Empty) : string.Empty;
        if (description) description.text = (descriptions != null && index < descriptions.Length) ? (descriptions[index] ?? string.Empty) : string.Empty;
    }

    void PlayUiSfx(AudioClip clip)
    {
        if (!clip || uiAudio == null) return;
        if (Time.unscaledTime < nextUiSfxAllowed) return;

        uiAudio.Stop();
        uiAudio.PlayOneShot(clip);
        nextUiSfxAllowed = Time.unscaledTime + uiSfxCooldown;
    }

    int GetMaxCount()
    {
        int a = bigIcons != null ? bigIcons.Length : 0;
        int b = titles != null ? titles.Length : 0;
        int c = descriptions != null ? descriptions.Length : 0;
        return Mathf.Max(AtLeastOne, Mathf.Max(a, Mathf.Max(b, c)));
    }

    void UpdateCombineSlots()
    {
        if (combineItems == null) return;
        SetCombineSlotColor(Slot0, IsOwned(SlotAIndex));
        SetCombineSlotColor(Slot1, IsOwned(SlotBIndex));

        if (currentIndex == FirstCombineWeaponIndex) ShowCombineSlot(Slot1, false);
        else if (currentIndex == SecondCombineWeaponIndex) ShowCombineSlot(Slot1, true);
    }

    bool IsOwned(int index) => itemsOwned != null && index >= 0 && index < itemsOwned.Length && itemsOwned[index];

    void SetCombineSlotColor(int slot, bool hasItem)
    {
        if (combineItems == null || slot < 0 || slot >= combineItems.Length) return;
        var img = combineItems[slot];
        if (!img) return;

        
        img.color = hasItem
            ? new Color(ColorOne, ColorOne, ColorOne, OwnedAlpha)
            : new Color(ColorOne, ColorOne, ColorOne, NotOwnedAlpha);
    }

    void ShowCombineSlot(int slot, bool visible)
    {
        if (combineItems == null || slot < 0 || slot >= combineItems.Length) return;
        var img = combineItems[slot];
        if (!img) return;
        img.transform.gameObject.SetActive(visible);
    }

    public void CombineAssignWeapon()
    {
        if (!weaponManager || currentIndex < 0) return;

        int targetIndex = currentIndex;

        if (currentIndex == FirstCombineWeaponIndex && !IsOwned(SlotAIndex)) return;
        else if (currentIndex == SecondCombineWeaponIndex && !(IsOwned(SlotAIndex) && IsOwned(SlotBIndex))) return;

        int max = weaponManager.weapons != null ? weaponManager.weapons.Length : 0;

        if (currentIndex == SecondCombineWeaponIndex)
            targetIndex = Mathf.Clamp(currentIndex + 1, MinIndex, Mathf.Max(MinIndex, max - 1));

        PlayUiSfx(selectSfx);
        weaponManager.EquipFromInventory(targetIndex);

        if (combinePanel) combinePanel.SetActive(false);
    }

    void EnsureItemsOwnedSize(int minSize)
    {
        if (itemsOwned == null) { itemsOwned = new bool[minSize]; return; }
        if (itemsOwned.Length >= minSize) return;

        var resized = new bool[minSize];
        for (int i = 0; i < itemsOwned.Length; i++) resized[i] = itemsOwned[i];
        itemsOwned = resized;
    }
}
