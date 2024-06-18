using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager: MonoBehaviour
{
    public static event Action ValveActivated;
    public static event Action SwitchActivated;

    public static void OnValveActivated()
    {
        ValveActivated?.Invoke();
    }

    public static void OnSwitchActivated()
    {
        SwitchActivated?.Invoke();
    }

    private void OnDestroy()
    {
        ValveActivated = null;
        SwitchActivated = null;
    }
}
