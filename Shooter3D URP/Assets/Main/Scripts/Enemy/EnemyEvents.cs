using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEvents : MonoBehaviour
{
    public event Action DamageTaken;
    public event Action<Vector3, Vector3> EnemyDied;
    public event Action EnemyFall;
    public event Action EnemyStoodUp;
    public event Action StateChaseContinued;
    public event Action AttackInstantiated;

    public void OnDamageTaken()
    {
        DamageTaken?.Invoke();
    }

    public void OnEnemyDied(Vector3 force, Vector3 hitPoint)
    {
        EnemyDied?.Invoke(force, hitPoint);
        OnEnemyFall();
    }

    public void OnEnemyFall()
    {
        EnemyFall?.Invoke();
    }

    public void OnEnemyStoodUp()
    {
        EnemyStoodUp?.Invoke();
    }

    public void OnStateChaseContinued()
    {
        StateChaseContinued?.Invoke();
    }

    public void OnAttackInstantiated()
    {
        AttackInstantiated?.Invoke();
    }
}
