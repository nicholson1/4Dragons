using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapNodeButtonBindingHandler : ButtonBindingHandler
{
    public override void ButtonClickCallback()
    {
        Debug.Log($"ButtonClickCallback() override from MapNode button {this.gameObject.name}");
        base.ButtonClickCallback();
    }
}
