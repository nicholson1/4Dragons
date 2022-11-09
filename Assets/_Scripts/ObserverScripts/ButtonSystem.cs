using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonSystem : SubjectBeingObserved, Observer
{

    [SerializeField] private TextMeshProUGUI fireText;
    [SerializeField] private TextMeshProUGUI freezeText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI doubleText;

    public void OnNotify(Collectible obj, NotificationType notificationType)
    {
        //do nothing
        
    }

    public void OnNotifyUI(int i, NotificationType notificationType)
    {
        
        switch (notificationType)
        {
            case NotificationType.FirePickup:
                RefreshButtonUi(i, fireText);
                break;
            case NotificationType.FreezePickup :
                RefreshButtonUi(i, freezeText);
                break;
            case NotificationType.SpeedPickup:
                RefreshButtonUi(i, speedText);
                break;
            case NotificationType.DoublePointsPickup:
                RefreshButtonUi(i, doubleText);
                break;
        }
    }

    public void OnNotifyButtonClick(NotificationType notificationType)
    {
        // do nothing
    }

    void Start()
    {
        //GM = GetComponent<GameManager>();
        foreach (InventorySystem subject in FindObjectsOfType<InventorySystem>())
        {
            subject.AddObserver(this);
        }
    }

    void RefreshButtonUi(int i, TextMeshProUGUI text)
    {
        if (i > 0)
        {
            text.text = i.ToString();
            text.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            text.transform.parent.gameObject.SetActive(false);
        }
    }
    
}
