using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelicDisplay : MonoBehaviour
{
    [SerializeField]private ToolTip toolTip;
    [SerializeField]private Image icon;
    [SerializeField]private UIHoverEffect UIHoverEffect;

    public Relic Relic;

    private void Start()
    {
        RelicManager.UseRelic += UseRelic;
    }

    private void OnDestroy()
    {
        RelicManager.UseRelic -= UseRelic;

    }

    public void SetRelicUI(Relic relic)
    {
        Relic = relic;
        icon.sprite = relic.icon;
        toolTip.e = relic;
        toolTip.Title = relic.name;
        toolTip.Message = relic.relicDescription;
        
        UIHoverEffect.FlashScale();

    }

    private void UseRelic(RelicType r)
    {
        if (Relic.relicType == r)
        {
            UIHoverEffect.FlashScale();
        }
    }
}
