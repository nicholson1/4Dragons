using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScreenVictory : UIScreen
{
    [SerializeField] private VictorySequence victorySequence;

    public override void Activate(bool navigatableOnActivated = true)
    {
        base.Activate(navigatableOnActivated);
        
        victorySequence.StartVictorySequence();

    }
}
