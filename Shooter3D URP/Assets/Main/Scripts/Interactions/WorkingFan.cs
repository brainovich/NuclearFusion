using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkingFan : MonoBehaviour
{
    [Header("Switches")]
    private int _switchesToActivate = 3;
    private int _switchesActivated = 0;

    [Header("Fan logic")]
    private float _timeToTurnOffMax = 3f;
    private float _timeToTurnOffCurrent;
    private float _rotationDeree = 90;
    private float _startRotationDeree = 90;
    private int _damage = 100;
    private bool _turnOffFan = false;

    [Header("Links")]
    private AudioSource _audio;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        EventManager.SwitchActivated += CheckSwitches;
    }

    private void Update()
    {
        if (!_turnOffFan)
        {
            transform.Rotate(new Vector3(0, 0, _rotationDeree) * Time.deltaTime);
        }
        else if(_timeToTurnOffCurrent < _timeToTurnOffMax)
        {
            _timeToTurnOffCurrent += Time.deltaTime;
            _rotationDeree -= Time.deltaTime * (_rotationDeree/_timeToTurnOffMax);
            _audio.pitch = _rotationDeree / _startRotationDeree;
            transform.Rotate(new Vector3(0, 0, _rotationDeree) * Time.deltaTime);
        }
        else
        {
            _rotationDeree = 0;
            _audio.Stop();
        }
    }

    private void CheckSwitches()
    {
        _switchesActivated++;
        if(_switchesActivated >= _switchesToActivate)
        {
            _turnOffFan = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("hit smth");
        if (!_turnOffFan && other.TryGetComponent<IDamagable>(out IDamagable player) && other.CompareTag("Player"))
        {
            player.TakeDamage(_damage, -other.transform.forward * 100, Vector3.zero);
        }
    }

    private void OnDestroy()
    {
        EventManager.SwitchActivated -= CheckSwitches;
    }
}
