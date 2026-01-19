using System;
using System.Collections.Generic;
using UnityEngine;

public class UIStateMonitor : MonoBehaviour
{
    public event Action<UIScreen> OnScreenChanged;

    public UIScreen CurrentNavigatableScreen => currentNavigatableScreen;
    public UIScreen CurrentActiveScreen => currentActiveScreen;
    public UIScreen PreviousActiveScreen => previousActiveScreen;


    private Stack<UIScreen> UIScreenStack = new Stack<UIScreen>();
    private HashSet<UIScreen> uiScreens = new HashSet<UIScreen>();

    private UIScreen currentNavigatableScreen = null;
    private UIScreen currentActiveScreen = null;
    private UIScreen previousActiveScreen = null;

    //public void AddToStack(UIScreen screen)
    //{
    //    UIScreenStack.Push(screen);
    //}

    //public void RemoveFromStack(UIScreen screen)
    //{
    //    if(UIScreenStack.Peek() == screen)
    //    {
    //        UIScreenStack.Pop();
    //    }

    //    UIScreenStack.Peek().Activate();
    //}

    public UIScreen GetCurrentTopMostScreen => UIScreenStack.Count > 0 ? UIScreenStack.Peek() : null;

    public void RegisterScreen(UIScreen screen)
    {
        uiScreens.Add(screen);
        screen.OnNewScreenActive += HandleScreenNavigatableChange;
        
    }

    private void HandleScreenNavigatableChange(UIScreen eventOwner, bool navigatable)
    {
        previousActiveScreen = currentActiveScreen;

        foreach (var screen in uiScreens)
        {
            if (screen != eventOwner)
            {
                screen.SetNavigatable(false);
            }
            else 
            {
                if(screen.Navigatable)
                    currentNavigatableScreen = screen;                
            }
        }

        currentActiveScreen = eventOwner;
        OnScreenChanged?.Invoke(currentActiveScreen);
    }
}
