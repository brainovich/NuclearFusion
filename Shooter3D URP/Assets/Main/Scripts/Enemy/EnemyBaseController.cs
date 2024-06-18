using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; //delete further

public class EnemyBaseController : MonoBehaviour
{
    [Header("Moving")]
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _runSpeed;
    private float _walkAcceleration = 1;
    private float _runAcceleration = 400;
    private Vector3 _enemyPosition;
    private bool _isFall;
    protected EnemyMover _mover;

    [Header("Attack")]
    [SerializeField] private float _atackRange;
    [SerializeField] private float _atackRate;
    [SerializeField] private float _notificationRadius;
    [SerializeField] protected int _damage;
    protected bool _canAtack = true;

    [Header("Health")]
    [SerializeField] private int _healthMax;
    private bool _isDead = false;
    private bool _wasDead = false;
    protected EnemyHealth _health;

    [Header("Field Of View")]
    private float _visionAngleRegular = 60;
    private float _visionAngleAttack = 360;
    protected Transform _player; //turn to Vector3??
    protected EnemyVision _vision;

    [Header("Patrol")]
    [SerializeField] private Transform[] _pointsToPatrol;
    private int _pointsToPatrolQty;
    private int _nextPointNumber;
    private Vector3 _nextPoint;
    private Vector3 _previousPlayerPoint;
    private float _onPointDelay = 2;
    private float _onPointTime;

    [Header("Chase")]
    private float _chaseCooldownMax = 5;
    private float _chaseCooldownCurrent;
    private bool _canContinueChasing = true;

    [Header("Looking Enemy")]
    private float _lookingTimeMax = 5;
    private float _lookingTimeCurrent;

    [Header("States")]
    private bool _stateIsSwitched = false;
    private bool _isChasing = false;
    private bool _isOnPoint = true;
    private bool _playerInSightRange;
    private bool _playerWasInSightRange = false;
    private bool _playerInAttackRange;
    public enum State
    {
        Patrol,
        Chase,
        LookingEnemy,
        Attack,
        Dead
    }
    [HideInInspector] public State CurentState { get; private set; }
    private State previousState;


    [Header("Audio effects")]
    protected EnemyAudioFX _audioFX;

    [Header("Animation effects")]
    protected EnemyVisualFX _visualFX;
    protected EnemyRagdoll _ragdoll;

