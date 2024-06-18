using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDemoEvent : MonoBehaviour
{
    [SerializeField] private GameObject _soldier;
    private Rigidbody[] _soldierRbs;

    // Start is called before the first frame update
    void Start()
    {
        _soldierRbs = _soldier.GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rb in _soldierRbs)
        {
            rb.isKinematic = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (Rigidbody rb in _soldierRbs)
        {
            rb.isKinematic = false;
        }
    }
}
