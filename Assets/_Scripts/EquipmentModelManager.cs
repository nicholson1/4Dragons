using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ImportantStuff;
using UnityEngine;
using Random = UnityEngine.Random;

public class EquipmentModelManager : MonoBehaviour
{
    [SerializeField] private GameObject[] HeadModels;
    [SerializeField] private GameObject[] ChestModels;
    [SerializeField] private GameObject[] ShoulderModels;
    [SerializeField] private GameObject[] GloveModels;
    [SerializeField] private GameObject[] BootsModels;

    [SerializeField] private GameObject[] Faces;
    [SerializeField] private GameObject[] Hair;

    [SerializeField] private GameObject[] LeftHandWep;
    [SerializeField] private GameObject[] RightHandWep;

    // [SerializeField] private GameObject[] Hammers;
    // [SerializeField] private GameObject[] Swords;
    // [SerializeField] private GameObject[] Axes;
    // [SerializeField] private GameObject[] Sheilds;
    // [SerializeField] private GameObject[] Wands;
    // [SerializeField] private GameObject[] Orbs;
    
    // [SerializeField] private GameObject[] Daggers;
    [SerializeField] private bool isPlayerCharacter = false;
    
    private int rightHandIndex = 0;
    private int LeftHandIndex = 33;


    private int headIndex = 0;
    private int shoulderIndex = 7;
    private int chestIndex = 9;
    private int gloveIndex = 7;
    private int bootIndex = 6;
    private int faceIndex = 5;
    private int hairIndex = 7;

    private bool showHelm = true;
    
    // head, shoulder, chest, gloves, boots, weapon1, weapon2
    private int[] forcedCosmetics = new int[7] { 0,0,0,0,0,0,0};

    private void Start()
    {
        if (!isPlayerCharacter)
            return;
        HeadModels[headIndex].SetActive(false);
        headIndex = PlayerPrefsManager.GetHelmet();
        HeadModels[headIndex].SetActive(true);
        
        ShoulderModels[shoulderIndex].SetActive(false);
        shoulderIndex = PlayerPrefsManager.GetShoulder();
        ShoulderModels[shoulderIndex].SetActive(true);
        
        ChestModels[chestIndex].SetActive(false);
        chestIndex = PlayerPrefsManager.GetChest();
        ChestModels[chestIndex].SetActive(true);
        
        GloveModels[gloveIndex].SetActive(false);
        gloveIndex = PlayerPrefsManager.GetGloves();
        GloveModels[gloveIndex].SetActive(true);
        
        BootsModels[bootIndex].SetActive(false);
        bootIndex = PlayerPrefsManager.GetBoots();
        BootsModels[bootIndex].SetActive(true);
        
        Faces[faceIndex].SetActive(false);
        faceIndex = PlayerPrefsManager.GetFace();
        Faces[faceIndex].SetActive(true);
        
        Hair[hairIndex].SetActive(false);
        hairIndex = PlayerPrefsManager.GetHair();
        Hair[hairIndex].SetActive(true);
        
        LeftHandWep[LeftHandIndex].SetActive(false);
        RightHandWep[rightHandIndex].SetActive(false);

        LeftHandIndex = PlayerPrefsManager.GetWeapon1();
        rightHandIndex = PlayerPrefsManager.GetWeapon2();
        
        LeftHandWep[LeftHandIndex].SetActive(true);
        RightHandWep[rightHandIndex].SetActive(true);


        forcedCosmetics = new[]
        {
            PlayerPrefsManager.GetHelmetLock(),
            PlayerPrefsManager.GetShoulderLock(),
            PlayerPrefsManager.GetChestLock(),
            PlayerPrefsManager.GetGlovesLock(),
            PlayerPrefsManager.GetBootsLock(),
            PlayerPrefsManager.GetWeapon1Lock(),
            PlayerPrefsManager.GetWeapon2Lock()
        };

        if (PlayerPrefsManager.GetShowHelm() > 0)
            showHelm = true;
        else
            showHelm = false;
        
        FixHead();

    }

    public void RandomCharacter()
    
    {
        Faces[faceIndex].SetActive(false);
        faceIndex = Random.Range(0, Faces.Length);
        Faces[faceIndex].SetActive(true);

        Hair[hairIndex].SetActive(false);
        hairIndex = Random.Range(0, Hair.Length);
        Hair[hairIndex].SetActive(true);

        // 1/4 chance they done show helm
        int roll = Random.Range(0, 4);
        if (roll == 0)
        {
            showHelm = false;
        }
    }

    public void UpdateHead()
    {
        showHelm = !showHelm;
        if(showHelm == true)
            PlayerPrefsManager.SetShowHelm(1);
        else
            PlayerPrefsManager.SetShowHelm(0);
        
        HeadModels[headIndex].SetActive(!HeadModels[headIndex].activeSelf);

        if (!showHelm)
        {
            Hair[hairIndex].SetActive(true);
            //Debug.Log("showing hari");

        }
        else 
        {
            if (headIndex > 10 || headIndex == 0)
            {
                Hair[hairIndex].SetActive(true);
            }
            else
            {
                Hair[hairIndex].SetActive(false);
            }
        }
        
    }

