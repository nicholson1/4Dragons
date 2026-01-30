using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentButton : MonoBehaviour
{
    public TextMeshProUGUI ButtonLabel => buttonLabel;
    public Image ButtonImage => buttonImage;
    public MultiImageButton Button => button;



    [SerializeField] private TextMeshProUGUI buttonLabel;
    [SerializeField] private Image buttonImage;
    [SerializeField] private MultiImageButton button;
    [SerializeField] private UIHoverEffect hoverEffect;

    public void SetEquipmentButton(Sprite sprite, string text)
    {
        buttonImage.sprite = sprite;
        buttonLabel.text = text;

    }

    public void DeactivateButton()
    {
        button.interactable = false;
        hoverEffect.SetTweening(false);
        hoverEffect.SetShake(false);
    }

    public void ActivateButton()
    {
        button.interactable = true;
        hoverEffect.SetTweening(true);
        hoverEffect.SetShake(true);
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

}
