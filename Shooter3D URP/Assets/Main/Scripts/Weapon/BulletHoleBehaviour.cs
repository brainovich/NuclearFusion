using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoleBehaviour : MonoBehaviour, IPoolable
{
    private Transform _originalParent;

    private void Start()
    {
        _originalParent = GameManager.instance.weaponController.transform;
    }

    private void OnEnable()
    {
        StartCoroutine(ReturnBullet());
    }

    public void ReturnToPool(Transform originalParent)
    {
        transform.SetParent(originalParent);
        transform.position = originalParent.position;
        Restore();
    }

    public void Restore()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator ReturnBullet()
    {
        yield return new WaitForSeconds(2);
        ReturnToPool(_originalParent);
    }
}
