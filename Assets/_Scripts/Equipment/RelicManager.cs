using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ImportantStuff;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RelicManager : MonoBehaviour
{
    [SerializeField] private Transform RelicDisplayHolder;
    [SerializeField] private RelicDisplay RelicDisplayPrefab;
    private List<RelicType> UnSeenRelics = new List<RelicType>();
    private List<RelicType> SeenRelics = new List<RelicType>();
    public List<RelicType> PickedRelics = new List<RelicType>();
    private List<RelicType> UnSeenDragonRelics = new List<RelicType>();
    private List<RelicType> SeenDragonRelics = new List<RelicType>();

    [SerializeField] private Sprite[] relicIcons = new Sprite[]{};
    
    public static event Action<RelicType> UseRelic; 
    
    private int FirstDragonRelicIndex = 30;
    public static RelicManager _instance;

    private bool UsedRelic16 = false; // money
    public bool UsedRelic23 = false; // resurect
    public bool UsedRelic19 = false; // prevent first spell damage
    public bool UsedRelic20 = false; // prevent first physical damage
    public bool UsedRelic24 = false; // first buff free
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
    private void Start()
    {
        UnSeenRelics = Enum.GetValues(typeof(RelicType)).Cast<RelicType>().ToList();
        //Debug.Log("unseen relic counts " + UnSeenRelics.Count);
        UnSeenDragonRelics.AddRange(UnSeenRelics.GetRange(FirstDragonRelicIndex, UnSeenRelics.Count - FirstDragonRelicIndex));
        UnSeenRelics.RemoveRange(FirstDragonRelicIndex, UnSeenRelics.Count - FirstDragonRelicIndex);
    }

    public Relic GetCommonRelic()
    {
        RelicType t = SelectCommonRelicType();
        Relic r = new Relic(t, sprite:relicIcons[(int)t]);
        return r;
    }
    public Relic GetDragonRelic()
    {
        RelicType t = SelectDragonRelicType();
        Relic r = new Relic(t, sprite:relicIcons[(int)t]);
        return r;
    }

    public bool CheckRelic(RelicType r)
    {
        if (PickedRelics.Contains(r))
        {
            UseRelic(r);
            return true;
        }
        return false;
    }

    public void SelectRelic(Equipment relic)
    {
        Relic r = (Relic)relic;
        if (SeenRelics.Contains(r.relicType))
        {
            PickedRelics.Add(r.relicType);
            SeenRelics.Remove(r.relicType);
        }
        if (SeenDragonRelics.Contains(r.relicType))
        {
            PickedRelics.Add(r.relicType);
            SeenDragonRelics.Remove(r.relicType);
        }
        //add to display
        RelicDisplay relicDisplay = Instantiate(RelicDisplayPrefab, RelicDisplayHolder);
        relicDisplay.SetRelicUI(r);

        if (UsedRelic16 == false)
        {
            if(CheckRelic(RelicType.Relic16))
            {
                CombatController._instance.Player.GetGold(500);
                UsedRelic16 = true;
            }
        }

    }

    private RelicType SelectCommonRelicType()
    {
        int index = 0;
        if (UnSeenRelics.Count > 0)
        {
            RelicType r =  UnSeenRelics[Random.Range(0, UnSeenRelics.Count)];
            SeenRelics.Add(r);
            UnSeenRelics.Remove(r);

            if (UnSeenRelics.Count == 0)
            {
                UnSeenRelics.AddRange(SeenRelics);
                SeenRelics.Clear();
            }
            
            return r;
        }

        Debug.LogWarning("NO RELICS YET");
        return RelicType.None;
    }
    private RelicType SelectDragonRelicType()
    {
        int index = 0;
        if (UnSeenDragonRelics.Count > 0)
        {
            RelicType r =  UnSeenDragonRelics[Random.Range(0, UnSeenRelics.Count)];
            SeenRelics.Add(r);
            UnSeenRelics.Remove(r);
            
            if (UnSeenDragonRelics.Count == 0)
            {
                UnSeenDragonRelics.AddRange(SeenDragonRelics);
                SeenDragonRelics.Clear();
            }
            
            return r;
        }
        // if (SeenDragonRelics.Count > 0)
        // {
        //     RelicType r =  SeenDragonRelics[Random.Range(0, UnSeenRelics.Count)];
        //     //SeenRelics.Remove(r);
        //     return r;
        // }
        
        Debug.LogWarning("NO Dragon RELICS YET");
        return RelicType.None;
    }


} 