    public void FixHead()
    {
        if ((headIndex > 10 || headIndex == 0))
        {
            Hair[hairIndex].SetActive(true);
        }
        else
        {
            Hair[hairIndex].SetActive(false);
        }
        if(!showHelm)
        {
            HeadModels[headIndex].SetActive(false);
            Hair[hairIndex].SetActive(true);
        }
        
    }


    public void UpdateSlot(Equipment equipment,  bool remove = false)
    {
        int newIndex = equipment.modelIndex;

        if (remove)
        {
            newIndex = 0;
        }
        
        // head, shoulder, chest, gloves, boots, weapon1, weapon2

        switch (equipment.slot)
        {
            case Equipment.Slot.Head:

                if (showHelm == false)
                {
                    if(forcedCosmetics[0] == 1)
                        break;
                    HeadModels[headIndex].SetActive(false);
                    headIndex = newIndex;
                    Hair[hairIndex].SetActive(true);
                    break;
                }

                if (newIndex > 10 || newIndex == 0)
                {
                    Hair[hairIndex].SetActive(true);
                }
                else
                {
                    Hair[hairIndex].SetActive(false);
                }
                // need to deal with hair
                HeadModels[headIndex].SetActive(false);
                headIndex = newIndex;
                HeadModels[headIndex].SetActive(true);
                break;
            
            case Equipment.Slot.Shoulders:
                if(forcedCosmetics[1] == 1)
                    break;
                ShoulderModels[shoulderIndex].SetActive(false);
                shoulderIndex = newIndex;
                ShoulderModels[shoulderIndex].SetActive(true);
                break;
            case Equipment.Slot.Chest:
                if(forcedCosmetics[2] == 1)
                    break;
                ChestModels[chestIndex].SetActive(false);
                chestIndex = newIndex;
                ChestModels[chestIndex].SetActive(true);
                break;
            case Equipment.Slot.Gloves:
                if(forcedCosmetics[3] == 1)
                    break;
                GloveModels[gloveIndex].SetActive(false);
                gloveIndex = newIndex;
                GloveModels[gloveIndex].SetActive(true);
                break;
            case Equipment.Slot.Boots:
                if(forcedCosmetics[4] == 1)
                    break;
                BootsModels[bootIndex].SetActive(false);
                bootIndex = newIndex;
                BootsModels[bootIndex].SetActive(true);
                break;

        }
    }

    public void UpdateWeapon(Equipment w1, Equipment w2)
    {
        Weapon wep1 = (Weapon)w1;
        Weapon wep2 = (Weapon)w2;

        int newIndex1 = rightHandIndex;
        int newIndex2 = LeftHandIndex;
        
        
        //figured out which one is in which hand
        
        // put the hammer in hand 1
        // put the shield in hand 2
        if (wep2 != null)
        {
            if (wep2.spellType1 == Weapon.SpellTypes.Hammer2 || wep2.spellType1 == Weapon.SpellTypes.Hammer3 || wep2.spellType1 == Weapon.SpellTypes.Axe3)
            {
                (wep1, wep2) = (wep2, wep1);
                //Debug.Log("put hammer in right hand");
                //Debug.Log(wep1 + " " + wep2);
            }

            //newIndex1 = wep1.modelIndex;
        }

        if (wep1 != null)
        {
            
            if (wep1.spellType1 == Weapon.SpellTypes.Shield2 || wep1.spellType1 == Weapon.SpellTypes.Shield3)
            {
                (wep1, wep2) = (wep2, wep1);

            }
            //newIndex2 = wep2.modelIndex;

        }

        if (wep2 != null)
        {
            newIndex2 = wep2.modelIndex;
            //Debug.Log(wep2.name + " " + newIndex2);


        }

        if (wep1 != null)
        {
            newIndex1 = wep1.modelIndex;
            //Debug.Log(wep1.name + " " + newIndex1);


        }

        
        LeftHandWep[LeftHandIndex].SetActive(false);
        RightHandWep[rightHandIndex].SetActive(false);

        if(forcedCosmetics[5] != 1)
            LeftHandIndex = newIndex2;
        if(forcedCosmetics[6] != 1)
            rightHandIndex = newIndex1;
        
        LeftHandWep[LeftHandIndex].SetActive(true);
        RightHandWep[rightHandIndex].SetActive(true);

    }
    
    public void FaceButton(int direction)
    {
        Faces[faceIndex].SetActive(false);
        faceIndex += direction;
        if (faceIndex > Faces.Length - 1 )
        {
            faceIndex = 0;
        }else if (faceIndex < 0)
        {
            faceIndex = Faces.Length - 1;
        }
        PlayerPrefsManager.SetFace(faceIndex);
        Faces[faceIndex].SetActive(true);
    }
    
    public void HairButton(int direction)
    {
        Hair[hairIndex].SetActive(false);
        hairIndex += direction;
        if (hairIndex > Hair.Length - 1 )
        {
            hairIndex = 0;
        }else if (hairIndex < 0)
        {
            hairIndex = Hair.Length - 1;
        }
        PlayerPrefsManager.SetHair(hairIndex);
        Hair[hairIndex].SetActive(true);
    }
    
