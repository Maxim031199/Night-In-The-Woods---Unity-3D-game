using System;

public enum RuntimeState
{
    None,
    Playing,
    Paused,
    Inventory,
    Win,
    GameOver
}

public static class RuntimeGameState
{
    public static RuntimeState Current { get; private set; } = RuntimeState.Playing;

    public static event Action<RuntimeState> OnStateChanged;

    public static void SetState(RuntimeState newState)
    {
        if (newState == Current) return;
        Current = newState;
        OnStateChanged?.Invoke(newState);
    }


    public static bool IsGameplayActive =>
        Current == RuntimeState.Playing;


    public static bool IsFrozen =>
        Current == RuntimeState.Paused ||
        Current == RuntimeState.Inventory ||
        Current == RuntimeState.Win ||
        Current == RuntimeState.GameOver;
}