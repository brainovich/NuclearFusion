using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.instance.playerHealth.IsDead == false)
        {
            GameManager.instance.uiController.PlayEndOfLevelScreen();
            GameManager.instance.weaponController.TurnOffAllWeapons();
            GameManager.instance.uiController.TurnOnCursor();
        }
    }
}
