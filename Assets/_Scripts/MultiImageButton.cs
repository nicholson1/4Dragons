using UnityEngine;
using UnityEngine.UI;

public class MultiImageButton : Button
{
    private Graphic[] graphics;

    protected override void Awake()
    {
        base.Awake();

        // Cache all child graphics, including this button's target graphic.
        graphics = GetComponentsInChildren<Graphic>(true);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        Color targetColor = state switch
        {
            SelectionState.Normal    => colors.normalColor,
            SelectionState.Highlighted => colors.highlightedColor,
            SelectionState.Pressed   => colors.pressedColor,
            SelectionState.Selected  => colors.selectedColor,
            SelectionState.Disabled  => colors.disabledColor,
            _ => colors.normalColor
        };

        float duration = instant ? 0f : colors.fadeDuration;

        foreach (Graphic graphic in graphics)
        {
            if (graphic != null)
                graphic.CrossFadeColor(targetColor, duration, true, true);
        }
    }
}