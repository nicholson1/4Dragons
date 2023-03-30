using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolTipManager : MonoBehaviour
{
   public static ToolTipManager _instance;

   public ToolTipDisplay MainTip;
   public ToolTipDisplay SpellTip;
   public ToolTipDisplay ItemTip;

   public bool activated = false;

   // public TextMeshProUGUI tiptext;
   // public TextMeshProUGUI tiptitle;
   // public TextMeshProUGUI spellCost;
   // public TextMeshProUGUI iLvl;
   // public TextMeshProUGUI rarity;
   // public Image icon;


   [SerializeField] public Color[] rarityColors;
   private RectTransform _rt;

   //public HorizontalLayoutGroup LayoutGroup;
   private void Awake()
   {
      if (_instance != null && _instance != this)
      {
         Destroy(this.gameObject);
      }
      else
      {
         _instance = this;
      }
   }

   private void Start()
   {
      Cursor.visible = true;
      gameObject.SetActive(false);
      _rt = GetComponent<RectTransform>();
   }

   private void Update()
   {
      
      //Debug.Log((Screen.height - Input.mousePosition.y) + " Y");
      //Debug.Log((Screen.width - Input.mousePosition.x) + " X");

      _rt.pivot = new Vector2(.5f, .5f);
      if (Screen.height - Input.mousePosition.y < 130)
      {
         _rt.pivot += new Vector2(0, 3);
      }
      if (Screen.width - Input.mousePosition.x < 150)
      {
         _rt.pivot += new Vector2(5, 0);
         //LayoutGroup.reverseArrangement = true;
      }
      //LayoutGroup.reverseArrangement = false;


      
      transform.position = Input.mousePosition;
      

   }

   public void SetAndShowToolTip(string title, string message, string cost , string itemLvl, int itemRarity, Sprite i, Color c, bool is_Spell, bool is_item, Equipment equip)
   {

      if (activated)
      {
         return;
      }
      
      if (is_Spell)
      {
        UpdateSpellTip(SpellTip, equip);
      }
      else
      {
         SpellTip.gameObject.SetActive(false);

      }

      if (is_item)
      {
         UpdateItemTipDisplay(ItemTip, title , itemLvl, itemRarity, equip);

      }
      else
      {
         ItemTip.gameObject.SetActive(false);
      }

      if (!is_Spell && !is_item)
      {
         UpdateTipDisplay(MainTip, title, message, cost, itemLvl, itemRarity, i, c, is_Spell, is_item);

      }
      else
      {
         MainTip.gameObject.SetActive(false);
      }

      activated = true;

      Debug.Log("hello");


   }

   //todo MAYBE WE CANT DO THIS ALL AT ONCE OR IF WE DO WE PASS IN A SPELL OR WEAPON, AND THEN FIGURE OUT DATA ON THIS, I THINK THAT WOULD WORK
   private void UpdateSpellTip(ToolTipDisplay current, Equipment e)
   {
      SpellTip.gameObject.SetActive(true);

      //Debug.Log("Updating spell tip");
      Weapon w = (Weapon) e;
      Weapon.SpellTypes s = w.spellType1;
      (string, Sprite, Color, string) info = StatDisplayManager._instance.GetValuesFromSpell(s);

        
      current.icon.sprite = info.Item2;
      current.icon.color = info.Item3;
      // current.text.color = info.Item3;
      // current.spell = s;
      //   
      // toolTip.e = w;
      // toolTip.icon = icon.sprite;
      // toolTip.IconColor = icon.color;

      current.tiptext.text = info.Item4;
      List<List<object>> DataTable = DataReader._instance.GetWeaponScalingTable();
      List<int> power = TheSpellBook._instance.GetPowerValues(s, w, CombatController._instance.Player._combatEntity);
        
      current.tiptitle.text = DataTable[(int)s][0].ToString();;
      current.tiptitle.color = info.Item3;
      current.tiptext.text = AdjustDescriptionValues(DataTable[(int)s][3].ToString(), power[1], power[0]);
      current.spellCost.text = DataTable[(int)s][2].ToString();
      gameObject.SetActive(true);



   }
   private void UpdateTipDisplay(ToolTipDisplay current, string title, string message, string cost , string itemLvl, int itemRarity, Sprite i, Color c,  bool is_Spell, bool is_item)
   {
      current.gameObject.SetActive(true);
      
      if (i == null)
      {
         current.icon.gameObject.SetActive(false);
      }
      else
      {
         current.icon.sprite = i;
         current.icon.color = c;
         current.icon.gameObject.SetActive(true);

         // figure out the color
      }
      
      transform.position = Input.mousePosition;
      SetRarityText(itemRarity, current);
      gameObject.SetActive(true);
      current.tiptext.text = message;
      current.spellCost.text = cost;
      current.tiptitle.text = title;

      if (itemRarity != -1 && !is_Spell)
      {
         current.tiptitle.color = current.rarity.color;
      }
      else
      {
         current.tiptitle.color = c;
      }
      if (itemLvl != "")
      {
         current.iLvl.text = "Lvl: " + itemLvl;
         current.iLvl.color = current.rarity.color = rarityColors[0];

      }
   }
   private void UpdateItemTipDisplay(ToolTipDisplay current, string title, string itemLvl, int itemRarity, Equipment e)
   {
      //Debug.Log("hellllo");
     
      //make them all active
      foreach (var sd in current.stats)
      {
         sd.gameObject.SetActive(true);
      }
      

      current.gameObject.SetActive(true);
      
      
      transform.position = Input.mousePosition;
      SetRarityText(itemRarity, current);
      gameObject.SetActive(true);
      current.tiptitle.text = title;

      if (itemRarity != -1 )
      {
         current.tiptitle.color = current.rarity.color;
      }
      
      if (itemLvl != "")
      {
         current.iLvl.text = "Lvl: " + itemLvl;
         current.iLvl.color = current.rarity.color = rarityColors[0];

      }
      
      
      int count = 0;
      foreach (var kvp in e.stats)
      {
         if (kvp.Key != Equipment.Stats.Rarity && kvp.Key != Equipment.Stats.ItemLevel)
         {
            current.stats[count].UpdateValues(kvp.Key, kvp.Value);
            count += 1;
                
         }
      }
      //hide the ones that arnt being used
      for (int i = current.stats.Length -1; i > count-1 ; i--)
      {
         current.stats[i].gameObject.SetActive(false);
      }

      current.slot.text = e.slot.ToString();
      if (e.isWeapon)
      {
         current.slot.text = "Weapon";
      }
   }

   private void SetRarityText(int r, ToolTipDisplay current)
   {
      switch (r)
      {
         case 0:
            current.rarity.text = "Common";
            current.rarity.color = rarityColors[0];
            break;
         case 1:
            current.rarity.text = "Uncommon";
            current.rarity.color = rarityColors[1];
            break;
         case 2:
            current.rarity.text = "Rare";
            current.rarity.color = rarityColors[2];
            break;
         case 3:
            current.rarity.text = "Epic";
            current.rarity.color = rarityColors[3];
            break;
         case -1 :
            current.rarity.text = "";
            current.rarity.color = Color.white;
            break;
            
      }
   }
   
   

   public void HideToolTipAll()
   {
      if (activated == false)
      {
         return;
      }
      //Debug.Log("Hide tool tip");
      HideToolTip(MainTip);
      HideToolTip(ItemTip);
      HideToolTip(SpellTip);
      activated = false;
      //Debug.Log("hiiii");

   }
   public void HideToolTip(ToolTipDisplay current)
   {
      //Debug.Log("Hide tool tip");
      gameObject.SetActive(false);
      current.spellCost.text = string.Empty;
      current.tiptext.text = string.Empty;
      current.tiptitle.text = string.Empty;
      current.iLvl.text = string.Empty;
      current.rarity.text = string.Empty;

      
   }
   public string AdjustDescriptionValues(string message, int turns, float amount)
   {
      message = message.Replace("$", turns.ToString());
      message = message.Replace("@", amount.ToString());
      message = message.Replace("#", (Mathf.RoundToInt(amount/4)*4).ToString());
        

      return message;

   }

  
}
