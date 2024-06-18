using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private LayerMask _ignoreMask;
    private Transform _head;

    private float _visionRadius = 20;
    private float _visionAngle;

    public bool PlayerInSightRange { get; private set; }

    public void Initialize(float visionAngle, Transform head)
    {
        ChangeSightRange(visionAngle);
        _head = head;
    }

    private void FixedUpdate()
    {
        FieldOfViewCheck();
    }

    private void FieldOfViewCheck()
    {
        if (!GameManager.instance.enemyManager.PlayerInCombatArea)
        {
            Collider[] rangeChecks = Physics.OverlapSphere(transform.position, _visionRadius, _targetMask);

            if (rangeChecks.Length != 0)
            {
                Transform target = rangeChecks[0].transform;
                Vector3 directionToTarget = (target.position - _head.transform.position).normalized;

                Debug.DrawRay(_head.transform.position, directionToTarget, Color.red);

                if (Vector3.Angle(_head.forward, directionToTarget) < _visionAngle / 2)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                    if (Physics.Raycast(_head.transform.position, directionToTarget, distanceToTarget, ~_ignoreMask) && 
                        Physics.Raycast(_head.transform.position, directionToTarget, distanceToTarget, _targetMask) &&
                        !Physics.Raycast(_head.transform.position, directionToTarget, distanceToTarget, _obstacleMask))
                    {
                        PlayerInSightRange = true;
                    }
                    else
                    {
                        PlayerInSightRange = false;
                    }
                }
                else
                {
                    PlayerInSightRange = false;
                }

            }
            else if (PlayerInSightRange)
            {
                PlayerInSightRange = false;
            }
        }
        else
        {
            PlayerInSightRange = true;
        }
    }

    public void ChangeSightRange(float angle)
    {
        _visionAngle = angle;
    }
}
