using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteManager : MonoBehaviour
{
    public static EliteManager _instance;
    public GameObject[] elites;
    private List<int> EncounteredElites = new List<int>();

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
    
    public EliteType GetEliteType(int level)
    {
        int typeIndex = -1;

        if (EncounteredElites.Count == 20)
        {
            EncounteredElites = new List<int>();
        }
        
        //todo back up if they somehow run into more than 5 elites in 1 trial
        while (typeIndex == -1)
        {
            int i = 0;
            if (level < 10)
            {
                i = Random.Range(0, 5);
            }
            else if (level < 20)
            {
                i = Random.Range(5, 10);
            }
            else if (level < 30)
            {
                i = Random.Range(10, 15);
            }
            else if (level < 40)
            {
                i = Random.Range(15, 20);
            }
            if (!EncounteredElites.Contains(i))
            {
                typeIndex = i;
            }
        }

        EncounteredElites.Add(typeIndex);
        return (EliteType)typeIndex;
    }
}
public enum EliteType
{
    //Trial 1
    Ranger,
    Woodsman,
    Gladiator,
    Cultist,
    Knight,
    
    //Trial 2
    Commander,
    Druid,
    Witchdoctor,
    Pyromancer,
    Bloodbender,
    
    //Trial 3
    HillGiant,
    FireGiant,
    FrostGiant,
    BloodGiant,
    VoidGiant,
    
    //Trial 4
    FireKing,
    IceQueen,
    Assassin,
    Shaman,
    Paladin,
}