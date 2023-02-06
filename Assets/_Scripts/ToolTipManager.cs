using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToolTipManager : MonoBehaviour
{
   public static ToolTipManager _instance;

   public TextMeshProUGUI tiptext;
   public TextMeshProUGUI tiptitle;
   public TextMeshProUGUI spellCost;


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
   }

   private void Update()
   {
      transform.position = Input.mousePosition;
   }

   public void SetAndShowToolTip(string title, string message, string cost = "")
   {
      gameObject.SetActive(true);
      tiptext.text = message;
      spellCost.text = cost;
      tiptitle.text = title;

   }

   public void HideToolTip()
   {
      gameObject.SetActive(false);
      spellCost.text = string.Empty;
      tiptext.text = string.Empty;
      tiptitle.text = string.Empty;
   }
}
