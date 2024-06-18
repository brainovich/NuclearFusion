using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    public PlayerController2 playerController;
    public PlayerHealth playerHealth;
    public WeaponController weaponController;
    public UIController uiController;
    public EnemyManager enemyManager;

    public Transform player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance == this)
        {
            Destroy(gameObject);
        }
    }
}
