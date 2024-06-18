using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableItem : MonoBehaviour, IDamagable
{
    [SerializeField] private GameObject _brokenItem;
    [SerializeField] private float _health;
    [SerializeField] private float _explosionForce;

    private MeshRenderer _mesh;
    private Collider _collider;
    private AudioSource _audio;
    private Rigidbody[] _pieces;

    void Start()
    {
        _brokenItem.SetActive(false);
        _mesh = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
        _audio = GetComponent<AudioSource>();
        _pieces = _brokenItem.GetComponentsInChildren<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.rigidbody.velocity.magnitude);

        if (collision.rigidbody.velocity.magnitude > 5)
        {
            Break();
        }
    }

    private void Break()
    {
        _mesh.enabled = false;
        _collider.enabled = false;
        _brokenItem.SetActive(true);

        foreach (Rigidbody rb in _pieces)
        {
            rb.AddExplosionForce(_explosionForce, transform.position, 10);
        }

        _audio.Play();
        ReturnToPool();
    }

    public void TakeDamage(int damageAmount, Vector3 force, Vector3 hitPoint)
    {
        _health -= damageAmount;
        if (_health <= 0)
        {
            Break();
        }
    }

    private void ReturnToPool()
    {
        IPoolable[] objects = gameObject.GetComponentsInChildren<IPoolable>();
        foreach (IPoolable item in objects)
        {
            item.ReturnToPool(GameManager.instance.weaponController.transform);
        }
    }

    public void OnObjDestroy(int timeDelay)
    {
    }
}
