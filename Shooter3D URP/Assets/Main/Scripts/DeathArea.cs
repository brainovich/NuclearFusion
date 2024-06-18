using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<IDamagable>(out IDamagable damagable) && other.CompareTag("Player"))
        {
            damagable.TakeDamage(1000, Vector3.zero, Vector3.zero);
        }
    }
}
