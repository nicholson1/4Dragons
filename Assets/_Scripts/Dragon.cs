using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Dragon : MonoBehaviour
{

   [SerializeField] private Character c;
   
   public SpellType spellType;
   public DragonType dragonType;

   

   private List<int> PhysicalSpellIndexes =  new List<int>() { 1, 2, 4, 5, 7, 8, 10, 11, 13, 14 };
   private List<int> MagicSpellIndexes =  new List<int>() {};


   [SerializeField] private GameObject[] Models;
   [SerializeField] private Material[] Materials;

   public void InitializeDragon()
   {
      int rarity = -1;
      //randomSpellType
      spellType = (SpellType)Random.Range(0, 5);
      //random drag depending on level
      if (c._level == 10)
      {
         dragonType = DragonType.Nightmare;
         rarity = 1;
      }
      else if (c._level == 20)
      {
         rarity = 2;
         dragonType = DragonType.TerrorBringer;

      }
      else if (c._level == 25)
      {
         rarity = 3;
         dragonType = DragonType.SoulEater;
      }
      else if (c._level == 30)
      {
         rarity = 3;
         dragonType = DragonType.Usurper;

      }
      else
      {
         dragonType = (DragonType)Random.Range(0, 4);
         //dragonType = DragonType.Usurper;


      }

      c._equipment = CreateAllDragonEquipment(c._level, rarity);
      c._spellScrolls = GetDragonSpells(c._level, rarity, spellType, dragonType);
      
      c._gold = c._level * 10 + (Random.Range(-c._level, c._level+1));

   }

   private List<Weapon> GetDragonSpells(int level, int rarity, SpellType spellSchool, DragonType dragType )
   {
      List<Weapon> Spells = new List<Weapon>();
      int physicalSpellCount = 0;
      int magicSpellCount = 0;

      int materialIndex = 0;
      int modelIndex = 0;

      //based on dragon type there are a diff number of physical and spell abilities

      switch (spellSchool)
      {
         case SpellType.Nature:
            MagicSpellIndexes =  new List<int>() {15,16,17,18,19};
            materialIndex = 0;
            break;
         case SpellType.Fire:
            MagicSpellIndexes =  new List<int>() {20,21,22,23,24};
            materialIndex = 1;
            break;
         case SpellType.Ice:
            MagicSpellIndexes =  new List<int>() {25,26,27,28,29};
            materialIndex = 2;
            break;
         case SpellType.Blood:
            MagicSpellIndexes =  new List<int>() {30,31,32,33,34};
            materialIndex = 3;
            break;
         case SpellType.Shadow:
            MagicSpellIndexes =  new List<int>() {35,36,37,38,39};
            materialIndex = 4;
            break;
      }

      switch (dragType)
      {
         case DragonType.Nightmare:
            physicalSpellCount = 4;
            magicSpellCount = 1;
            modelIndex = 0;
            break;
         case DragonType.TerrorBringer:
            physicalSpellCount = 3;
            magicSpellCount = 2;
            modelIndex = 1;
            break;
         case DragonType.SoulEater:
            physicalSpellCount = 2;
            magicSpellCount = 3;
            modelIndex = 2;
            break;
         case DragonType.Usurper:
            physicalSpellCount = 1;
            magicSpellCount = 4;
            modelIndex = 3;
            break;
      }

      int roll;

      for (int i = 0; i < physicalSpellCount; i++)
      {
         roll = PhysicalSpellIndexes[Random.Range(0, PhysicalSpellIndexes.Count)];
         Spells.Add(c.EC.CreateSpellScroll(level, rarity, (Weapon.SpellTypes)roll));
         Debug.Log((Weapon.SpellTypes)roll);
         PhysicalSpellIndexes.Remove(roll);
         //physicalSpellCount -= 1;
      }
        
      Debug.Log(spellType);
      Debug.Log(MagicSpellIndexes.Count);
      for (int i = 0; i < magicSpellCount; i++)
      {
         roll = MagicSpellIndexes[Random.Range(0, MagicSpellIndexes.Count)];
         Spells.Add(c.EC.CreateSpellScroll(level, rarity, (Weapon.SpellTypes)roll));
         Debug.Log((Weapon.SpellTypes)roll);
         MagicSpellIndexes.Remove(roll);
         //magicSpellCount -= 1;
         
      }

      SetModelAndMaterial(modelIndex, materialIndex);

      return Spells;
   }

   private void SetModelAndMaterial(int modelIndex, int materialIndex)
   {
      Models[modelIndex].gameObject.SetActive(true);
      c._am = Models[modelIndex].GetComponent<Animator>();
      Models[modelIndex].GetComponentInChildren<SkinnedMeshRenderer>().material = Materials[materialIndex + (5 * modelIndex)];
   }

   private bool ContainsSpell(List<Weapon> Spells, Weapon.SpellTypes spellType)
   {
      bool haveSpell = false;
      foreach (var w in Spells)
      {
         if (w.spellType1 == spellType)
         {
            haveSpell = true;
            return haveSpell;
         }
      }
      return haveSpell;

   }
   
   public List<Equipment> CreateAllDragonEquipment(int level, int rarity)
   {
      List<Equipment> generatedEquipment = new List<Equipment>();

      if (c.EC == null)
      {
         c.EC = FindObjectOfType<EquipmentCreator>();

      }
      //only the first 6 elements
      var v = Enum.GetValues (typeof (Equipment.Slot));
      int i = 0;
      while (i < 6)
      {
         //int level = Random.Range(1, 20);
            
         generatedEquipment.Add(c.EC.CreateArmor(level, (Equipment.Slot)i,rarity, Equipment.Stats.Strength, Equipment.Stats.SpellPower ));
         i++;
      }

      return generatedEquipment;
   }

   public enum SpellType
   {
      Nature,
      Ice,
      Fire,
      Blood,
      Shadow
   }
   
   public enum DragonType
   {
      Nightmare,
      TerrorBringer,
      SoulEater,
      Usurper,
   }
}
