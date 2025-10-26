using System;

public static class GameEvents
{
    // true = ON and false = OFF
    public static event Action<bool> LighterToggled;
    public static void ToggleLighter(bool on) => LighterToggled?.Invoke(on);
    public static event Action<int> WeaponChanged;
    public static void RaiseWeaponChanged(int id) => WeaponChanged?.Invoke(id);

    public static Action<GameState> OnStateEnter;
    public static Action<GameState> OnStateExit;
    public static Func<GameState> OnGetCurrentState;
}

