using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMover : MonoBehaviour
{
    [Header("Links")]
    private NavMeshAgent _agent;
    private EnemyEvents _events;

    public void Initialize(float speed)
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = speed;
        _events = GetComponentInParent<EnemyEvents>();

        _events.EnemyFall += SuddenStop;
    }
    public void Warp(Vector3 position) => _agent.Warp(position);
    public float GetSpeed() => _agent.velocity.magnitude;
    public float RemainingDistance() => _agent.remainingDistance;
    public bool IsMoving() => _agent.velocity.magnitude > 0;
    public bool HasPath() => _agent.hasPath;

    public void MoveTo(Vector3 point, float speed, float acceleration)
    {
        _agent.isStopped = false;
        _agent.destination = point;
        _agent.speed = speed;
        _agent.acceleration = acceleration;
    }

    public void MoveTo(Vector3 point, float speed)
    {
        _agent.isStopped = false;
        _agent.destination = point;
        _agent.speed = speed;
    }

    public void MoveTo(Vector3 point)
    {
        _agent.isStopped = false;
        _agent.destination = point;
    }

    public void Stop()
    {
        _agent.isStopped = true;
    }

    public void SuddenStop()
    {
        _agent.acceleration = 1000;
        _agent.isStopped = true;
    }
}
