using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using UIImage = UnityEngine.UI.Image;
using UIText = UnityEngine.UI.Text;
using UIButton = UnityEngine.UI.Button;

public class ItemsInventory : MonoBehaviour
{
    [Header("UI Data")]
    [SerializeField] private Sprite[] bigIcons;
    [SerializeField] private UIImage bigIcon;

    [Header("Item Buttons (order = itemNumber)")]
    [SerializeField] private UIButton[] itemButtons;

    [SerializeField] private string[] titles;
    [SerializeField] private UIText title;

    [TextArea(2, 6)]
    public string[] descriptions;
    [SerializeField] private UIText description;

    [Header("Action Buttons")]
    [SerializeField] private GameObject useButton;
    [SerializeField] private GameObject combineButton;

    [Tooltip("Item indices that should show Combine instead of Use. Default: 0..3")]
    [SerializeField] private int[] forceCombineFor = new int[] { 0, 1, 2, 3 };

    [Tooltip("Item indices that should show Use even if forceCombineFor also contains them.")]
    [SerializeField] private int[] forceUseFor;

    [Tooltip("Item indices that hide both buttons (neither Use nor Combine).")]
    [SerializeField] private int[] hideBothFor;

    [Header("Inventory UI SFX (optional)")]
    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private AudioClip selectSfx;
    [SerializeField] private AudioSource uiAudio;
    [SerializeField] private float uiSfxCooldown = DefaultUiSfxCooldown;

    [Header("Links (optional)")]
    [SerializeField] private ItemsManager itemsManager;

    [Header("Events (optional)")]
    public UnityEvent<int> OnUseItem;
    public UnityEvent<int> OnCombineItem;

    [Header("Start")]
    [SerializeField] private int startItemNumber = DefaultStartItemNumber;

    private int currentItemNumber = InvalidItemNumber;
    private float nextUiSfxAllowed = 0f;

    private const float DefaultUiSfxCooldown = 0.05f;
    private const int DefaultStartItemNumber = 0;
    private const int InvalidItemNumber = -1;

    private int updateHealth;
    private float updateStamina;
    private float updateInfection;

    void Awake()
    {
        if (!itemsManager) itemsManager = FindFirstObjectByType<ItemsManager>();

        if (!uiAudio) uiAudio = GetComponent<AudioSource>();
        if (!uiAudio) uiAudio = gameObject.AddComponent<AudioSource>();
        uiAudio.playOnAwake = false;
        uiAudio.spatialBlend = 0f;
        uiAudio.ignoreListenerPause = true;

        WireItemButtons();
    }

    void Start()
    {
        int max = GetMaxCount();
        if (max == 0) return;

        startItemNumber = Mathf.Clamp(startItemNumber, 0, max - 1);
        SelectIndex(startItemNumber);
    }

    public void SelectIndex(int itemNumber) => ChooseItem(itemNumber);

    public void OnUsePressed()
    {
        PlayUiSfx(selectSfx);
        if (currentItemNumber == InvalidItemNumber) return;

        if (itemsManager && itemsManager.TryGetComponent(out ItemsManager _))
        {
            itemsManager.UseItem(currentItemNumber);
        }

        if (currentItemNumber == 7)
        {
            var wm = FindFirstObjectByType<WeaponManager>();
            if (wm) wm.EquipFromInventory(7);
        }
        OnUseItem?.Invoke(currentItemNumber);
    }

    public void OnCombinePressed()
    {
        PlayUiSfx(selectSfx);
        if (currentItemNumber == InvalidItemNumber) return;

        if (itemsManager && itemsManager.TryGetComponent(out ItemsManager _))
        {
            if (itemsManager is IItemsCombiner combiner) combiner.CombineItem(currentItemNumber);
            else if (itemsManager) itemsManager.SendMessage("CombineItem", currentItemNumber, SendMessageOptions.DontRequireReceiver);
        }
        OnCombineItem?.Invoke(currentItemNumber);
    }

    public void PlayClickSfx() => PlayUiSfx(clickSfx);

    private void ChooseItem(int itemNumber)
    {
        int max = GetMaxCount();
        if (max == 0 || itemNumber < 0 || itemNumber >= max || itemNumber == currentItemNumber) return;

        PlayUiSfx(clickSfx);
        UpdateItemUI(itemNumber);
        UpdateActionButtons(itemNumber);

        currentItemNumber = itemNumber;
    }

    private void UpdateItemUI(int itemNumber)
    {
        if (bigIcon)
        {
            var sprite = (bigIcons != null && itemNumber < bigIcons.Length) ? bigIcons[itemNumber] : null;
            bigIcon.sprite = sprite;
            bigIcon.enabled = sprite != null;
        }

        if (title)
            title.text = (titles != null && itemNumber < titles.Length) ? (titles[itemNumber] ?? "") : "";

        if (description)
            description.text = (descriptions != null && itemNumber < descriptions.Length) ? (descriptions[itemNumber] ?? "") : "";
    }

    private void UpdateActionButtons(int itemNumber)
    {
        bool hideBoth = IndexIn(itemNumber, hideBothFor);
        bool forceUse = IndexIn(itemNumber, forceUseFor);
        bool forceCombine = IndexIn(itemNumber, forceCombineFor);

        if (hideBoth)
        {
            SetGO(useButton, false);
            SetGO(combineButton, false);
            return;
        }

        if (forceUse)
        {
            SetGO(useButton, true);
            SetGO(combineButton, false);
            return;
        }

        if (forceCombine)
        {
            SetGO(useButton, false);
            SetGO(combineButton, true);
            return;
        }

        SetGO(useButton, true);
        SetGO(combineButton, false);
    }

    private void SetGO(GameObject go, bool on)
    {
        if (go && go.activeSelf != on) go.SetActive(on);
    }

    private bool IndexIn(int idx, int[] list)
    {
        if (list == null) return false;
        foreach (var i in list) if (i == idx) return true;
        return false;
    }

    private void PlayUiSfx(AudioClip clip)
    {
        if (!clip || uiAudio == null) return;
        if (Time.unscaledTime < nextUiSfxAllowed) return;

        uiAudio.Stop();
        uiAudio.PlayOneShot(clip);
        nextUiSfxAllowed = Time.unscaledTime + uiSfxCooldown;
    }

    private int GetMaxCount()
    {
        int a = bigIcons != null ? bigIcons.Length : 0;
        int b = titles != null ? titles.Length : 0;
        int c = descriptions != null ? descriptions.Length : 0;
        int d = itemButtons != null ? itemButtons.Length : 0;
        return Mathf.Max(0, Mathf.Max(a, Mathf.Max(b, Mathf.Max(c, d))));
    }

    private void WireItemButtons()
    {
        if (itemButtons == null) return;

        for (int i = 0; i < itemButtons.Length; i++)
        {
            var btn = itemButtons[i];
            if (!btn) continue;

            int itemNumber = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectIndex(itemNumber));
        }
    }

    [ContextMenu("Auto-Find Buttons Under Children")]
    private void AutoFindButtons()
    {
        itemButtons = GetComponentsInChildren<UIButton>(true);
        WireItemButtons();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

public interface IItemsCombiner
{
    void CombineItem(int itemNumber);
}
