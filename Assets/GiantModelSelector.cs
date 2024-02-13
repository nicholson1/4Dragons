using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantModelSelector : MonoBehaviour
{
    [SerializeField] private GameObject[] GiantModel;

    public void SelectModel(EliteType eliteType)
    {
        GiantModel[(int)eliteType - 10].gameObject.SetActive(true);
    }
}