    [Header("Links")]
    [SerializeField] private Transform _transformParent;
    protected EnemyEvents _events;
    private EnemyManager _enemyManager;

    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (_wasDead)
        {
            SecondaryInitialize();
        }
    }

    protected virtual void Initialize()
    {
        _mover = GetComponentInChildren<EnemyMover>();
        _health = GetComponentInChildren<EnemyHealth>();
        _vision = GetComponentInChildren<EnemyVision>();
        _audioFX = GetComponentInChildren<EnemyAudioFX>();
        _visualFX = GetComponentInChildren<EnemyVisualFX>();
        _ragdoll = GetComponentInChildren<EnemyRagdoll>();
        _events = GetComponent<EnemyEvents>();

        _mover.Initialize(_walkSpeed);
        _health.Initialize(_healthMax, _transformParent, DeathEffect);
        _visualFX.Initialize();
        _vision.Initialize(_visionAngleRegular, _visualFX.HeadBone);
        _audioFX.Initialize();
        _ragdoll.Initialize();

        _pointsToPatrolQty = _pointsToPatrol.Length;

        _enemyManager = GameManager.instance.enemyManager;
        _player = GameManager.instance.playerController.gameObject.transform;

        _events.DamageTaken += OnDamageTaken;
        _events.StateChaseContinued += ContinueChasing;
        _events.EnemyStoodUp += OnEnemyStoodUp;
        _events.EnemyFall += OnEnemyFall;
    }

    protected virtual void SecondaryInitialize()
    {
        _health.SecondaryInitialize();
        _vision.Initialize(_visionAngleRegular, _visualFX.HeadBone);
        _visualFX.Initialize();
        _ragdoll.Disable();

        OnEnemyStoodUp();

        _isDead = false;
        _playerWasInSightRange = false;
        _stateIsSwitched = false;
        _isChasing = false;
        _isOnPoint = true;
        _canAtack = true;

        _wasDead = false;
    }

    private void FixedUpdate()
    {
        _playerInAttackRange = Vector3.Distance(_enemyPosition, _player.position) <= _atackRange;
        _isDead = _health.IsDead;
        _playerInSightRange = _vision.PlayerInSightRange;
        if (_playerInSightRange && !_playerWasInSightRange)
        {
            _playerWasInSightRange = true;
        }

        _audioFX.IsMoving(_mover.IsMoving());
        _enemyPosition = _mover.transform.position;

        SwitchState();
        StateMachine();
    }

    private void SwitchState()
    {
        if (_playerInSightRange && _playerInAttackRange && !_isDead && !_isFall)
        {
            CurentState = State.Attack;
        }
        else if ((_playerInSightRange || _playerWasInSightRange) && !_isDead && !_isFall)
        {
            CurentState = State.Chase;
        }
        else if (_isChasing && !_playerInSightRange && !_isDead && !_isFall)
        {
            CurentState = State.LookingEnemy;
        }
        else if (_isDead)
        {
            CurentState = State.Dead;
        }
        else if (!_isDead && !_isFall)
        {
            CurentState = State.Patrol;
        }
    }

    private void StateMachine()
    {
        if (previousState != CurentState)
        {
            _stateIsSwitched = false;
        }
        if (!_isFall)
        {
            switch (CurentState)
            {
                case State.Patrol:
                    Patrol(_walkSpeed, _walkAcceleration);
                    break;
                case State.Chase:
                    Chase(_runSpeed, _runAcceleration);
                    break;
                case State.LookingEnemy:
                    LookingEnemy();
                    break;
                case State.Attack:
                    Attack(ResetAtack());
                    break;
            }
        }
        Debug.Log(CurentState);
        previousState = CurentState;
    }

    private void Patrol(float speed, float acceleration)
    {
        if (_stateIsSwitched)
        {
            if (_isOnPoint)
            {
                //choose new point and walk there
                _nextPointNumber = Random.Range(0, _pointsToPatrolQty);
                _nextPoint = _pointsToPatrol[_nextPointNumber].position;
                _mover.MoveTo(_nextPoint, speed, acceleration);
                _isOnPoint = false;
            }
            if (Vector3.Distance(_enemyPosition, _nextPoint) < 2)
            {
                //stop on the point and wait for certain amount of time
                _mover.Stop();
                _onPointTime += Time.deltaTime;
                if (_onPointTime >= _onPointDelay)
                {
                    _isOnPoint = true;
                    _onPointTime = 0;
                }
            }
            return;
        }
        else
        {
            _audioFX.CalmSound();
            _vision.ChangeSightRange(_visionAngleRegular);
            _stateIsSwitched = true;
        }
    }

    private void Chase(float speed, float acceleration)
    {
        if (_stateIsSwitched)
        {
            if (_playerInSightRange)
            {
                _mover.MoveTo(_player.position, speed, acceleration);
                _previousPlayerPoint = _player.position;
            }
            else
            {
                _mover.MoveTo(_previousPlayerPoint, speed, acceleration);
                _chaseCooldownCurrent += Time.deltaTime;
                _playerWasInSightRange = _chaseCooldownCurrent < _chaseCooldownMax;
                return;
            }
            _chaseCooldownCurrent = 0;
            return;
        }
        else
        {
            _isChasing = true;

            _previousPlayerPoint = _player.position;

            _playerWasInSightRange = true;

            _vision.ChangeSightRange(_visionAngleAttack);

            if (previousState != CurentState && previousState != State.Attack && !_enemyManager.PlayerInCombatArea)
            {
                _mover.Stop();

                _audioFX.DetectedSound();
                _visualFX.SetTrigger(_visualFX.detectedTriggerKey);

                _canContinueChasing = false;

                _enemyManager.NotifyEnemiesOfPlayerDetection(_health.gameObject, _notificationRadius);

            }
            if (!_canContinueChasing)
            {
                return;
            }

            _audioFX.FocusedSound();

            _stateIsSwitched = true;
        }
    }

    public void ContinueChasing() => _canContinueChasing = true; //set in animation

    public void GetNotificationOfChasing()
    {
        _previousPlayerPoint = _player.position;
        _playerWasInSightRange = true;
        previousState = State.Chase;
        CurentState = State.Chase;
        _vision.ChangeSightRange(_visionAngleAttack);
        Chase(_runSpeed, _runAcceleration);
    }

    private void LookingEnemy()
    {
        if (_stateIsSwitched)
        {
            _lookingTimeCurrent += Time.deltaTime;

            if (_lookingTimeCurrent >= _lookingTimeMax || !_mover.HasPath())
            {
                _visualFX.SetTrigger(_visualFX.stopLookingTriggerKey);
                _lookingTimeCurrent = 0;
                _isChasing = false;
                _isOnPoint = true;
            }
        }
        else
        {
            if (previousState != CurentState)
            {
                _visualFX.SetTrigger(_visualFX.startLookingTriggerKey);
            }

            if (_mover.HasPath())
            {
                if (Vector3.Distance(_enemyPosition, _previousPlayerPoint) < 2)
                {
                    _lookingTimeCurrent += Time.deltaTime;
                    if (_lookingTimeCurrent < _lookingTimeMax)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else if (!_mover.HasPath())
            {
                _lookingTimeCurrent += Time.deltaTime;
                if (_lookingTimeCurrent < _lookingTimeMax)
                {
                    return;
                }
            }

            _lookingTimeCurrent = 0;

            _previousPlayerPoint += new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
            _visualFX.SetTrigger(_visualFX.stopLookingTriggerKey);
            _stateIsSwitched = true;
        }
    }

    protected virtual void Attack(IEnumerator resetAttack)
    {
        //method for override
    }

    protected virtual void DeathEffect()
    {
        _wasDead = true;
        _ragdoll.AdjustPositionAndRotation();
    }

    private void OnDamageTaken()
    {
        Chase(_runSpeed, _runAcceleration);
        _playerWasInSightRange = true;
    }

    private void OnEnemyStoodUp()
    {
        _isOnPoint = true;
        _isFall = false;
    }

    private void OnEnemyFall()
    {
        _isFall = true;
    }

    private IEnumerator ResetAtack()
    {
        _canAtack = false;
        yield return new WaitForSeconds(_atackRate);
        _canAtack = true;
    }
}
