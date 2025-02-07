using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLimitBySiblings : MonoBehaviour
{
    [field: SerializeField] public LimitRectPosition Limiter {get; private set;}
    [field: SerializeField] public Vector2[] Limits {get; private set;}

    int _siblingIndex = 0;

    private void Start()
    {
        _siblingIndex = transform.GetSiblingIndex();
        Limiter.SetLimits(Limits[_siblingIndex], Limits[_siblingIndex]);
    }

    void Update()
    {
        int siblingIndex = transform.GetSiblingIndex();

        if (siblingIndex == _siblingIndex)
            return;

        _siblingIndex = siblingIndex;
        Limiter.SetLimits(Limits[_siblingIndex], Limits[_siblingIndex]);
    }    
}
