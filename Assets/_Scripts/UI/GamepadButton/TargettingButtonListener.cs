using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zak.UISystem;

public class TargettingButtonListener : MonoBehaviour, IGamepadButtonListener, IDropListener
{
    public Button GamepadButton => button;
    public event Action OnGamepadButtonSelected;
    public event Action OnGamepadButtonDeselected;

    //public Button GamepadButton => button;
    public Image TargetImage => targetImage;

    private Button button;
    private Image targetImage;
    private CombatEntity combatEntity;

    private PotionDrag potionDrag;

    public void HandleGamepadButtonDeselected(Selectable selectable)
    {
        TargetImage.enabled = false;
    }

    public void HandleGamepadButtonPressed(Selectable selectable, InputSource source)
    {
        potionDrag.UsePotion(combatEntity);
    }

    public void HandleGamepadButtonSelected(Selectable selectable)
    {
        TargetImage.enabled = true;
    }

    public void InitializeButton(PotionDrag potion)
    {
        combatEntity = GetComponentInParent<HealthBar>().displayCharacter._combatEntity;

        potionDrag = potion;
        targetImage.sprite = potion.PotionImage.sprite;
    }

    public void HandleCancelPerformed()
    {

    }

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        targetImage = GetComponent<Image>();
        targetImage.enabled = false;
    }

    public bool CanAcceptDrop(IDraggablePayload payload)
    {
        var potionDrag = payload as PotionDrag;

        if (potionDrag == null)
            return false;

        return true;
    }
}
