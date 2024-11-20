using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using ImportantStuff;
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

    [SerializeField] private Equipment.Slot slot;

    public void OnSelected()
    {
        selected = !selected;

        UpdateVisuals();
        SetPlayerPrefs();
        
    }

    private void UpdateVisuals()
    {
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

    private void SetPlayerPrefs()
    {
        int val;
        if (selected)
            val = 1;
        else
            val = 0;
        switch (slot)
        {
            case Equipment.Slot.Head:
                PlayerPrefsManager.SetHelmetLock(val);
                break;
            case Equipment.Slot.Shoulders:
                PlayerPrefsManager.SetShoulderLock(val);
                break;
            case Equipment.Slot.Chest:
                PlayerPrefsManager.SetChestLock(val);
                break;
            case Equipment.Slot.Gloves:
                PlayerPrefsManager.SetGlovesLock(val);
                break;
            case Equipment.Slot.Boots:
                PlayerPrefsManager.SetBootsLock(val);
                break;
            case Equipment.Slot.OneHander:
                PlayerPrefsManager.SetWeapon1(val);
                break;
            case Equipment.Slot.TwoHander:
                PlayerPrefsManager.SetWeapon2(val);
                break;
        }
    }

    private void Start()
    {
        bool update = false;
        switch (slot)
        {
            case Equipment.Slot.Head:
                update = (PlayerPrefsManager.GetHelmetLock() == 1);
                break;
            case Equipment.Slot.Shoulders:
                update = (PlayerPrefsManager.GetShoulderLock() == 1);
                break;
            case Equipment.Slot.Chest:
                update = (PlayerPrefsManager.GetChestLock() == 1);
                break;
            case Equipment.Slot.Gloves:
                update = (PlayerPrefsManager.GetGlovesLock() == 1);
                break;
            case Equipment.Slot.Boots:
                update = (PlayerPrefsManager.GetBootsLock() == 1);
                break;
            case Equipment.Slot.OneHander:
                update = (PlayerPrefsManager.GetWeapon1() == 1);
                break;
            case Equipment.Slot.TwoHander:
                update = (PlayerPrefsManager.GetWeapon2() == 1);
                break;
        }
        
        if(update)
        {
            selected = selected = true;
            UpdateVisuals();
        }
    }
}
