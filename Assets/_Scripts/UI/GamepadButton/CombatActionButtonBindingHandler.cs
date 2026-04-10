using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatActionButtonBindingHandler : MonoBehaviour
{   
    private UIScreenCombat combatScreen;
    private Button button;
    private ToolTip toolTip;
    private RectTransform rt;
    [SerializeField] private int actionIndex = 0;
  

    private InputHandler inputHandler;

    private bool pressedOnce = false;
    
    private void HandleAttackPressed(int index)
    {
        if(actionIndex == index)
        {
            if(EventSystem.current.currentSelectedGameObject == button.gameObject)
            {
                Debug.LogError($"press button {gameObject.name}");
                button.onClick.Invoke();
                return;
            }

            Debug.LogError($"Select button {gameObject.name}");
            button.Select();
            return;
        }


        //if (pressedOnce)
        //{
        //    pressedOnce = false;

        //    if (actionIndex != index)
        //    {
        //        Debug.LogError($"Attack pressed for something else (index {index} - Closing tooltip for attack {actionIndex}");                
        //        return;
        //    }

        //    Debug.LogError($"Attack Pressed twice with Index {index} - Closing tooltip for attack {actionIndex}");
        //    button.onClick.Invoke();            
        //    return;
        //}

        //if (actionIndex != index)
        //{
        //    pressedOnce = false;
        //    return;
        //}

        //pressedOnce = true;
        //button.Select();
        //Debug.LogError($"Attack Pressed once with Index {index} - Opening tooltip for attack index {actionIndex}");
        //toolTip.ShowTipFromGamepadNavi(rt);
    }

    //private void DebugClickCombatButton()
    //{
    //    Debug.Log($"combatButton of {gameObject.name} was clicked!");
    //}

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
        button = GetComponent<Button>();
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
