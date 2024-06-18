using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //delete further

public class EnemyDemo : MonoBehaviour
{
    [Header("Moving")]
    private EnemyMover _mover;
    private Vector3 _enemyPosition;
    private float _walkSpeed = 0.5f;
    private float _runSpeed = 4;
    private float _runAcceleration = 400;
    public bool _isMoving;
    public bool _isFall;

    [Header("Attack")]
    private float _atackRange = 3;
    private int _damage = 30;
    private Transform _target;

    [Header("Health")]
    private bool _isDead = false;

    [Header("Field Of View")]
    [SerializeField] private LayerMask _targetMask;
    private float _visionRadius = 3f;
    private bool seeTarget = false;

    [Header("Chase")]
    private bool _canContinueChasing = true;

    [Header("States")]
    private bool _targetInAttackRange;
    public enum State
    {
        Patrol,
        Chase,
        LookingEnemy,
        Attack,
        Dead
    }
    public State CurentState { get; private set; }
    private State _previousState;
    private bool _stateIsSwitched;

    [Header("Audio effects")]
    private EnemyAudioFX _audioFX;

    [Header("Animation effects")]
    private EnemyVisualFX _visualFX;
    private EnemyRagdoll _ragdoll;

    [Header("Links")]
    [SerializeField] Transform transformParent;
    private EnemyEvents _events;
    private EnemyManager enemyManager;


    [Header("Explosion settings")]
    public float radiusExplosion = 3;
    public float explosionForceHor = 30;
    public float explosionForceVer = 5;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _mover = GetComponentInChildren<EnemyMover>();
        _audioFX = GetComponentInChildren<EnemyAudioFX>();
        _visualFX = GetComponentInChildren<EnemyVisualFX>();
        _ragdoll = GetComponentInChildren<EnemyRagdoll>();
        _events = GetComponent<EnemyEvents>();

        _mover.Initialize(_walkSpeed);
        _audioFX.Initialize();
        _visualFX.Initialize();
        _ragdoll.Initialize();

        _isDead = false;
        _stateIsSwitched = false;

        _events.StateChaseContinued += ContinueChasing;
        _events.EnemyStoodUp += OnEnemyStoodUp;
        _events.EnemyFall += OnEnemyFall;
    }

    private void Update()
    {
        _audioFX.IsMoving(_mover.IsMoving());
        _enemyPosition = _mover.transform.position;

        Vision();
        SwitchState();
        StateMachine();

        if (_target != null)
        {
            _targetInAttackRange = Vector3.Distance(_target.transform.position, _enemyPosition) < _atackRange;
        }
        else
        {
            _targetInAttackRange = false;
        }
    }

    private void Vision()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(_enemyPosition, _visionRadius, _targetMask);
        Debug.Log("VISION WORKING");

        if (rangeChecks.Length != 0)
        {
            Debug.Log("found smth"); 
            _target = rangeChecks[0].transform;
            Vector3 directionToTarget = (_target.position - transform.position).normalized;

            Debug.DrawRay(_enemyPosition, directionToTarget, Color.red);

            seeTarget = true;
        }
        else
        {
            seeTarget = false;
        }
    }

    private void SwitchState()
    {
        if (seeTarget && _targetInAttackRange && !_isDead && !_isFall)
        {
            CurentState = State.Attack;
        }
        else if (seeTarget && !_isDead && !_isFall)
        {
            CurentState = State.Chase;
        }
        else if (_isDead)
        {
            CurentState = State.Dead;
        }
    }

    private void StateMachine()
    {
        if (_previousState != CurentState)
        {
            _stateIsSwitched = false;
        }
        if (!_isFall)
        {
            switch (CurentState)
            {
                case State.Chase:
                    Chase(_runSpeed, _runAcceleration);
                    break;
                case State.Attack:
                    Attack();
                    break;
            }
        }
        Debug.Log(CurentState);
        _previousState = CurentState;
    }

    private void Chase(float speed, float acceleration)
    {
        if (_stateIsSwitched)
        {
            Debug.Log("trying to reach");
            _mover.MoveTo(_target.position, speed, acceleration);
        }
        else
        {

            if (_previousState != CurentState)
            {
                _mover.Stop();

                _audioFX.DetectedSound();
                _visualFX.SetTrigger(_visualFX.detectedTriggerKey);

                _canContinueChasing = false;
            }
            if (!_canContinueChasing)
            {
                return;
            }

            _audioFX.FocusedSound();

            _stateIsSwitched = true;
        }
    }

    public void ContinueChasing() => _canContinueChasing = true; //set in animation!!!

    private void Attack()
    {
        Explode();
        _isDead = true;
        Destroy(transformParent.gameObject, 2);
    }

    private void Explode()
    {
        Collider[] bodiesInExplosionArea;
        List<IDamagable> damagableInExplosionArea = new List<IDamagable>();
        List<Vector3> damagableForceDirection = new List<Vector3>();
        Vector3 forceDirection;

        bodiesInExplosionArea = Physics.OverlapSphere(_mover.transform.position, radiusExplosion);
        Debug.Log(bodiesInExplosionArea.Length);

        for (int j = 0; j < bodiesInExplosionArea.Length; j++)
        {
            Collider body = bodiesInExplosionArea[j];
            if (!body.gameObject.isStatic)
            {
                forceDirection = (body.transform.position - _mover.transform.position).normalized * explosionForceHor + Vector3.up * explosionForceVer;
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
                    Debug.Log($"{damagableInExplosionArea.Count} {body.name}");
                }
            }
        }

        for (int i = 0; i < damagableInExplosionArea.Count; i++)
        {
            bool hurtingItself = false;

            if (damagableInExplosionArea[i].gameObject.GetComponentInParent<EnemyBaseController>() != null)
            {
                EnemyBaseController parentScript = damagableInExplosionArea[i].gameObject.GetComponentInParent<EnemyBaseController>();
                if (parentScript.gameObject.GetHashCode() == this.gameObject.GetHashCode())
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

        _visualFX.DeathParticleEffect();
        _audioFX.AttackSound();
        _visualFX.Disappear();
    }

    private void OnEnemyStoodUp()
    {
        //_isOnPoint = true;
        _isFall = false;
    }

    private void OnEnemyFall()
    {
        _isFall = true;
    }
}
