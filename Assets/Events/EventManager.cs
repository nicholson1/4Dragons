using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [field: SerializeField] public Database Database { get; private set;}

    bool isInitialized;

    public void Initialize()
    {
        if (Database == null)
            Database = Resources.Load<Database>("Database");

        isInitialized = true;
    }

    public EEvent GetRandomEvent(int trialCount)
    {
        return EEvent.Blacksmith;
    }
}
