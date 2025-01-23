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

    public (string, Sprite, Color, string) GetValues(Stats stat)
    {
        //Debug.Log(stat);
        return (statNames[(int)stat-2], statIcons[(int)stat-2], statColors[(int)stat-2], statDesc [(int)stat-2]);
    }
    
    public (string, Sprite, Color, string) GetValuesFromSpell(SpellTypes s)
    {
        int stat = GetStatIntFromSpell(s);
        //Debug.Log(stat);
        return (statNames[(int)stat-2], statIcons[(int)stat-2], statColors[(int)stat-2], statDesc [(int)stat-2]);
    }
    private int GetStatIntFromSpell(SpellTypes s)
    {
        int i = 0;
        switch (s)
        {
            case SpellTypes.Dagger1:
                i = 7;
                break;
            case SpellTypes.Dagger2:
                i = 7;
                break;
            case SpellTypes.Dagger3:
                i = 7;
                break;
            case SpellTypes.Shield1:
                i = 8;
                break;
            case SpellTypes.Shield2:
                i = 8;
                break;
            case SpellTypes.Shield3:
                i = 8;
                break;
            case SpellTypes.Sword1:
                i = 5;
                break;
            case SpellTypes.Sword2:
                i = 5;
                break;
            case SpellTypes.Sword3:
                i = 5;
                break;
            case SpellTypes.Axe1:
                i = 6;
                break;
            case SpellTypes.Axe2:
                i = 6;
                break;
            case SpellTypes.Axe3:
                i = 6;
                break;
            case SpellTypes.Hammer1:
                i = 9;
                break;
            case SpellTypes.Hammer2:
                i = 9;
                break;
            case SpellTypes.Hammer3:
                i = 9;
                break;
            case SpellTypes.Nature1:
                i = 11;
                break;
            case SpellTypes.Nature2:
                i = 11;
                break;
            case SpellTypes.Nature3:
                i = 11;
                break;
            case SpellTypes.Nature4:
                i = 11;
                break;
            case SpellTypes.Nature5:
                i = 11;
                break;
            case SpellTypes.Fire1:
                i = 12;
                break;
            case SpellTypes.Fire2:
                i = 12;
                break;
            case SpellTypes.Fire3:
                i = 12;
                break;
            case SpellTypes.Fire4:
                i = 12;
                break;
            case SpellTypes.Fire5:
                i = 12;
                break;
            case SpellTypes.Ice1:
                i = 13;
                break;
            case SpellTypes.Ice2:
                i = 13;
                break;
            case SpellTypes.Ice3:
                i = 13;
                break;
            case SpellTypes.Ice4:
                i = 13;
                break;
            case SpellTypes.Ice5:
                i = 13;
                break;
            case SpellTypes.Blood1:
                i = 14;
                break;
            case SpellTypes.Blood2:
                i = 14;
                break;
            case SpellTypes.Blood3:
                i = 14;
                break;
            case SpellTypes.Blood4:
                i = 14;
                break;
            case SpellTypes.Blood5:
                i = 14;
                break;
            case SpellTypes.Shadow1:
                i = 15;
                break;
            case SpellTypes.Shadow2:
                i = 15;
                break;
            case SpellTypes.Shadow3:
                i = 15;
                break;
            case SpellTypes.Shadow4:
                i = 15;
                break;
            case SpellTypes.Shadow5:
                i = 15;
                break;

        }

        return i;
    }
    
    
}
