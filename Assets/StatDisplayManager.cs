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

    public (string, Sprite, Color) GetValues(Equipment.Stats stat)
    {
        return (statNames[(int)stat-2], statIcons[(int)stat-2], statColors[(int)stat-2] );
    }
    
    
}
