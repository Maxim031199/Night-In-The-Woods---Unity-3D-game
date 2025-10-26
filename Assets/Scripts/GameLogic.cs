using UnityEngine;
using TMPro;

public class GameLogic : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text counter;
    [SerializeField] public int totalPages = 8;

    private const string CounterObjectName = "PageCounter";

    public int PageCount { get; private set; }

    void Awake()
    {
        if (!counter)
        {
            var go = GameObject.Find(CounterObjectName);
            if (go) counter = go.GetComponent<TMP_Text>();
        }
    }

    void Start()
    {
        PageCount = 0;
        UpdateLabel();
    }

    public void AddPage(int amount = 1)
    {
        PageCount = Mathf.Clamp(PageCount + amount, 0, totalPages);
        UpdateLabel();
    }

    public void ResetPages()
    {
        PageCount = 0;
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (!counter) return;
        counter.text = $"{PageCount}/{totalPages}";
    }
}
