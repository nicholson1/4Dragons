using System;
using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zak.UISystem;

public class PotionDrag : MonoBehaviour, IDraggablePayload//IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IGamepadButtonListener
{
    public Image PotionImage => image;
    public RectTransform RT => transform as RectTransform;
    public GameObject sourceObject => this.gameObject;
    public PotionHolder currentLocation = null; 

    [SerializeField] private ToolTip _toolTip;
    public Consumable potion;
    [SerializeField] private Image image;
    private RectTransform _rectTransform;
    private Canvas canvas;
    private Transform holder;
    private Vector3 startPos;
       
       
    [SerializeField] private AudioClip usePotion;
    [SerializeField] private float usePotionVol;

    public event Action OnGamepadButtonSelected;
    public event Action OnGamepadButtonDeselected;

    private UIScreenCombat combatScreen;

    public void InitializePotion(Consumable p)
    {
        if (_rectTransform == null)
        {
            _rectTransform = transform as RectTransform;
            canvas = transform.parent.parent.GetComponent<Canvas>();
            holder = transform.parent;
        }

        potion = p;
        _toolTip.e = potion;
        image.sprite = p.icon;

        if (transform.GetSiblingIndex() >= 5)
        {
            SteamAchievementManager.Unlock("bag_potions");
        }

        //gamepadButton ??= GetComponentInChildren<Button>();
        combatScreen = GetComponentInParent<UIScreenCombat>(true);
        combatScreen.RegisterActivePotion(this);

        TutorialManager.Instance.QueueTip(TutorialNames.Potions);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
        //tartPos
        startPos = _rectTransform.anchoredPosition;
        holder.GetComponent<CanvasGroup>().blocksRaycasts = false;

        //_rectTransform.SetParent(transform.parent);
              
    }
       

    public void OnDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
        _rectTransform.anchoredPosition += eventData.delta/ canvas.scaleFactor;
    }

    public void OnDrop(PointerEventData eventData)
    {

              

    }

    public void HandleCancelPerformed()
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        int layer_mask = LayerMask.GetMask("Characters");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the ray hits any object
        if (Physics.Raycast(ray, out hit,100,layer_mask))
        {
            CombatEntity CE = hit.transform.gameObject.GetComponent<CombatEntity>();
            if (CE != null)
            {
                UsePotion(CE);                            
            }
        }

        holder.GetComponent<CanvasGroup>().blocksRaycasts = true;

        //else reset to the pos start position
        _rectTransform.anchoredPosition = startPos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void HandleGamepadButtonSelected(Selectable selectable)
    {
        OnGamepadButtonSelected?.Invoke();
        _toolTip.ShowTipFromGamepadNavi(selectable.GetComponent<RectTransform>());

    }

    public void HandleGamepadButtonDeselected(Selectable selectable)
    {
        OnGamepadButtonDeselected?.Invoke();
        _toolTip.CloseTip();

    }

    //public void HandleGamepadButtonPressed(Selectable selectable, InputSource source)
    //{        
    //    Debug.LogError($"Potion button pressed, initiatePotionTargetting!");
    //    combatScreen.InitiatePotionTargetting(this);


    //}

    public void UsePotion(CombatEntity target)
    {
        target.HitWithPotion(potion.ConsumableType);


    }


}
