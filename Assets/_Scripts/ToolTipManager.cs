using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolTipManager : MonoBehaviour
{
   public static ToolTipManager _instance;

   public ToolTipDisplay MainTip;
   public ToolTipDisplay SpellTip;
   public ToolTipDisplay RelicTip;
   public ToolTipDisplay ItemTip;
   public ToolTipDisplay Comparison1tip;
   public ToolTipDisplay Comparison2tip;

   public Character c;

   
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

      if (c == null)
      {
         c = CombatController._instance.Player;
      }
   }

   private void Update()
   {
      // scale based on reference resolution
      
      //Debug.Log((Screen.height - Input.mousePosition.y) + " Y");
      //Debug.Log((Screen.width - Input.mousePosition.x) + " X");

      _rt.pivot = new Vector2(.5f, .5f);
      if (Screen.height - Input.mousePosition.y < Screen.height/3.5f)
      {
         _rt.pivot += new Vector2(0, 3);
      }
      if (Screen.width - Input.mousePosition.x < Screen.width/5f)
      {
         _rt.pivot += new Vector2(5, 0);
         //LayoutGroup.reverseArrangement = true;
      }
      //LayoutGroup.reverseArrangement = false;


      
      transform.position = Input.mousePosition;
      

   }

   public void SetAndShowToolTip(string title, string message, string cost , string itemLvl, int itemRarity, Sprite i, Color c, bool is_Spell, bool is_item, Equipment equip, bool is_relic)
   {
      transform.position = Input.mousePosition;


      if (activated)
      {
         return;
      }
      
      Comparison1tip.gameObject.SetActive(false);
      Comparison2tip.gameObject.SetActive(false);
      
      if (is_Spell)
      {
        UpdateSpellTip(SpellTip, equip);
      }
      else
      {
         SpellTip.gameObject.SetActive(false);

      }

      if (is_relic)
      {
         UpdateRelicTip(RelicTip, equip);
      }
      else
      {
         RelicTip.gameObject.SetActive(false);
      }

      if (is_item)
      {
         UpdateItemTipDisplay(ItemTip, title , itemLvl, itemRarity, equip);
         UpdateComparisonTip(equip);
      }
      else
      {
         ItemTip.gameObject.SetActive(false);
      }

      if (!is_Spell && !is_item && !is_relic)
      {

         UpdateTipDisplay(MainTip, title, message, cost, itemLvl, itemRarity, i, c, is_Spell, is_item);
      }
      else
      {
         MainTip.gameObject.SetActive(false);
      }

      activated = true;

      //Debug.Log("hello");


   }

   private void UpdateComparisonTip( Equipment newItem)
   {
      if (c == null)
      {
         c = CombatController._instance.Player;
      }
      
      Comparison1tip.gameObject.SetActive(false);
      Comparison2tip.gameObject.SetActive(false);

      if (c._equipment.Contains(newItem))
      {
         return;
      }
      
      Equipment oldItem1 = null;
      Equipment oldItem2 = null;
      // get old item or items
      if (newItem.isWeapon)
      {
         if (newItem.slot == Equipment.Slot.Scroll)
         {
            
            return;
         }
         else
         {
            // need to get both
            if (c._weapons.Count == 0)
            {
               
               return;
            }else if (c._weapons.Count == 1)
            {
               oldItem1 = c._weapons[0];
            }
            else
            {
               oldItem1 = c._weapons[0];
               oldItem2 = c._weapons[1];

            }
         }
      }
      else
      {
         // check if we have something equipped
         
         foreach (var eq in c._equipment)
         {
            if (eq.slot == newItem.slot)
            {
               oldItem1 = eq;
            }
         }
      }
      //if nothign equipped in that slot stop
      if (oldItem1 == null)
      {
         return;
      }
         
      //make a list of gained stats, make a list of lossed stats
      if (oldItem1 != null)
      {
         UpdateGetGainsAndLosses(Comparison1tip, newItem, oldItem1);
         Comparison1tip.gameObject.SetActive(true);
         Comparison1tip.tiptitle.text = oldItem1.name;
         Comparison1tip.tiptitle.color = rarityColors[oldItem1.stats[Equipment.Stats.Rarity]];

      }
      if (oldItem2 != null)
      {
         UpdateGetGainsAndLosses(Comparison2tip, newItem, oldItem2);
         Comparison2tip.gameObject.SetActive(true);
         Comparison2tip.tiptitle.text = oldItem2.name;
         Comparison2tip.tiptitle.color = rarityColors[oldItem2.stats[Equipment.Stats.Rarity]];

      }
   }

   private void UpdateGetGainsAndLosses(ToolTipDisplay current, Equipment newItem,
      Equipment oldItem)
   {
      Dictionary<Equipment.Stats, int> Gains = new Dictionary<Equipment.Stats, int>();
      Gains.AddRange(newItem.stats);
      Dictionary<Equipment.Stats, int> Losses = new Dictionary<Equipment.Stats, int>();

      foreach (var kvp in oldItem.stats)
      {
         if (!Gains.ContainsKey(kvp.Key))
         {
            Losses.Add(kvp.Key, -kvp.Value);
         }
         else
         {
            Gains[kvp.Key] -= kvp.Value;

            if (Gains[kvp.Key] == 0)
            {
               Gains.Remove(kvp.Key);
            }else if (Gains[kvp.Key] < 0)
            {
               Losses.Add(kvp.Key, Gains[kvp.Key]);
               Gains.Remove(kvp.Key);
            }
         }
      }
      
      foreach (var sd in current.stats)
      {
         sd.gameObject.SetActive(true);
      }
      foreach (var sd in current.lossesStats)
      {
         sd.gameObject.SetActive(true);
      }

      //current.gameObject.SetActive(true);
      
      
      transform.position = Input.mousePosition;
      gameObject.SetActive(true);
      
      int count = 0;
      foreach (var kvp in Gains)
      {
         if (kvp.Key != Equipment.Stats.Rarity && kvp.Key != Equipment.Stats.ItemLevel)
         {
            current.stats[count].UpdateValues(kvp.Key, kvp.Value, 1);
            count += 1;
                
         }
      }
      //hide the ones that arnt being used
      for (int i = current.stats.Length -1; i > count-1 ; i--)
      {
         current.stats[i].gameObject.SetActive(false);
      }
      
      int count2 = 0;
      foreach (var kvp in Losses)
      {
         if (kvp.Key != Equipment.Stats.Rarity && kvp.Key != Equipment.Stats.ItemLevel)
         {
            current.lossesStats[count2].UpdateValues(kvp.Key, kvp.Value, -1);
            count2 += 1;
                
         }
      }
      //hide the ones that arnt being used
      for (int i = current.lossesStats.Length -1; i > count2-1 ; i--)
      {
         current.lossesStats[i].gameObject.SetActive(false);
      }
   }
   
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
      current.iLvl.text = "Lvl: " +e.stats[Equipment.Stats.ItemLevel].ToString();
      current.iLvl.color = rarityColors[e.stats[Equipment.Stats.Rarity]];
      gameObject.SetActive(true);
   }
   private void UpdateRelicTip(ToolTipDisplay current, Equipment e)
   {
      RelicTip.gameObject.SetActive(true);

      //Debug.Log("Updating spell tip");
      Relic r = (Relic) e;

      current.tiptext.text = r.relicDescription;
        
      current.tiptitle.text = r.name;
      current.tiptitle.color = rarityColors[4];
      current.icon.sprite = r.getIcon;
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
         if (e.slot == Equipment.Slot.Scroll)
         {
            current.slot.text = "Scroll";

         }
         else
         {
            current.slot.text = "Weapon";

         }

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
      //Debug.Log("Hide Tool Tip");

      //Debug.Log("Hide tool tip");
      HideToolTip(MainTip);
      HideToolTip(ItemTip);
      HideToolTip(SpellTip);
      activated = false;
      //Debug.Log("hiiii");
      gameObject.SetActive(false);

   }
   public void HideToolTip(ToolTipDisplay current)
   {
      //Debug.Log("Hide tool tip");
      current.gameObject.SetActive(false);
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
      message = message.Replace("#", (Mathf.FloorToInt(amount/4)*4).ToString());
        

      return message;

   }

  
}
