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
        if(pressedOnce)
        {
            if(actionIndex != index)
            {
                toolTip.CloseTip();
                pressedOnce = false;
                return;
            }

            button.onClick.Invoke();
            pressedOnce = false;
            return;
        }

        if (actionIndex != index)
            return;

        pressedOnce = true;
        toolTip.ShowTipFromGamepadNavi(rt);
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

    private void OnEnable()
    {
        inputHandler.OnAttackButtonPressed.AddListener(HandleAttackPressed);

        //button.onClick.AddListener(DebugClickCombatButton);
    }

    private void OnDisable()
    {
        inputHandler.OnAttackButtonPressed.RemoveListener(HandleAttackPressed);

        //button.onClick.RemoveListener(DebugClickCombatButton);
        combatScreen.OnCombatUINavigationChanged -= UpdateBinding;
    }
}
