using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosiveType : EnemyBaseController
{
    [Header("Explosion settings")]
    private float _radiusExplosion = 3;
    private float _explosionForceHor = 30;
    private float _explosionForceVer = 5;

    private bool _exploded = false;

    protected override void SecondaryInitialize()
    {
        base.SecondaryInitialize();
        _exploded = false;
    }

    private void Explode()
    {
        Collider[] bodiesInExplosionArea;
        List<IDamagable> damagableInExplosionArea = new List<IDamagable>();
        List<Vector3> damagableForceDirection = new List<Vector3>();
        Vector3 forceDirection;

        bodiesInExplosionArea = Physics.OverlapSphere(_mover.transform.position, _radiusExplosion);

        for (int j = 0; j < bodiesInExplosionArea.Length; j++)
        {
            Collider body = bodiesInExplosionArea[j];
            if (!body.gameObject.isStatic)
            {
                forceDirection = (body.transform.position - _mover.transform.position).normalized * _explosionForceHor + Vector3.up * _explosionForceVer;
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

        for (int i = 0; i < damagableInExplosionArea.Count; i++)
        {
            bool hurtingItself = false;

            if (damagableInExplosionArea[i].gameObject.GetComponentInParent<EnemyBaseController>() != null)
            {
                EnemyBaseController parentScript = damagableInExplosionArea[i].gameObject.GetComponentInParent<EnemyBaseController>();
                if(parentScript.gameObject.GetHashCode() == this.gameObject.GetHashCode())
                {
                    hurtingItself = true;
                }
            }
                if (!damagableInExplosionArea[i].gameObject.CompareTag("DestroyedObj") && damagableInExplosionArea[i] != null && !hurtingItself)
            {
                Vector3 forceApplied = damagableForceDirection[i] * 10;
                damagableInExplosionArea[i].TakeDamage(_damage, forceApplied, Vector3.zero);
            }
        }

        _exploded = true;
        _visualFX.DeathParticleEffect();
        _audioFX.AttackSound();
        _visualFX.Disappear();
    }
    protected override void Attack(IEnumerator resetAttack)
    {
        Explode();
        _health.SuddenDeath();
        DeathEffect();
    }

    protected override void DeathEffect()
    {
        base.DeathEffect();
        if (!_exploded)
        {
            Explode();
        }
    }
}
