using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnemyAudioFX : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioClip[] stepsSounds;
    [SerializeField] private AudioClip calmSounds;
    [SerializeField] private AudioClip focusedSounds;
    [SerializeField] private AudioClip detectedSound;
    [SerializeField] private AudioClip attackSound;

    [Header("Sounds properties")]
    [SerializeField] private float _stepsGapWalk;
    [SerializeField] private float _stepsGapRun;
    private float _stepsGapCurrent;
    private bool _isMoving;

    [Header("Links")]
    private AudioSource _mainSource;
    private AudioSource _stepsSource;
    private EnemyBaseController _base;
    private EnemyEvents _events;

    public void IsMoving(bool isMoving) { _isMoving = isMoving; }

    public void Initialize()
    {
        _stepsSource = GetComponents<AudioSource>()[0];
        _mainSource = GetComponents<AudioSource>()[1];
        _base = GetComponentInParent<EnemyBaseController>();
        _events = GetComponentInParent<EnemyEvents>();

        _events.EnemyFall += SuddenStop;
    }

    void Update()
    {
        if (_isMoving)
        {
            _stepsGapCurrent += Time.deltaTime;
        }
        if(_base != null)
        {
            StepsStateMachine();
        }
    }

    public void StepsStateMachine()
    {
        switch (_base.CurentState)
        {
            case EnemyBaseController.State.Patrol:
                StepsSound(_stepsGapWalk);
                break;
            case EnemyBaseController.State.Chase:
                StepsSound(_stepsGapRun);
                break;
        }
    }

    private void StepsSound(float soundsGap)
    {
        if (_stepsGapCurrent >= soundsGap)
        {
            _stepsSource.clip = stepsSounds[Random.Range(0, stepsSounds.Length)];
            _stepsSource.Play();
            _stepsGapCurrent = 0;
        }
    }

    private void SuddenStop()
    {
        _mainSource.Stop();
        _stepsSource.Stop();
    }

    public void CalmSound()
    {
        _mainSource.clip = calmSounds;
        _mainSource.loop = true;
        _mainSource.Play();
    }

    public void FocusedSound()
    {
        _mainSource.clip = focusedSounds;
        _mainSource.loop = true;
        _mainSource.Play();
    }

    public void DetectedSound()
    {
        _mainSource.clip = detectedSound;
        _mainSource.loop = false;
        _mainSource.Play();
    }

    public void AttackSound()
    {
        _stepsSource.clip = null;
        _mainSource.clip = attackSound;
        _mainSource.loop = false;
        _mainSource.Play();
        Debug.Log("explosion");
    }
}