using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ImportantStuff;
using UnityEngine;

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

    private int rightHandIndex = 0;
    private int LeftHandIndex = 0;


    private int headIndex = 0;
    private int shoulderIndex = 6;
    private int chestIndex = 2;
    private int gloveIndex = 1;
    private int bootIndex = 5;
    private int faceIndex = 0;
    private int hairIndex = 4;

    private bool showHelm = true;

    //todo save in player prefs
    // head, shoulder, chest, gloves, boots, weapon1, weapon2
    private int[] forcedCosmetics = new int[7] { 0,0,0,0,0,0,0}; 
    
    public void RandomCharacter()
    {
        faceIndex = Random.Range(0, Faces.Length);
        Faces[0].SetActive(false);
        Faces[faceIndex].SetActive(true);
        
        hairIndex = Random.Range(0, Hair.Length);
        Hair[0].SetActive(false);
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
        RightHandWep[rightHandIndex].SetActive(true);
    }

    public void RandomFaceAndHair()
    {
        Hair[hairIndex].SetActive(false);
        hairIndex = Random.Range(0, Hair.Length);
        Hair[hairIndex].SetActive(true);

        Faces[faceIndex].SetActive(false);
        faceIndex = Random.Range(0, Faces.Length);
        Faces[faceIndex].SetActive(true);
    }

    public void ForceCosmeticToggle(int slotIndex)
    {
        forcedCosmetics[slotIndex] = (forcedCosmetics[slotIndex] + 1) % 2;
        Debug.Log(forcedCosmetics);
        
    }
    
    
}
