using System;
using System.Collections;
using System.Collections.Generic;
using Map;
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

        
        CheckRelicForOnPickupEffect(relic);
        

    }
    

    private void UseRelic(RelicType r)
    {
        if (Relic.relicType == r)
        {
            UIHoverEffect.FlashScale();
        }
    }

    private void CheckRelicForOnPickupEffect(Relic relic)
    {
        Start();
        if (relic.relicType == RelicType.Relic16)
        {
            if(RelicManager._instance.CheckRelic(RelicType.Relic16))
            {
                CombatController._instance.Player.GetGold(500);
            }
        }
        if (relic.relicType == RelicType.DragonRelic8)
        {
            if(RelicManager._instance.CheckRelic(RelicType.DragonRelic8))
            {
                CombatController._instance.Player._maxEnergy += 1;
            }
        }
        if (relic.relicType == RelicType.DragonRelic9)
        {
            if(RelicManager._instance.CheckRelic(RelicType.DragonRelic9))
            {
                CombatController._instance.Player._maxEnergy += 1;
            }
        }
        if (relic.relicType == RelicType.DragonRelic10)
        {
            if(RelicManager._instance.CheckRelic(RelicType.DragonRelic10))
            {
                CombatController._instance.Player._maxEnergy += 2;
            }
        }
        if (relic.relicType == RelicType.DragonRelic11)
        {
            if(RelicManager._instance.CheckRelic(RelicType.DragonRelic11))
            {
                CombatController._instance.Player._maxEnergy += 2;
            }
        }
        if (relic.relicType == RelicType.DragonRelic12)
        {
            if(RelicManager._instance.CheckRelic(RelicType.DragonRelic12))
            {
                CombatController._instance.Player._maxEnergy += 2;
            }
        }
        if (relic.relicType == RelicType.DragonRelic13)
        {
            if(RelicManager._instance.CheckRelic(RelicType.DragonRelic13))
            {
                CombatController._instance.Player._maxEnergy += 2;
            }
        }
        if (relic.relicType == RelicType.DragonRelic14)
        {
            if(RelicManager._instance.CheckRelic(RelicType.DragonRelic14))
            {
                CombatController._instance.Player._maxEnergy += 3;
            }
        }
        if (relic.relicType == RelicType.Relic27)
        {
            if(RelicManager._instance.CheckRelic(RelicType.Relic27))
            {
                MapView.Instance.SetAttainableNodes();
            }
        }
    }
}
