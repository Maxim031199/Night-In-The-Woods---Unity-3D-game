using System;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Starting Values")]
    [Range(0, 100)] public int startHealth = 100;
    [Range(0f, 100f)] public float startInfection = 0f;

    [Header("Runtime (read-only)")]
    [SerializeField] int health;
    [SerializeField] float infection; 

    public int Health => health;
    public float Infection => infection;

    public event Action<int> OnHealthChanged;
    public event Action<float> OnInfectionChanged;
    public event Action OnDead;
    public event Action OnInfectionMax;

    void Awake()
    {
        health = Mathf.Clamp(startHealth, 0, 100);
        infection = Mathf.Clamp(startInfection, 0f, 100f);
    }


    void Start()
    {
        if (health <= 0) OnDead?.Invoke();
        if (infection >= 100f) OnInfectionMax?.Invoke();
    }


    public void Damage(int amount)
    {
        if (amount <= 0 || health <= 0) return;
        SetHealth(health - amount);
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || health <= 0) return;
        SetHealth(health + amount);
    }

    public void AddInfection(float amount)
    {
        if (amount <= 0f || infection >= 100f) return;
        SetInfection(infection + amount);
    }

    public void ReduceInfection(float amount)
    {
        if (amount <= 0f) return;
        SetInfection(infection - amount);
    }


    public void SetHealth(int value)
    {
        int clamped = Mathf.Clamp(value, 0, 100);
        if (clamped == health) return;

        health = clamped;
        OnHealthChanged?.Invoke(health);
        if (health == 0) OnDead?.Invoke();
    }

    public void SetInfection(float value)
    {
        float clamped = Mathf.Clamp(value, 0f, 100f);
        if (Mathf.Approximately(clamped, infection)) return;

        infection = clamped;
        OnInfectionChanged?.Invoke(infection);
        if (infection >= 100f) OnInfectionMax?.Invoke();
    }


    void Update()
    {
        if (health != SaveScript.health)
            SetHealth(SaveScript.health);

        if (!Mathf.Approximately(infection, SaveScript.infection))
            SetInfection(SaveScript.infection);
    }

    public void Kill() => SetHealth(0);
    public void MaxInfection() => SetInfection(100f);
}
