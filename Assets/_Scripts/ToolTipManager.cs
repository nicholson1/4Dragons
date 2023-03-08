using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ToolTipManager : MonoBehaviour
{
   public static ToolTipManager _instance;

   public TextMeshProUGUI tiptext;
   public TextMeshProUGUI tiptitle;
   public TextMeshProUGUI spellCost;
   public TextMeshProUGUI iLvl;
   public TextMeshProUGUI rarity;

   [SerializeField] public Color[] rarityColors;
   private RectTransform _rt;

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
         _rt.pivot += new Vector2(3, 0);
      }

      
      transform.position = Input.mousePosition;
      

   }

   public void SetAndShowToolTip(string title, string message, string cost = "", string itemLvl = "", int itemRarity = -1)
   {
      transform.position = Input.mousePosition;
      SetRarityText(itemRarity);
      gameObject.SetActive(true);
      tiptext.text = message;
      spellCost.text = cost;
      tiptitle.text = title;
      tiptitle.color = rarity.color;
      if (itemLvl != "")
      {
         iLvl.text = "Lvl. " + itemLvl;

      }
      //Debug.Log(itemRarity);
      


   }

   private void SetRarityText(int r)
   {
      switch (r)
      {
         case 0:
            rarity.text = "Common";
            rarity.color = rarityColors[0];
            break;
         case 1:
            rarity.text = "Uncommon";
            rarity.color = rarityColors[1];
            break;
         case 2:
            rarity.text = "Rare";
            rarity.color = rarityColors[2];
            break;
         case 3:
            rarity.text = "Epic";
            rarity.color = rarityColors[3];
            break;
         case -1 :
            rarity.text = "";
            rarity.color = Color.white;
            break;
            
      }
   }
   
   

   public void HideToolTip()
   {
      gameObject.SetActive(false);
      spellCost.text = string.Empty;
      tiptext.text = string.Empty;
      tiptitle.text = string.Empty;
      iLvl.text = string.Empty;
      rarity.text = string.Empty;
   }
}
