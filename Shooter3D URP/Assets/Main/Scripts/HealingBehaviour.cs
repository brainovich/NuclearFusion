using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingBehaviour : MonoBehaviour, IInteractable
{
    private int _healingAmount = 30;
    private bool _pickedUp = false;

    private AudioSource _audio;
    private MeshRenderer _mesh;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _mesh = GetComponent<MeshRenderer>();
    }

    public void OnInteraction()
    {
        PlayerHealth playerHealth = GameManager.instance.playerHealth;

        if(playerHealth.CurrentHealth < playerHealth.MaxHealth && !_pickedUp)
        {
            _pickedUp = true;

            playerHealth.Heal(_healingAmount);

            GameManager.instance.uiController.UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);

            _audio.Play();
            _mesh.enabled = false;
            Destroy(gameObject, 1);
        }
    }
}
