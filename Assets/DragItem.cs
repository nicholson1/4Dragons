using ImportantStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DragItem : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public bool IsBeingDragged => isBeingDragged;

    public Equipment e;
    public InventorySlot currentLocation;
    private InventorySlot temp;
    public Image icon;
    public Image Background;
    public Image Glow;
    public Image GamepadHighlighter;
    public SlotHighlighter ForgeableHighlighter;

    [SerializeField] private Sprite[] BackgroundSprites;
    [SerializeField] private Sprite[] GlowSprites;

    public Equipment.Slot slotType;
    public TextMeshProUGUI LvlText;
    
    public static event Action<ErrorMessageManager.Errors> CombatMove;

    [SerializeField] public RectTransform _rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] public ToolTip _toolTip;

    public bool canBeDragged = true;
    
    [SerializeField] private AudioClip pickUp;
    [SerializeField] private float pickUpVol;
    [SerializeField] private float pickUpPitch;
    
    [SerializeField] private TextMeshProUGUI sellPrice;

    private bool isBeingDragged = false;

    private Character character;

    private InputHandler inputHandler;
    private Coroutine draggingRoutine = null;
    private DragItemHoverEffect hoverEffect;

    private Vector2 offsetWhenDragged = new Vector2(-50f, 50f);
    private UIStateMonitor stateMonitor;
    
    public bool IsItemFromInventory()
    {
        return currentLocation != null && currentLocation.Slot == Equipment.Slot.All;
    }

    public bool HasSameSlotType(Equipment.Slot otherSlotType)
    {
        return this.slotType == otherSlotType;
    }

    public bool IsShopItem()
    {
        return currentLocation.Slot == Equipment.Slot.Merchant;
    }

    public void HighlightItem(bool toHighlight)
    {
        GamepadHighlighter.enabled = toHighlight;

        if (toHighlight)
        {
            hoverEffect.GamepadSelect();
            _toolTip.ShowTipFromGamepadNavi(_rectTransform);
        }
        else
        {
            hoverEffect.GamepadDeselect();
            _toolTip.CloseTip();
        }
    }

    public bool CanBeDraggedInCombat()
    {
        if (e.isRelic) return false;

        var currentEnergy = character._currentEnergy;

        if (!character.isPlayerCharacter) return false;

        if (e.slot == Equipment.Slot.Scroll && RelicManager._instance.CheckRelic(RelicType.Relic1))
        {
            currentEnergy = 1;
        }
        if (e.slot == Equipment.Slot.OneHander && RelicManager._instance.CheckRelic(RelicType.Relic2))
        {
            currentEnergy = 1;
        }

        return currentEnergy >= 0;
    }

    public void HandleForgeHighlight(bool isOn)
    {
        ForgeableHighlighter.ToggleHighlighter(isOn);
    }

    public bool IsGear()
    {
        return slotType >= Equipment.Slot.Head && slotType <= Equipment.Slot.Scroll;
    }

    public bool IsForgeable()
    {
        return e.IsGear();       
    }

    public bool HasFundsForForge(ForgeMode mode, out int forgeCost)
    {
        float priceMod = ForgeManager._instance.priceMod;

        if (priceMod <= 0)
        {
            forgeCost = 0;
            return true;
        }

        forgeCost = ForgeManager._instance.GetUpgradePrice(mode, e);

        if (CombatController._instance.Player._gold <= forgeCost)
        {
            return false;
        }

        return true;
    }

    public bool TryUpgrade()
    {
        if(IsGear() && HasFundsForForge(ForgeMode.Upgrade, out int forgeCost))
        {
            EquipmentManager._instance.UpgradeEquipment(this);
            CombatController._instance.Player.GetGold(-forgeCost);
            UIController._instance.PlayUpgradeSound();
            ForgeManager._instance.ShowIcon();
            ForgeManager._instance.ShowForgePrice(e);            
            return true;
        }

        currentLocation.NotEnoughGoldEvent();
        UIController._instance.PlayUIError();
        return false;
    }

    public bool TryEnhance()
    {
        if (e.stats[Stats.Rarity] >= 3)
        {
            UIController._instance.PlayUIError();
            return false;
        }
        if(IsGear() && HasFundsForForge(ForgeMode.Enhance, out int forgeCost))
        {
            EquipmentManager._instance.EnhanceEquipment(this);
            CombatController._instance.Player.GetGold(-forgeCost);
            UIController._instance.PlayEnhanceSound();
            ForgeManager._instance.ShowIcon();
            ForgeManager._instance.ShowForgePrice(e);
            return true;
        }

        currentLocation.NotEnoughGoldEvent();
        UIController._instance.PlayUIError();
        return false;
    }

    public void ShowForgePrice(bool value)
    {
        if (!value)
        {
            ForgeManager._instance.HidePrice();
            return;
        }

        ForgeManager._instance.SetForgeLabelPosition(_rectTransform);
        ForgeManager._instance.ShowForgePrice(e);
    }

    public void GamepadStartDrag(InventorySlot origin, InputSource source)
    {
        Debug.LogError($"DragItem.GamepadStartDrag() should not execute!");
        return;
        if (e.isRelic) return;

        AdjustDragabilityBasedOnEnergy(CombatController._instance.Player, CombatController._instance.Player._currentEnergy);  //, 1,1); looks like these last 2 argument isn't being used in the function


        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = .6f;
        SoundManager.Instance.Play2DSFX(pickUp, pickUpVol, pickUpPitch, .05f);
        UIController._instance.StateMonitor.SetItemOnGamepad(this);

        HighlightItem(false);
        
        if(draggingRoutine != null)
        {
            StopCoroutine(draggingRoutine);
            draggingRoutine = null;
        }

        if (source == InputSource.Gamepad)
            draggingRoutine = StartCoroutine(GamepadDragRoutine());
        else if (source == InputSource.MouseKeyboard)
            draggingRoutine = StartCoroutine(MouseDragRoutine());
    }

    public void GamepadFinishDrag(bool shouldHighlight = true)
    {
        Debug.LogError($"DragItem.GamepadFinishDrag() executed (should not!)");
        if(draggingRoutine != null)
        {
            StopCoroutine(draggingRoutine);
            draggingRoutine = null;
        }

        UIController._instance.StateMonitor.SetItemOnGamepad(null);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;
        _rectTransform.localScale = currentLocation._rt.localScale;
        HighlightItem(shouldHighlight);
    }

    private IEnumerator GamepadDragRoutine()
    {
        transform.SetAsLastSibling();
        _rectTransform.anchoredPosition += offsetWhenDragged;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        RectTransform currentRectTransform = currentSelected.GetComponent<RectTransform>();
        while (isBeingDragged)
        {
            //TODO: handle if during this process, mouse detected
            if(currentSelected != EventSystem.current.currentSelectedGameObject)
            {
                currentSelected = EventSystem.current.currentSelectedGameObject;
                transform.position = currentSelected.transform.position;
                _rectTransform.anchoredPosition += offsetWhenDragged;
                //currentRectTransform = currentSelected.GetComponent<RectTransform>();
                //_rectTransform.anchoredPosition = currentRectTransform.anchoredPosition;
            }

            yield return null;
        }
    }

    private IEnumerator MouseDragRoutine()
    {
        transform.SetAsLastSibling();
        //_rectTransform.anchoredPosition += offsetWhenDragged;

        var inputHandler = EventSystem.current.GetComponent<InputHandler>();

        while (isBeingDragged)
        {
            //TODO: handle if during this process, mouse detected
            var position = inputHandler.MousePosition;

            _rectTransform.position = position;

            yield return null;
        }
    }

    public void SetInventorySlot(InventorySlot slot)
    {        
        currentLocation = slot;
        var slotRT = currentLocation.GetComponent<RectTransform>();
        transform.SetParent(currentLocation.transform);

        _rectTransform.anchoredPosition = slotRT.anchoredPosition;
        _rectTransform.localScale = slotRT.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HighlightItem(true);

        if(e.IsGear())
        {
            ForgeManager._instance.SetForgeLabelPosition(_rectTransform);
            if (ForgeManager._instance.ForgeMode == ForgeMode.Upgrade)
            {
                ForgeManager._instance.ShowForgePrice(e);
            }

            else if (ForgeManager._instance.ForgeMode == ForgeMode.Enhance)
            {
                if (e.stats[Stats.Rarity] < 3)
                {
                    ForgeManager._instance.ShowForgePrice(e);
                }
            }
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HighlightItem(false);

        if (!e.IsGear()) return;

        if (ForgeManager._instance.ForgeMode != ForgeMode.None)
        {
            ForgeManager._instance.HidePrice();
        }
    }

    
    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("OnPointerDown");
        if(e.isRelic) return;

        AdjustDragabilityBasedOnEnergy(CombatController._instance.Player, CombatController._instance.Player._currentEnergy);  //, 1,1); looks like these last 2 argument isn't being used in the function
        
        if (ForgeManager._instance.ForgeMode == ForgeMode.Upgrade)
        {
            TryUpgrade();
        }
        if (ForgeManager._instance.ForgeMode == ForgeMode.Enhance)
        {
            TryEnhance();
        }
    }

    public void PrepItemForDrag()
    {
        canvasGroup.alpha = .6f;
        //transform.SetAsLastSibling();
    }

    public void PrepItemForReturn()
    {
        Debug.LogError($"on return, currentLocation = {currentLocation.name}");
        canvasGroup.alpha = 1f;
        transform.SetParent(currentLocation.transform.parent);
        _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;
        _rectTransform.localScale = currentLocation._rt.localScale;
    }

    public void PrepForRemove()
    {
        canvasGroup.alpha = 1f;
    }

    public void PrepItemForDrop(InventorySlot destinationSlot)
    {
        // see GamepadFinishDrag(bool shouldHighlight = true) for the working version
        Debug.LogError($"DragItem.PrepItemForDrop at {destinationSlot.name}");
        canvasGroup.alpha = 1f;
        currentLocation = destinationSlot;
        transform.SetParent(destinationSlot._rt.parent);
        _rectTransform.anchoredPosition = destinationSlot._rt.anchoredPosition;
        _rectTransform.localScale = destinationSlot._rt.localScale;
        HighlightItem(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.LogError($"DragItem.OnBeginDrag should not execute!");
        if(e.isRelic) return;

        AdjustDragabilityBasedOnEnergy(CombatController._instance.Player, CombatController._instance.Player._currentEnergy);  //, 1,1); looks like these last 2 argument isn't being used in the function

        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = .6f;
        _rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        SoundManager.Instance.Play2DSFX(pickUp, pickUpVol, pickUpPitch, .05f);
        UIController._instance.StateMonitor.SetItemOnGamepad(this);
        //Debug.Log("pickup");

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.LogError($"Mouse end drag here!");
        //canvasGroup.blocksRaycasts = true;
        //canvasGroup.alpha = 1f;
        //_rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;
        //UIController._instance.StateMonitor.SetItemOnGamepad(null);
        Debug.LogError($"DragItem.OnEndDrag should not execute!");

        if (draggingRoutine != null)
        {
            StopCoroutine(draggingRoutine);
            draggingRoutine = null;
        }

        UIController._instance.StateMonitor.SetItemOnGamepad(null);
        isBeingDragged = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;
        _rectTransform.localScale = currentLocation._rt.localScale;
        HighlightItem(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("dragging");
        if (e.isRelic) return;
        
        _rectTransform.anchoredPosition += eventData.delta/ canvas.scaleFactor;
        
        // check if we are over upgrade or sell
        // if (eventData.pointerDrag != null)
        // {
        //     InventorySlot i = eventData.pointerDrag.GetComponent<InventorySlot>();
        //     if (i == null)
        //     {
        //         return;
        //     }
        //     if (i.SellType != InventorySlot.SellShopType.None )
        //     {
        //         Debug.Log("hi");
        //     }
        // }

    }

    /* old version OnDrop
    public void OnDrop(PointerEventData eventData)
    {
        return;
        HighlightItem(false);
        //another DragItem being drop to this one, notify the slot
        if(currentLocation == null)
        {
            Debug.LogError($"Error! this item doesn't occupy an inventory slot!");
            return;
        }

        currentLocation.HandleMouseDropOnOccupiedSlot();

        return;

        isBeingDragged = false;
        if (canBeDragged == false)
        {
            //notification
            if (CombatController._instance.entitiesInCombat.Count > 1)
            {
                Debug.Log("Cannot drag in combat");
                CombatMove(ErrorMessageManager.Errors.CombatMove);
                
                SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
                return;
            }
        }
        if (eventData.pointerDrag != null)
        {
            DragItem di = eventData.pointerDrag.GetComponent<DragItem>();

            if (currentLocation.Slot != di.slotType && currentLocation.Slot != Equipment.Slot.All)
            {
                return;
            }
            
            if(di.currentLocation.Slot == Equipment.Slot.Merchant)
                return;

            //might need to handle sell item here
            
            if (slotType == di.slotType || currentLocation.Slot == di.currentLocation.Slot)
            {
                temp = currentLocation;
                currentLocation = di.currentLocation;
                currentLocation.Item = this;
                _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;
                
                

                di.currentLocation = temp;
                di._rectTransform.anchoredPosition = temp._rt.anchoredPosition;
                di.currentLocation.Item = di;
                temp = null;
                
                currentLocation.LabelCheck();
                di.currentLocation.LabelCheck();
                
                //Debug.Log(slotType +" "+ di.slotType);

                if (slotType != Equipment.Slot.All || currentLocation.Slot != Equipment.Slot.All)
                {
                    // it is in out equipment
                    //Debug.Log( di.e.name+ " => removing " + e.name);
                    if (this.currentLocation.Slot == Equipment.Slot.All)
                    {
                        EquipmentManager._instance.UnEquipItem(e);
                    }
                    EquipmentManager._instance.EquipFromInventory(di.e);
                    
                    
                }
                //EquipmentManager._instance.c.UpdateStats();

            }
        }
        else
        {
            SoundManager.Instance.Play2DSFX(UIController._instance.errorSFX, UIController._instance.errorVol, 1, .05f);
        }
    }
    */


    private string GetWeaponType(SpellTypes spell)
    {
        string name = "";
        switch (spell)
        {
            case SpellTypes.Dagger1:
                name = "Dagger";
                break;
            case SpellTypes.Dagger2:
                name = "Dagger";
                break;
            case SpellTypes.Shield1:
                name = "Shield";
                break;
            case SpellTypes.Shield2:
                name = "Shield";
                break;
            case SpellTypes.Sword1:
                name = "Sword";
                break;
            case SpellTypes.Sword2:
                name = "Sword";
                break;
            case SpellTypes.Axe1:
                name = "Axe";
                break;
            case SpellTypes.Axe2:
                name = "Axe";
                break;
            case SpellTypes.Hammer1:
                name = "Hammer";
                break;
            case SpellTypes.Hammer2:
                name = "Hammer";
                break;
            case SpellTypes.Nature1:
                name = "Nature";
                break;
            case SpellTypes.Nature2:
                name = "Nature";
                break;
            case SpellTypes.Nature3:
                name = "Nature";
                break;
            case SpellTypes.Nature4:
                name = "Nature";
                break;
            case SpellTypes.Fire1:
                name = "Fire";
                break;
            case SpellTypes.Fire2:
                name = "Fire";
                break;
            case SpellTypes.Fire3:
                name = "Fire";
                break;
            case SpellTypes.Fire4:
                name = "Fire";
                break;
            case SpellTypes.Ice1:
                name = "Ice";
                break;
            case SpellTypes.Ice2:
                name = "Ice";
                break;
            case SpellTypes.Ice3:
                name = "Ice";
                break;
            case SpellTypes.Ice4:
                name = "Ice";
                break;
            case SpellTypes.Blood1:
                name = "Blood";
                break;
            case SpellTypes.Blood2:
                name = "Blood";
                break;
            case SpellTypes.Blood3:
                name = "Blood";
                break;
            case SpellTypes.Blood4:
                name = "Blood";
                break;
            case SpellTypes.Shadow1:
                name = "Shadow";
                break;
            case SpellTypes.Shadow2:
                name = "Shadow";
                break;
            case SpellTypes.Shadow3:
                name = "Shadow";
                break;
            case SpellTypes.Shadow4:
                name = "Shadow";
                break;
            }

        return name;

    }

    private void AdjustDragabilityBasedOnEnergy( Character c, int cur)
    {
        if (!c.isPlayerCharacter)
        {
            return;
        }
        if (CombatController._instance.entitiesInCombat.Count <= 1)
        {
            canBeDragged = true;
            return;
        }

        if (e.slot == Equipment.Slot.Scroll && RelicManager._instance.CheckRelic(RelicType.Relic1))
        {
            cur = 1;
        }
        if (e.slot == Equipment.Slot.OneHander && RelicManager._instance.CheckRelic(RelicType.Relic2))
        {
            cur = 1;
        }
        
        if (cur <= 0)
        {
            canBeDragged = false;
        }
        else
        {
            canBeDragged = true;

        }

        if (e.isRelic)
        {
            canBeDragged = false;
        }
    }

    public void TurnOnSellPrice(int price)
    {
        sellPrice.text = price.ToString();
        sellPrice.gameObject.SetActive(true);
    }
   
    public void TurnOffSellPrice()
    {
        sellPrice.gameObject.SetActive(false);
    }


    public void InitializeDragItem(Equipment equip, InventorySlot location)
    {
        stateMonitor ??= UIController._instance.StateMonitor;

        // we have to clear the previous equipment
        //remove slot, remove stats
        _toolTip.ResetTooltip();
        _toolTip.is_item = true;

        canvas = UIController._instance.GetComponent<Canvas>();//location.transform.parent.parent.GetComponent<Canvas>();

        e = equip;
        currentLocation = location;
        currentLocation.Item = this;
        _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;

        _rectTransform.localScale = currentLocation._rt.localScale;
        currentLocation.LabelCheck();
        icon.sprite = e.icon;
        Background.sprite = BackgroundSprites[e.stats[Stats.Rarity]];
        Glow.sprite = GlowSprites[e.stats[Stats.Rarity]];
        if (e.stats[Stats.Rarity] == 0)
        {
            Glow.gameObject.SetActive(false);
        }
        else
        {
            Glow.gameObject.SetActive(true);
        }
        slotType = e.slot;

        if (slotType != currentLocation.Slot && currentLocation.Slot != Equipment.Slot.All && currentLocation.Slot != Equipment.Slot.Merchant)
        {
            //Debug.Log(slotType + " "+ currentLocation.Slot);
            Debug.Log("I think we fudged this one up bud");
        }
        if (e.slot != Equipment.Slot.Relic && e.slot != Equipment.Slot.Consumable)
        {
            _toolTip.iLvl = e.stats[Stats.ItemLevel].ToString();
            LvlText.text = "Lvl: " + e.stats[Stats.ItemLevel];


            _toolTip.rarity = e.stats[Stats.Rarity];
            _toolTip.Cost = "";
            _toolTip.Title = e.name;
            _toolTip.e = e;

        }
        else
        {
            if (e.slot == Equipment.Slot.Relic)
                _toolTip.is_relic = true;
            _toolTip.rarity = e.stats[Stats.Rarity];
            _toolTip.Title = e.name;
            _toolTip.e = e;
            canBeDragged = false;
            _toolTip.is_item = false;
        }
        //LvlText.color = ToolTipManager._instance.rarityColors[e.stats[Stats.Rarity]];
        if (!e.isWeapon)
        {
            _toolTip.Message += "Slot: " + e.slot + "\n";
        }
        foreach (var stat in e.stats)
        {
            if (stat.Key != Stats.Rarity && stat.Key != Stats.ItemLevel)
            {
                _toolTip.Message += stat.Key + ": " + stat.Value + "\n";
            }
        }

        if (e.isWeapon)
        {
            Weapon x = (Weapon)e;
            if (x.spellType1 != SpellTypes.None)
            {
                _toolTip.Cost = x.scalingInfo1[2].ToString();
                _toolTip.Message += x.scalingInfo1[0] + "\n";
            }
            if (x.spellType2 != SpellTypes.None)
            {
                _toolTip.Cost += ", " + x.scalingInfo2[2].ToString();
                _toolTip.Message += x.scalingInfo2[0] + "\n";


            }

            _toolTip.is_spell = true;

            _toolTip.Message = "Weapon: " + GetWeaponType(x.spellType1) + "\n" + _toolTip.Message;
        }

        this.gameObject.name = $"DragItem-{e.name}";
        //canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    private void Start()
    {
        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        hoverEffect = GetComponent<DragItemHoverEffect>();

        if (GamepadHighlighter.enabled)
            GamepadHighlighter.enabled = false;

        character = CombatController._instance.Player;
        character.UpdateEnergy += AdjustDragabilityBasedOnEnergy;
        //CombatController.EndCombatEvent += EndCombat;
    }
    private void OnDestroy()
    {
        character.UpdateEnergy -= AdjustDragabilityBasedOnEnergy;

        //CombatController.EndCombatEvent -= EndCombat;

    }


}
