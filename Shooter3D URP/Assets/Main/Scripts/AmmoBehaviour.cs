using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBehaviour : MonoBehaviour, IInteractable
{
    [SerializeField] private int _ammoQty;
    [SerializeField] private int _weaponType;
    private bool _pickedUp = false;
    private AudioSource _audio;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    public void OnInteraction()
    {
        if (!_pickedUp)
        {
            GameObject weapon = GameManager.instance.weaponController.CurrentWeapon;

            if (weapon != null && weapon.TryGetComponent<WeaponBase>(out WeaponBase weaponBase))
            {
                _pickedUp = true;

                weaponBase.ReplenishAmmo(_ammoQty);

                Physics.IgnoreCollision(GameManager.instance.playerController.GetComponentInChildren<Collider>(), GetComponent<Collider>());
                MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mesh in meshes)
                {
                    mesh.enabled = false;
                }
                _audio.Play();
                Destroy(gameObject, 1); // temporary. Later return to pool
            }
        }
    }
}
