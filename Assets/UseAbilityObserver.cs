using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseAbilityObserver : MonoBehaviour, Observer
{
    private CharacterMovement player;
    private void Awake()
    {
        
        player = GetComponent<CharacterMovement>();
        foreach (ButtonBeingObserved button in FindObjectsOfType<ButtonBeingObserved>())
        {
            button.AddObserver(this);
        }
    }

    public void OnNotify(Collectible obj, NotificationType notificationType)
    {
        // not used
    }

    public void OnNotifyUI(int i, NotificationType notificationType)
    {
        // not used
    }

    public void OnNotifyButtonClick(NotificationType notificationType)
    {
        //Debug.Log("im ");
        switch (notificationType)
        {
            case NotificationType.FirePickup:
                StartCoroutine(Fire());
                break;
            case NotificationType.FreezePickup :
               Freeze();
                break;
            case NotificationType.SpeedPickup:
                StartCoroutine(SpeedBoost());
                
                break;
        }
    }
    
    private void Freeze()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            enemy.GetComponent<Enemy>().Freeze();
        }
    }
    

    public IEnumerator SpeedBoost()
    {
        player.speed += 5;
        yield return new WaitForSeconds(5f);
        player.speed -= 5;
    }
    
    public IEnumerator Fire()
    {
       
        player.Fire.SetActive(true);
        player.feared = true;
        yield return new WaitForSeconds(5.5f);
        player.Fire.SetActive(false);
        player.feared = false;

    }
}
