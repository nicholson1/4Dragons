using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamepadGlobalCancelButton : MonoBehaviour
{
    private Button button;
    private InputHandler inputHandler;

    private void Start()
    {
        button = GetComponent<Button>();
        inputHandler = EventSystem.current.GetComponent<InputHandler>();


    }
}
