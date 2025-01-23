using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IntentDisplay : MonoBehaviour, IPointerExitHandler
{
    [SerializeField]private Image intent0;
    [SerializeField]private Image intent1;

    [SerializeField] private ToolTip _toolTip;


    public float bounceHeight;
    private RectTransform rt;
    public void UpdateInfo(SpellTypes spell)
    {
        (Sprite, Sprite) sprites = TheSpellBook._instance.GetAbilityTypeIcons(spell);

        intent0.sprite = sprites.Item1;
        if (sprites.Item2 != null)
        {
            intent1.gameObject.SetActive(true);
            intent1.sprite = sprites.Item2;
        }
        else
        {
            intent1.sprite = null;
            intent1.gameObject.SetActive(false);
            

        }

        (string, string, string, string) strings = TheSpellBook._instance.GetIntentTitleAndDescription(spell);

        _toolTip.iLvl = "";
        _toolTip.Cost = "";
        _toolTip.rarity = -1;

        if (strings.Item3 == "")
        {
            _toolTip.Title = strings.Item1;
            _toolTip.Message = strings.Item2;
            
        }
        else
        {
            _toolTip.Title = strings.Item1 + " and " + strings.Item3;
            _toolTip.iLvl = "";
            _toolTip.Message = strings.Item2 +" and " + strings.Item4;
        }
        
    }

    private Vector3 startPosition;
    void Start()
    {
        rt = GetComponent<RectTransform>();
        //startPosition = rt.localPosition;
        // Start the infinite pingpong animation
        StartPingPongAnimation();
    }

    void StartPingPongAnimation()
    {
        float target = rt.anchoredPosition.y + bounceHeight;
        //rt.localPosition = startPosition;
        // Create a loop for the pingpong animation
        LeanTween.moveY(rt, target , 2)
            .setEaseInOutSine()
            .setLoopPingPong();
    }

    private void FixedUpdate()
    {
        if (timer < 0)
            return;
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            StartPingPongAnimation();

        }
    }

    private float timer = -1;
    public void OnPointerExit(PointerEventData eventData)
    {
        timer = 2;

    }
}
