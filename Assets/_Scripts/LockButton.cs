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
    [SerializeField] private AudioClip lockSFX;
    [SerializeField] private AudioClip unlockSFX;

    public void OnSelected()
    {
        selected = !selected;

        if (selected)
        {
            icon.sprite = lockIcon;
            icon.color = lockedColor;
            SoundManager.Instance.Play2DSFX(lockSFX, .25f, 1, .05f);
        }
        else
        {
            icon.sprite = unLockIcon;
            icon.color = Color.white;
            SoundManager.Instance.Play2DSFX(unlockSFX, .25f, 1, .05f);
        }
    }

}
