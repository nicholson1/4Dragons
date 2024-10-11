using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class LockButton : MonoBehaviour
{
    [SerializeField] private Sprite lockIcon;    
    [SerializeField] private Sprite unLockIcon;
    [SerializeField] private Image icon;

    private Color lockedColor = new Color(1, 1, 1, .5f);

    private bool selected;

    public void OnSelected()
    {
        selected = !selected;

        if (selected)
        {
            icon.sprite = lockIcon;
            icon.color = lockedColor;
        }
        else
        {
            icon.sprite = unLockIcon;
            icon.color = Color.white;
        }
    }

}
