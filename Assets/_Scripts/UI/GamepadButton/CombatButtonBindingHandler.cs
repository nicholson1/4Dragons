using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatButtonBindingHandler : MonoBehaviour
{
    private Button button;
    [SerializeField] private int actionIndex = 0;

    private InputHandler inputHandler;
    
    private void HandleAttackPressed(int index)
    {
        if(actionIndex == index)
        {
            Debug.Log($"pressing button {actionIndex}");
            button.onClick.Invoke();
        }
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        inputHandler = EventSystem.current.GetComponent<InputHandler>();

    }

    private void OnEnable()
    {
        inputHandler.OnAttackButtonPressed.AddListener(HandleAttackPressed);
    }

    private void OnDisable()
    {
        inputHandler.OnAttackButtonPressed.RemoveListener(HandleAttackPressed);
    }
}
