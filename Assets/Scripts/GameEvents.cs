using System;

public static class GameEvents
{

    public static event Action<bool> LighterToggled;
    public static void ToggleLighter(bool on) => LighterToggled?.Invoke(on);


    public static event Action<int> WeaponChanged;
    public static void RaiseWeaponChanged(int id) => WeaponChanged?.Invoke(id);
}
