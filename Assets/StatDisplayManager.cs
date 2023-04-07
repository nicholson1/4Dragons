using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;

public class StatDisplayManager : MonoBehaviour
{
    public static StatDisplayManager _instance;

    [SerializeField] private Sprite[] statIcons;
    [SerializeField] private string[] statNames;
    [SerializeField] private Color[] statColors;
    [SerializeField] private string[] statDesc;



    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public (string, Sprite, Color, string) GetValues(Equipment.Stats stat)
    {
        //Debug.Log(stat);
        return (statNames[(int)stat-2], statIcons[(int)stat-2], statColors[(int)stat-2], statDesc [(int)stat-2]);
    }
    
    public (string, Sprite, Color, string) GetValuesFromSpell(Weapon.SpellTypes s)
    {
        int stat = GetStatIntFromSpell(s);
        //Debug.Log(stat);
        return (statNames[(int)stat-2], statIcons[(int)stat-2], statColors[(int)stat-2], statDesc [(int)stat-2]);
    }
    private int GetStatIntFromSpell(Weapon.SpellTypes s)
    {
        int i = 0;
        switch (s)
        {
            case Weapon.SpellTypes.Dagger1:
                i = 7;
                break;
            case Weapon.SpellTypes.Dagger2:
                i = 7;
                break;
            case Weapon.SpellTypes.Shield1:
                i = 8;
                break;
            case Weapon.SpellTypes.Shield2:
                i = 8;
                break;
            case Weapon.SpellTypes.Sword1:
                i = 5;
                break;
            case Weapon.SpellTypes.Sword2:
                i = 5;
                break;
            case Weapon.SpellTypes.Axe1:
                i = 6;
                break;
            case Weapon.SpellTypes.Axe2:
                i = 6;
                break;
            case Weapon.SpellTypes.Hammer1:
                i = 9;
                break;
            case Weapon.SpellTypes.Hammer2:
                i = 9;
                break;
            case Weapon.SpellTypes.Nature1:
                i = 11;
                break;
            case Weapon.SpellTypes.Nature2:
                i = 11;
                break;
            case Weapon.SpellTypes.Nature3:
                i = 11;
                break;
            case Weapon.SpellTypes.Nature4:
                i = 11;
                break;
            case Weapon.SpellTypes.Nature5:
                i = 11;
                break;
            case Weapon.SpellTypes.Fire1:
                i = 12;
                break;
            case Weapon.SpellTypes.Fire2:
                i = 12;
                break;
            case Weapon.SpellTypes.Fire3:
                i = 12;
                break;
            case Weapon.SpellTypes.Fire4:
                i = 12;
                break;
            case Weapon.SpellTypes.Fire5:
                i = 12;
                break;
            case Weapon.SpellTypes.Ice1:
                i = 13;
                break;
            case Weapon.SpellTypes.Ice2:
                i = 13;
                break;
            case Weapon.SpellTypes.Ice3:
                i = 13;
                break;
            case Weapon.SpellTypes.Ice4:
                i = 13;
                break;
            case Weapon.SpellTypes.Ice5:
                i = 13;
                break;
            case Weapon.SpellTypes.Blood1:
                i = 14;
                break;
            case Weapon.SpellTypes.Blood2:
                i = 14;
                break;
            case Weapon.SpellTypes.Blood3:
                i = 14;
                break;
            case Weapon.SpellTypes.Blood4:
                i = 14;
                break;
            case Weapon.SpellTypes.Blood5:
                i = 14;
                break;
            case Weapon.SpellTypes.Shadow1:
                i = 15;
                break;
            case Weapon.SpellTypes.Shadow2:
                i = 15;
                break;
            case Weapon.SpellTypes.Shadow3:
                i = 15;
                break;
            case Weapon.SpellTypes.Shadow4:
                i = 15;
                break;
            case Weapon.SpellTypes.Shadow5:
                i = 15;
                break;

        }

        return i;
    }
    
    
}
