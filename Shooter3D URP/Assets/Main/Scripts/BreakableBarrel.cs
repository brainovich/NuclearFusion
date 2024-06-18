using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBarrel : MonoBehaviour, IDamagable
{
    [Header("Explosion settings")]
    [SerializeField] private ParticleSystem _explosion;
    private float _radiusExplosion = 3;
    private float _explosionForceHor = 20;
    private float _explosionForceVer = 5;
    private int _damage = 20;

    [Header("Links")]
    private MeshRenderer _mesh;
    private Rigidbody _rb;
    private AudioSource _audio;

    private void Start()
    {
        _mesh = GetComponentsInChildren<MeshRenderer>()[1];
        _rb = GetComponent<Rigidbody>();
        _audio = GetComponent<AudioSource>();
    }

    public void TakeDamage(int damageAmount, Vector3 force, Vector3 hitPoint)
    {
        Explode();
    }

    private void Explode()
    {
        Collider[] bodiesInExplosionArea;
        List<IDamagable> damagableInExplosionArea = new List<IDamagable>();
        List<Vector3> damagableForceDirection = new List<Vector3>();
        Vector3 forceDirection;

        bodiesInExplosionArea = Physics.OverlapSphere(transform.position, _radiusExplosion);

        for(int j = 0; j < bodiesInExplosionArea.Length; j++)
        {
            Collider body = bodiesInExplosionArea[j];
            if (!body.gameObject.isStatic && body.gameObject != this.gameObject)
            {
                forceDirection = (body.transform.position - gameObject.transform.position).normalized * _explosionForceHor + Vector3.up * _explosionForceVer;
                Rigidbody rb = body.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(forceDirection, ForceMode.Impulse);
                }
                else
                {
                    rb = body.GetComponentInParent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(forceDirection, ForceMode.Impulse);
                    }
                }

                if (body.TryGetComponent<IDamagable>(out IDamagable damagableBody))
                {
                    damagableInExplosionArea.Add(damagableBody);
                    damagableForceDirection.Add(forceDirection);
                }
            }
        }
        this.gameObject.tag = "DestroyedObj";
        for(int i = 0; i < damagableInExplosionArea.Count; i++)
        {
            if (!damagableInExplosionArea[i].gameObject.CompareTag("DestroyedObj"))
            {
                Vector3 forceApplied = damagableForceDirection[i] * 10;
                damagableInExplosionArea[i].TakeDamage(_damage, forceApplied, Vector3.zero);
            }
        }

        _explosion.Play();
        _audio.Play();  
        _mesh.enabled = false;
        Destroy(gameObject, 3);
    }

    public void OnObjDestroy(int delay)
    {
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

    private void OnCollisionEnter(Collision collision)
    {
        if((collision.gameObject.CompareTag("Enemy") && _rb.velocity.magnitude > 3) || _rb.velocity.magnitude > 15)
        {
            Explode();
        }
    }
}
