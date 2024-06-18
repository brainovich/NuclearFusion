using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangeType : EnemyBaseController
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileLauncher;

    private float _avoidRange = 3;
    private bool _playerInAvoidRange = false;
    private bool _playerEnteredAvoidRange = false;
    private Vector3 _newPoint;
    private bool _movingToNewPoint = false;

    private int _projectilesPoolQty = 5;
    private List<GameObject> _projectilesPoolList;

    float minForce = 2;
    float maxForce = 5;

    protected override void Initialize()
    {
        base.Initialize();
        _events.AttackInstantiated += LaunchProjectile;
        _projectilesPoolList = new List<GameObject>();

        GameObject tmp;
        for(int i = 0; i < _projectilesPoolQty; i++)
        {
            tmp = Instantiate(_projectilePrefab);
            tmp.transform.SetParent(transform);
            tmp.SetActive(false);
            _projectilesPoolList.Add(tmp);
        }
    }

    protected override void Attack(IEnumerator resetAttack)
    {
        base.Attack(resetAttack);
        _playerInAvoidRange = Vector3.Distance(_player.transform.position, _mover.transform.position) <= _avoidRange;
        if (!_playerInAvoidRange && !_movingToNewPoint)
        {
            _mover.Stop();
            LookAtTarget();
            if (_canAtack)
            {
                _visualFX.SetTrigger("attack");
                StartCoroutine(resetAttack);
            }
            _playerEnteredAvoidRange = false;
        }
        else if (!_playerEnteredAvoidRange && !_movingToNewPoint)
        {
            Vector3 initPosition = _mover.transform.position;
            Vector3 directionToPlayer = _player.position - _mover.transform.position;
            _newPoint = _mover.transform.position - directionToPlayer.normalized * _avoidRange * 2;
            _newPoint.y = initPosition.y;
            _mover.MoveTo(_newPoint, 5, 20);

            _playerEnteredAvoidRange = true;
            _movingToNewPoint = true;
        }
        else if(_movingToNewPoint)
        {
            if(_mover.RemainingDistance() < 2)
            {
                _movingToNewPoint = false;
                _playerEnteredAvoidRange = false;
            }
        }
    }

    private void LookAtTarget()
    {
        Vector3 lookPos = _player.position - _mover.transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        _mover.transform.rotation = Quaternion.Slerp(_mover.transform.rotation, rotation, 0.2f);
    }

    private void LaunchProjectile()
    {
        GameObject projectile = GetProjectile();
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        projectile.SetActive(true);
        projectile.transform.position = _projectileLauncher.position;
        projectile.transform.rotation = _projectileLauncher.rotation;
        projectileRb.velocity = Vector3.zero;

        Vector3 playerPosition = _player.position;
        playerPosition.y += 0.5f;

        Vector3 direction = (playerPosition - _projectileLauncher.position);
        float distance = Vector3.Distance(_player.position, _projectileLauncher.position);
        float force = Mathf.Lerp(minForce, maxForce, distance / maxForce);

        projectileRb.AddForce(direction * force, ForceMode.Impulse);
    }

    private GameObject GetProjectile()
    {
        for (int i = 0; i < _projectilesPoolQty; i++)
        {
            if (!_projectilesPoolList[i].activeInHierarchy)
            {
                return _projectilesPoolList[i];
            }
        }
        return null;
    }
}
