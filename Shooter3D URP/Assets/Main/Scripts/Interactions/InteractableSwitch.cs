using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableSwitch : MonoBehaviour, IInteractable
{
    public bool isActivated = false;
    private Animator animator;
    public WorkingFan workingFan;
    private AudioSource _audio;

    void Start()
    {
        animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
    }

    public void OnInteraction()
    {
        if(!isActivated)
        {
            isActivated = true;
            animator.SetTrigger("switchActivated");
            _audio.Play();
            EventManager.OnSwitchActivated();
        }
    }
}
