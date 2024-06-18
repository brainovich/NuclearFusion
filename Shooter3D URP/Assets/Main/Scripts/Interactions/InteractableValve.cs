using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableValve : MonoBehaviour, IInteractable
{
    private float timeToRotateMax = 3.5f;
    private float timeToRotateCurrent;

    private AudioSource _audio;
    private bool _isAudioPlaying = false;
    private float _audioStopTimer;

    //public GameObject door;
    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }
    public void OnInteraction()
    {
        if (timeToRotateCurrent < timeToRotateMax)
        {
            EventManager.OnValveActivated();
            RotateValve();
            timeToRotateCurrent += Time.deltaTime;
            _audioStopTimer = 0;

            if (!_isAudioPlaying)
            {
                StartCoroutine(StopAudio());
                _isAudioPlaying = true;
                _audio.Play();
            }
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


    public void RotateValve()
    {
        transform.Rotate(new Vector3(0, 0, .1f));
    }

    /*public void OpenDoor()
    {
        door.transform.Translate(Vector3.up * Time.deltaTime);
    }*/
}
