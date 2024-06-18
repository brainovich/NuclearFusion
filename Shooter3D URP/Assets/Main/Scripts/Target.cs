using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour, IDamagable
{
    private float _health = 10f;

    public void TakeDamage(int damageAmount, Vector3 force, Vector3 hitPoint)
    {
        _health -= damageAmount;
        if (_health <= 0f)
        {
            OnObjDestroy(0);
        }
    }

    public void OnObjDestroy(int delay)
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        IPoolable[] objects = gameObject.GetComponentsInChildren<IPoolable>();
        foreach (IPoolable item in objects)
        {
            Debug.Log(objects.Length);
            item.ReturnToPool(GameManager.instance.weaponController.transform);
        }
    }
}
