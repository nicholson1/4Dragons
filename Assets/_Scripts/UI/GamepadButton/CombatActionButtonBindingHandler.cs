using DFG.UIHandling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatActionButtonBindingHandler : MonoBehaviour
{   
    private UIScreenCombat combatScreen;
    private ButtonExtender buttonExtender;
    private ToolTip toolTip;
    private RectTransform rt;
    [SerializeField] private int actionIndex = 0;
    

    private InputHandler inputHandler;

    
    private void HandleAttackPressed(int index)
    {
        if(index == actionIndex)
            buttonExtender.ClickButton(InputSource.Gamepad);

        //if(actionIndex == index)
        //{
        //    if(EventSystem.current.currentSelectedGameObject == button.gameObject)
        //    {
        //        Debug.LogError($"Attack pressed!");
        //        button.onClick.Invoke();
        //        return;
        //    }

        //    Debug.LogError($"Attack selected!");
        //    EventSystem.current.SetSelectedGameObject(this.gameObject);
        //    return;
        //}
    }

    private void BindInput()
    {
        inputHandler.OnAttackButtonPressed.AddListener(HandleAttackPressed);
    }

    private void UnbindInput()
    { 
        inputHandler.OnAttackButtonPressed.RemoveListener(HandleAttackPressed);
    }

    private void UpdateBinding(CombatUINavigationMode mode)
    { 
        if(mode == CombatUINavigationMode.Combat)
        {
            BindInput();
        }
        else
        {
            UnbindInput();
        }
    }

    private void Awake()
    {
        buttonExtender = GetComponent<ButtonExtender>();
        toolTip = GetComponent<ToolTip>();
        rt = GetComponent<RectTransform>();
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        combatScreen = GetComponentInParent<UIScreenCombat>();
        combatScreen.OnCombatUINavigationChanged += UpdateBinding;
    }


    private void OnDestroy()
    {
        combatScreen.OnCombatUINavigationChanged -= UpdateBinding;
    }
}
