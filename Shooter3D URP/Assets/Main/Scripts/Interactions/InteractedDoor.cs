using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractedDoor : MonoBehaviour
{
    [SerializeField] private GameObject _leftDoor;
    [SerializeField] private GameObject _rightDoor;

    private AudioSource _audio;
    private bool _isAudioPlaying = false;
    private float _audioStopTimer;

    // Start is called before the first frame update
    void Awake()
    {
        EventManager.ValveActivated += OnValveActivated;
        _audio = GetComponent<AudioSource>();
    }

    private void OnValveActivated()
    {
        _leftDoor.transform.position += Vector3.forward * Time.deltaTime;
        _rightDoor.transform.position += Vector3.back * Time.deltaTime;

        _audioStopTimer = 0;

        if (!_isAudioPlaying)
        {
            _isAudioPlaying = true;
            _audio.Play();
            StartCoroutine(StopAudio());
        }
    }

    private IEnumerator StopAudio()
    {
        while (_audioStopTimer < Time.deltaTime * 1.5f)
        {
            _audioStopTimer += Time.deltaTime;
            yield return null;
        }
        _audio.Stop();
        _audioStopTimer = 0;
        _isAudioPlaying = false;
    }

    private void OnDestroy()
    {
        EventManager.ValveActivated -= OnValveActivated;
    }
}
