using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindIt : MonoBehaviour
{
    [field: SerializeField] public int ID {get; private set;}

    public static FindIt[] instances;

    void Awake()
    {
        if(instances == null)
            instances = new FindIt[ID + 1];

        if (instances.Length <= ID)
        {
            FindIt[] tempInstances = new FindIt[instances.Length];

            for(int i = 0; i < instances.Length; i++)
                tempInstances[i] = instances[i];

            instances = new FindIt[ID + 1];

            for (int i = 0; i < tempInstances.Length; i++)
                instances[i] = tempInstances[i];
        }

        instances[ID] = this;
    }    
}
