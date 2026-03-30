using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargettingButtonListener : MonoBehaviour, IGamepadButtonListener
{
    public event Action OnGamepadButtonSelected;
    public event Action OnGamepadButtonDeselected;

    public Button GamepadButton => button;
    public Image TargetImage => targetImage;

    private Button button;
    private Image targetImage;
    private CombatEntity combatEntity;

    private PotionDrag potionDrag;


    public void HandleGamepadButtonDeselected(Selectable selectable)
    {
        TargetImage.enabled = false;
    }

    public void HandleGamepadButtonPressed(Selectable selectable)
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

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        targetImage = GetComponent<Image>();
        targetImage.enabled = false;
    }
        
}
