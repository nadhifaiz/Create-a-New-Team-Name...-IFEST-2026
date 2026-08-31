using System;
using UnityEngine;

public static class CombatFreezeSignal
{
    private static int activeFreezeCount = 0;

    public static bool IsFrozen => activeFreezeCount > 0;


    public static event Action<bool> OnFreezeStateChanged;

    public static void Begin()
    {
        activeFreezeCount++;
        if (activeFreezeCount == 1) OnFreezeStateChanged?.Invoke(true);
    }

    public static void End()
    {
        activeFreezeCount = Mathf.Max(0, activeFreezeCount - 1);
        if (activeFreezeCount == 0) OnFreezeStateChanged?.Invoke(false);
    }
}