using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragItem : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    public Equipment e;
    public InventorySlot currentLocation;
    private InventorySlot temp;
    public Image icon;
    public Image Background;
    public Image Glow;


    [SerializeField] private Sprite[] BackgroundSprites;
    [SerializeField] private Sprite[] GlowSprites;


    public Equipment.Slot slotType;
    public TextMeshProUGUI LvlText;
    
    public static event Action<ErrorMessageManager.Errors> CombatMove;



    [SerializeField] public RectTransform _rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private ToolTip _toolTip;

    public bool canBeDragged = true;
    
    [SerializeField] private AudioClip pickUp;
    [SerializeField] private float pickUpVol;
    [SerializeField] private float pickUpPitch;
    
    

    public void InitializeDragItem(Equipment equip, InventorySlot location)
    {
        canvas = location.transform.parent.parent.GetComponent<Canvas>();
        e = equip;
        currentLocation = location;
        currentLocation.Item = this;
        _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;

        _rectTransform.localScale = currentLocation._rt.localScale;
        currentLocation.LabelCheck();
        icon.sprite = e.icon;
        Background.sprite = BackgroundSprites[e.stats[Equipment.Stats.Rarity]];
        Glow.sprite = GlowSprites[e.stats[Equipment.Stats.Rarity]];
        if (e.stats[Equipment.Stats.Rarity] == 0)
        {
            Glow.gameObject.SetActive(false);
        }
        else
        {
            Glow.gameObject.SetActive(true);
        }
        slotType = e.slot;

        if (slotType != currentLocation.Slot && currentLocation.Slot != Equipment.Slot.All && currentLocation.Slot != Equipment.Slot.Sold)
        {
            //Debug.Log(slotType + " "+ currentLocation.Slot);
            Debug.Log("I think we fudged this one up bud");
        }
        if(e.slot != Equipment.Slot.Relic && e.slot != Equipment.Slot.Consumable)
        {
            _toolTip.iLvl = e.stats[Equipment.Stats.ItemLevel].ToString();
            LvlText.text = "Lvl: " + e.stats[Equipment.Stats.ItemLevel];
            

            _toolTip.rarity = e.stats[Equipment.Stats.Rarity];
            _toolTip.Cost = "";
            _toolTip.Title = e.name;
            _toolTip.e = e;

        }
        else
        {
            if(e.slot == Equipment.Slot.Relic)
                _toolTip.is_relic = true;
            _toolTip.rarity = e.stats[Equipment.Stats.Rarity];
            _toolTip.Title = e.name;
            _toolTip.e = e;
            canBeDragged = false;
            _toolTip.is_item = false;
        }
        //LvlText.color = ToolTipManager._instance.rarityColors[e.stats[Equipment.Stats.Rarity]];
        if (!e.isWeapon)
        {
            _toolTip.Message += "Slot: " + e.slot + "\n";
        }
        foreach (var stat in e.stats)
        {
            if (stat.Key != Equipment.Stats.Rarity && stat.Key != Equipment.Stats.ItemLevel )
            {
                _toolTip.Message += stat.Key +": "+ stat.Value + "\n";
            }
        }
        
        if (e.isWeapon)
        {
            Weapon x = (Weapon) e;
            if (x.spellType1 != Weapon.SpellTypes.None)
            {
                _toolTip.Cost = x.scalingInfo1[2].ToString();
                _toolTip.Message += x.scalingInfo1[0]+ "\n";
            }
            if (x.spellType2 != Weapon.SpellTypes.None)
            {
                _toolTip.Cost += ", " + x.scalingInfo2[2].ToString();
                _toolTip.Message += x.scalingInfo2[0]+ "\n";

                
            }

            _toolTip.is_spell = true;

            _toolTip.Message = "Weapon: " + GetWeaponType(x.spellType1) + "\n" + _toolTip.Message;
        }

    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("OnPointerDown");
        if(e.isRelic)
            return;
        AdjustDragabilityBasedOnEnergy(CombatController._instance.Player, CombatController._instance.Player._currentEnergy, 1,1);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(e.isRelic)
            return;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = .6f;
        SoundManager.Instance.Play2DSFX(pickUp, pickUpVol, pickUpPitch, .05f);
        //Debug.Log("pickup");

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        _rectTransform.anchoredPosition = currentLocation._rt.anchoredPosition;
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("dragging");
        
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

    public void OnDrop(PointerEventData eventData)
    {
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
            
            if(di.currentLocation.Slot == Equipment.Slot.Sold)
                return;
            
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
    private string GetWeaponType(Weapon.SpellTypes spell)
    {
        string name = "";
        switch (spell)
        {
            case Weapon.SpellTypes.Dagger1:
                name = "Dagger";
                break;
            case Weapon.SpellTypes.Dagger2:
                name = "Dagger";
                break;
            case Weapon.SpellTypes.Shield1:
                name = "Shield";
                break;
            case Weapon.SpellTypes.Shield2:
                name = "Shield";
                break;
            case Weapon.SpellTypes.Sword1:
                name = "Sword";
                break;
            case Weapon.SpellTypes.Sword2:
                name = "Sword";
                break;
            case Weapon.SpellTypes.Axe1:
                name = "Axe";
                break;
            case Weapon.SpellTypes.Axe2:
                name = "Axe";
                break;
            case Weapon.SpellTypes.Hammer1:
                name = "Hammer";
                break;
            case Weapon.SpellTypes.Hammer2:
                name = "Hammer";
                break;
            case Weapon.SpellTypes.Nature1:
                name = "Nature";
                break;
            case Weapon.SpellTypes.Nature2:
                name = "Nature";
                break;
            case Weapon.SpellTypes.Nature3:
                name = "Nature";
                break;
            case Weapon.SpellTypes.Nature4:
                name = "Nature";
                break;
            case Weapon.SpellTypes.Fire1:
                name = "Fire";
                break;
            case Weapon.SpellTypes.Fire2:
                name = "Fire";
                break;
            case Weapon.SpellTypes.Fire3:
                name = "Fire";
                break;
            case Weapon.SpellTypes.Fire4:
                name = "Fire";
                break;
            case Weapon.SpellTypes.Ice1:
                name = "Ice";
                break;
            case Weapon.SpellTypes.Ice2:
                name = "Ice";
                break;
            case Weapon.SpellTypes.Ice3:
                name = "Ice";
                break;
            case Weapon.SpellTypes.Ice4:
                name = "Ice";
                break;
            case Weapon.SpellTypes.Blood1:
                name = "Blood";
                break;
            case Weapon.SpellTypes.Blood2:
                name = "Blood";
                break;
            case Weapon.SpellTypes.Blood3:
                name = "Blood";
                break;
            case Weapon.SpellTypes.Blood4:
                name = "Blood";
                break;
            case Weapon.SpellTypes.Shadow1:
                name = "Shadow";
                break;
            case Weapon.SpellTypes.Shadow2:
                name = "Shadow";
                break;
            case Weapon.SpellTypes.Shadow3:
                name = "Shadow";
                break;
            case Weapon.SpellTypes.Shadow4:
                name = "Shadow";
                break;
            }

        return name;

    }

    private void AdjustDragabilityBasedOnEnergy( Character c, int cur, int max, int amount)
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
   
    private void Start()
    {
        Character.UpdateEnergy += AdjustDragabilityBasedOnEnergy;
        //CombatController.EndCombatEvent += EndCombat;
    }
    private void OnDestroy()
    {
        Character.UpdateEnergy -= AdjustDragabilityBasedOnEnergy;

        //CombatController.EndCombatEvent -= EndCombat;

    }
}
