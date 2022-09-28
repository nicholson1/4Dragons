
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;

public class InventorySystem : SubjectBeingObserved, Observer
{
    private GameManager GM;


    private int firePickupCount = 0;
    private int freezePickupCount = 0;
    private int speedPickupCount = 0;
    private int doublePointsPickupCount = 0;

    
    public void OnNotify(Collectible obj, NotificationType notificationType)
    {
        switch (notificationType)
        {
            case NotificationType.HealthPickup:
                //Debug.Log("Congrats! + 1 health");
                GM.lives += 1;
                break;
            
            case NotificationType.FirePickup:
                //Debug.Log("Congrats! + 1 fire");
                firePickupCount += 1;
                NotifyUI(firePickupCount, NotificationType.FirePickup);
                break;
            
            case NotificationType.FreezePickup:
                //Debug.Log("Congrats! + 1 freese");
                freezePickupCount += 1;
                NotifyUI(freezePickupCount, NotificationType.FreezePickup);
                break;
            
            case NotificationType.SpeedPickup:
                //Debug.Log("Congrats! + 1 speed");
                speedPickupCount += 1;
                NotifyUI(speedPickupCount, NotificationType.SpeedPickup);
                break;
            
            case NotificationType.DoublePointsPickup:
                //Debug.Log("Congrats! + 1 dp");
                doublePointsPickupCount += 1;
                NotifyUI(doublePointsPickupCount, NotificationType.DoublePointsPickup);
                break;
        }
        
    }

    public void OnNotifyUI(int i, NotificationType notificationType)
    {
        //throw new NotImplementedException();
    }

    public void OnNotifyButtonClick(NotificationType notificationType)
    {
        switch (notificationType)
        {

            case NotificationType.FirePickup:
                firePickupCount -= 1;
                NotifyUI(firePickupCount, NotificationType.FirePickup);
                break;
            
            case NotificationType.FreezePickup:
                freezePickupCount -= 1;
                NotifyUI(freezePickupCount, NotificationType.FreezePickup);
                break;
            
            case NotificationType.SpeedPickup:
                speedPickupCount -= 1;
                NotifyUI(speedPickupCount, NotificationType.SpeedPickup);
                break;
            
            case NotificationType.DoublePointsPickup:
                doublePointsPickupCount -= 1;
                NotifyUI(doublePointsPickupCount, NotificationType.DoublePointsPickup);
                StartCoroutine(DoublePoints());
                break;
        }
    }

    void Start()
    {
        GM = GetComponent<GameManager>();
        foreach (PickupObserved subject in FindObjectsOfType<PickupObserved>())
        {
            //Debug.Log(subject.name);

            subject.AddObserver(this);
        }
        foreach (ButtonBeingObserved subject in FindObjectsOfType<ButtonBeingObserved>())
        {
            subject.AddObserver(this);
            subject.gameObject.SetActive(false);
            
        }
        GM.GameScreen.SetActive(false);
        //StartCoroutine(wait());
        //
    }

    private IEnumerator wait()
    {
        Debug.Log("I ran");
        yield return new WaitForSeconds(.1f);
        foreach (ButtonBeingObserved subject in FindObjectsOfType<ButtonBeingObserved>())
            subject.gameObject.SetActive(false);
        GM.GameScreen.SetActive(false);
        Debug.Log("all dine");

        
    }

    private IEnumerator DoublePoints()
    {
       
        GM.doublePoints = true;
        yield return new WaitForSeconds(10);
        GM.doublePoints = false;
    }
    
}

