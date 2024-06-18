using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamagable
{
    [Header("Health")]
    private int _healthCurr;
    private int _healthMax;

    [Header("Death")]
    private int _deathEffectDelay = 5;
    private Action _deathCallback;
    [HideInInspector] public bool IsDead { get; private set; }

    [Header("Links")]
    [SerializeField] private Transform _enemyManager;
    private Transform _parent;
    private EnemyEvents _events;

    public void Initialize(int healthMax, Transform parent, Action deathCallback)
    {
        _events = GetComponentInParent<EnemyEvents>();
        _healthMax = healthMax;
        _healthCurr = _healthMax;
        _parent = parent;
        _deathCallback = deathCallback;
        IsDead = false;
        GameManager.instance.enemyManager.AddEnemyToList(this.gameObject);
    }

    public void SecondaryInitialize()
    {
        _healthCurr = _healthMax;
        IsDead = false;
        GameManager.instance.enemyManager.AddEnemyToList(this.gameObject);
    }

    public void TakeDamage(int damageAmount, Vector3 force, Vector3 hitPoint)
    {
        _healthCurr -= damageAmount;
        _events.OnDamageTaken();
        if (_healthCurr <= 0f && !IsDead)
        {
            IsDead = true;
            _events.OnEnemyDied(force, hitPoint);
            OnObjDestroy(_deathEffectDelay);
            GameManager.instance.enemyManager.RemoveEnemyFromList(this.gameObject);
        }
    }

    private void ReturnToPool()
    {
        _parent.SetParent(_enemyManager);
        _parent.gameObject.SetActive(false);
    }
    public void SuddenDeath()
    {
        GameManager.instance.enemyManager.RemoveEnemyFromList(this.gameObject);
        IsDead = true;
        Invoke("ReturnToPool", _deathEffectDelay / 2);
    }
    public void OnObjDestroy(int delayInSeconds)
    {
        StartCoroutine(HandleEnemyDeath(delayInSeconds / 2));
    }

    private IEnumerator HandleEnemyDeath(int delayInSeconds)
    {

        yield return new WaitForSeconds(delayInSeconds);
        _deathCallback?.Invoke();
        yield return new WaitForSeconds(delayInSeconds);
        ReturnToPool();
    }
    private void OnDisable()
    {
        this.gameObject.transform.localPosition = Vector3.zero;
        IPoolable[] objects = gameObject.GetComponentsInChildren<IPoolable>();
        foreach (var item in objects)
        {
            item.ReturnToPool(GameManager.instance.weaponController.transform);
        }
    }
}
