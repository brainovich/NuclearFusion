using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamagable
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; } = 100;
    public bool IsDead { get; private set; } = false;


    void Start()
    {
        CurrentHealth = MaxHealth;
        GameManager.instance.uiController.UpdateHealthUI(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(int damageAmount, Vector3 force, Vector3 hitPoint)
    {
        CurrentHealth -= damageAmount;
        GameManager.instance.uiController.UpdateHealthUI(CurrentHealth, MaxHealth);
        GameManager.instance.uiController.PlayDamageImpact();
        GameManager.instance.playerController.PlayImpactEffect(force, CurrentHealth <= 0);
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        GameManager.instance.playerController.PlayDeathAnimation();
        GameManager.instance.uiController.PlayDeathScreen();
    }

    public void Heal(int healAmount)
    {
        if(CurrentHealth < MaxHealth)
        {
            CurrentHealth += healAmount;
            if (healAmount > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }
    }

    public void OnObjDestroy(int timeDelay)
    {
    }
}
