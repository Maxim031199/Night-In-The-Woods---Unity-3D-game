using UnityEngine;
using TMPro;

public class GameLogic : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text counter;
    [SerializeField] public int totalPages = DefaultTotalPages;

    private const string CounterObjectName = "PageCounter";


    private const int DefaultTotalPages = 8;
    private const int MinPages = 0;
    private const int DefaultAddAmount = 1;

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
        PageCount = MinPages;
        UpdateLabel();
    }

    public void AddPage(int amount = DefaultAddAmount)
    {
        PageCount = Mathf.Clamp(PageCount + amount, MinPages, totalPages);
        UpdateLabel();
    }

    public void ResetPages()
    {
        PageCount = MinPages;
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (!counter) return;
        counter.text = $"{PageCount}/{totalPages}";
    }
}
