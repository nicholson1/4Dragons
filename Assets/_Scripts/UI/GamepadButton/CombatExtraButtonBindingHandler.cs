using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatExtraButtonBindingHandler : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button inspectButton;
    [SerializeField] private Button potionButton;

    private InputHandler inputHandler;


    private void Awake()
    {

    }
}
