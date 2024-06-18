using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private int _damage = 20;
    private Vector3 _startPosition;
    private Collider _collider;

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        StartCoroutine(ignoreCollision());
    }

    private void OnEnable()
    {
        _startPosition = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            if (collision.transform.CompareTag("Player"))
            {
                Vector3 direction = collision.transform.position - _startPosition;
                GameManager.instance.playerHealth.TakeDamage(_damage, direction * 10, Vector3.zero);
            }

            transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
        }
    }

    private IEnumerator ignoreCollision()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(.5f);
        _collider.enabled = true;
    }
}
