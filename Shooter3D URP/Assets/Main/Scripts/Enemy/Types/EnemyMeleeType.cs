using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeType : EnemyBaseController
{
    private float _hitDistance = 2.5f;
    private float _distanceToPlayer;
    private bool _canMove = true;

    protected override void Initialize()
    {
        base.Initialize();
        _events.AttackInstantiated += OnAttackInstantiated;
    }

    protected override void Attack(IEnumerator resetAttack)
    {
        _distanceToPlayer = Vector3.Distance(_player.position, _mover.transform.position);
        base.Attack(resetAttack);

        if (_canMove)
        {
            _mover.MoveTo(_player.position);
        }
        else if (!_canMove)
        {
            _mover.SuddenStop();
            LookAtTarget();
            return;
        }
        if (_distanceToPlayer < _hitDistance)
        {
            _mover.SuddenStop();
            LookAtTarget();
            if (_canAtack)
            {
                _visualFX.SetTrigger("attack");
                StartCoroutine(resetAttack);
                StartCoroutine(CanMove());
            }
        }
    }

    IEnumerator CanMove()
    {
        _canMove = false;
        yield return new WaitForSeconds(2);
        _canMove = true;
    }

    private void LookAtTarget()
    {
        Vector3 lookPos = _player.position - _mover.transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        _mover.transform.rotation = Quaternion.Slerp(_mover.transform.rotation, rotation, 0.2f);
    }

    private void OnAttackInstantiated()
    {
        if(_distanceToPlayer < _hitDistance * 1.2f)
        {
            Vector3 direction = _player.position - _mover.transform.position;
            direction.y = 1;

            GameManager.instance.playerHealth.TakeDamage(_damage, direction * 70, Vector3.zero);
        }
    }
}