    public void HeadButton(int direction)
    {
        HeadModels[headIndex].SetActive(false);
        headIndex += direction;
        if (headIndex > HeadModels.Length - 1 )
        {
            headIndex = 0;
        }else if (headIndex < 0)
        {
            headIndex = HeadModels.Length - 1;
        }
        if (headIndex > 10 || headIndex == 0)
        {
            Hair[hairIndex].SetActive(true);
        }
        else
        {
            Hair[hairIndex].SetActive(false);
        }
        PlayerPrefsManager.SetHelmet(headIndex);
        HeadModels[headIndex].SetActive(true);
    }
    public void ChestButton(int direction)
        {
            ChestModels[chestIndex].SetActive(false);
            chestIndex += direction;
            if (chestIndex > ChestModels.Length - 1 )
            {
                chestIndex = 0;
            }else if (chestIndex < 0)
            {
                chestIndex = ChestModels.Length - 1;
            }
            PlayerPrefsManager.SetChest(chestIndex);
            ChestModels[chestIndex].SetActive(true);
        }
    public void ShoulderButton(int direction)
    {
        ShoulderModels[shoulderIndex].SetActive(false);
        shoulderIndex += direction;
        if (shoulderIndex > ShoulderModels.Length - 1 )
        {
            shoulderIndex = 0;
        }else if (shoulderIndex < 0)
        {
            shoulderIndex = ShoulderModels.Length - 1;
        }
        PlayerPrefsManager.SetShoulder(shoulderIndex);
        ShoulderModels[shoulderIndex].SetActive(true);
    }
    public void BootsButton(int direction)
    {
        BootsModels[bootIndex].SetActive(false);
        bootIndex += direction;
        if (bootIndex > BootsModels.Length - 1 )
        {
            bootIndex = 0;
        }else if (bootIndex < 0)
        {
            bootIndex = BootsModels.Length - 1;
        }
        PlayerPrefsManager.SetBoots(bootIndex);
        BootsModels[bootIndex].SetActive(true);
    }
    public void GloveButton(int direction)
    {
        GloveModels[gloveIndex].SetActive(false);
        gloveIndex += direction;
        if (gloveIndex > GloveModels.Length - 1 )
        {
            gloveIndex = 0;
        }else if (gloveIndex < 0)
        {
            gloveIndex = GloveModels.Length - 1;
        }
        PlayerPrefsManager.SetGloves(gloveIndex);
        GloveModels[gloveIndex].SetActive(true);
    }
    public void Weapon1Button(int direction)
    {
        LeftHandWep[LeftHandIndex].SetActive(false);
        LeftHandIndex += direction;
        if (LeftHandIndex > LeftHandWep.Length - 1 )
        {
            LeftHandIndex = 0;
        }else if (LeftHandIndex < 0)
        {
            LeftHandIndex = LeftHandWep.Length - 1;
        }
        PlayerPrefsManager.SetWeapon1(LeftHandIndex);
        LeftHandWep[LeftHandIndex].SetActive(true);
    }
    public void Weapon2Button(int direction)
    {
        RightHandWep[rightHandIndex].SetActive(false);
        rightHandIndex += direction;
        if (rightHandIndex > RightHandWep.Length - 1 )
        {
            rightHandIndex = 0;
        }else if (rightHandIndex < 0)
        {
            rightHandIndex = RightHandWep.Length - 1;
        }
        PlayerPrefsManager.SetWeapon2(rightHandIndex);
        RightHandWep[rightHandIndex].SetActive(true);
    }

    public void RandomFaceAndHair()
    {
        Hair[hairIndex].SetActive(false);
        hairIndex = Random.Range(0, Hair.Length);
        Hair[hairIndex].SetActive(true);
        PlayerPrefsManager.SetHair(hairIndex);

        Faces[faceIndex].SetActive(false);
        faceIndex = Random.Range(0, Faces.Length);
        Faces[faceIndex].SetActive(true);
        PlayerPrefsManager.SetFace(faceIndex);
    }

    public void ForceCosmeticToggle(int slotIndex)
    {
        int val = (forcedCosmetics[slotIndex] + 1) % 2;
        forcedCosmetics[slotIndex] = val;


        // head, shoulder, chest, gloves, boots, weapon1, weapon2
        switch (slotIndex)
        {
            case 0:
                PlayerPrefsManager.SetChestLock(val);
                break;
            case 1:
                PlayerPrefsManager.SetShoulderLock(val);
                break;
            case 2:
                PlayerPrefsManager.SetChestLock(val);
                break;
            case 3:
                PlayerPrefsManager.SetGlovesLock(val);
                break;
            case 4:
                PlayerPrefsManager.SetBootsLock(val);
                break;
            case 5:
                PlayerPrefsManager.SetWeapon1Lock(val);
                break;
            case 6:
                PlayerPrefsManager.SetWeapon2Lock(val);
                break;
        }

    }
    
    
}
