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
    
    private int FirstDragonRelicIndex = 34;
    public static RelicManager _instance;

    public bool UsedRelic23 = false; // resurect
    public bool UsedRelic19 = false; // prevent first spell damage
    public bool UsedRelic20 = false; // prevent first physical damage
    public bool UsedRelic24 = false; // first buff free
    public bool UsedRelic8 = false; //  drop below 50% gain block
    public bool UsedRelic6 = false; //  drop below 50% gain armor + mr
    public bool UsedRelic7 = false; //  drop below 50% gain sp + ad

    public float HeartSeekersCounter = 0;
    public bool HasRelic4Buff = false;


    public int UnstableEnergyCoreCounter = 0;
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
        UnSeenRelics.RemoveAt(0); // remove none relic
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
            Debug.Log(r.ToString() + "relic used");
            UseRelic(r);
            return true;
        }
        return false;
    }
    
    public void GetRelic(int i)
    {
        RelicType t = (RelicType)i;
        Relic r = new Relic(t, sprite:relicIcons[(int)t]);
        SeenRelics.Add(r.relicType);

        SelectRelic( r);
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
            RelicType r =  UnSeenDragonRelics[Random.Range(0, UnSeenDragonRelics.Count)];
            SeenDragonRelics.Add(r);
            UnSeenDragonRelics.Remove(r);
            
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
