using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStateMonitor : MonoBehaviour
{            
    private Stack<UIScreen> UIScreenStack = new Stack<UIScreen>();
    private HashSet<UIScreen> uiScreens = new HashSet<UIScreen>();

    public UIScreen DebugCurrentNavigatableScreen = null;

    public void AddToStack(UIScreen screen)
    {
        UIScreenStack.Push(screen);
    }

    public void RemoveFromStack(UIScreen screen)
    {
        if(UIScreenStack.Peek() == screen)
        {
            UIScreenStack.Pop();
        }

        UIScreenStack.Peek().Activate();
    }

    public UIScreen GetCurrentTopMostScreen => UIScreenStack.Count > 0 ? UIScreenStack.Peek() : null;

    public void RegisterScreen(UIScreen screen)
    {
        uiScreens.Add(screen);
        screen.OnScreenSetToNavigatable += HandleScreenNavigatableChange;
    }

    private void HandleScreenNavigatableChange(UIScreen eventOwner)
    {
        foreach (var screen in uiScreens)
        {
            if (screen != eventOwner)
            {
                screen.SetNavigatable(false);
            }
            else 
            {
                DebugCurrentNavigatableScreen = screen;
            }
        }    
        
    }
}
