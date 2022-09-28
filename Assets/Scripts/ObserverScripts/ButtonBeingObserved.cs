using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBeingObserved : SubjectBeingObserved
{

       [SerializeField] private NotificationType type;

       public void ClickButton()
       {
              NotifyButtonClick(type);
              
       }

      
}
