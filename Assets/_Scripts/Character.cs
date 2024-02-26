using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ImportantStuff;
using Map;
using UnityEngine;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    private string _name;
    public int _level;
    public int _experience;
    public int _gold;
    public int _currentHealth;
    public int _maxHealth;
    public int _maxEnergy;
    public int _currentEnergy;

    [SerializeField] public Animator _am;

    public bool isPlayerCharacter;
    public bool isDragon;
    public bool isElite;


    public int inventorySize = 6;
    public List<Equipment> _equipment = new List<Equipment>();
    public List<Equipment> _inventory = new List<Equipment>();
    public List<Equipment> _Relics = new List<Equipment>();


    public List<Weapon> _weapons = new List<Weapon>();
    public List<Weapon> _spellScrolls = new List<Weapon>();

    public List<(CombatEntity.BuffTypes, int, float)> Buffs = new List<(CombatEntity.BuffTypes, int, float)>();
    public List<(CombatEntity.DeBuffTypes, int, float)> DeBuffs = new List<(CombatEntity.DeBuffTypes, int, float)>();

    public bool showHelm = true;


    Dictionary<Equipment.Stats, int> _stats;

    public EquipmentCreator EC;

    [SerializeField] public CombatEntity _combatEntity;

    public static event Action<Character,int, int> UpdateBlock; 
    public static event Action<Character> UsePrep; 

    public static event Action<Character,int, int, int> UpdateEnergy; 
    public static event Action<Character> UpdateStatsEvent;
    public static event Action<ErrorMessageManager.Errors> Notification;
    public static event Action<ErrorMessageManager.Errors, int> NotificationGold;


    public EquipmentModelManager EqMM;

    public void ToggleShowHelm()
    {
        showHelm = !showHelm;
        EqMM.UpdateHead();
    }

    private bool StartRun = false;

    public void MakeSureStartRuns()
    {
        Start();
    }

    protected virtual void Start()
    {
        if(StartRun)
            return;
        EC = FindObjectOfType<EquipmentCreator>();
        CombatController.ActivateCombatEntities += ActivateCombatEntity;
        CombatTrigger.EndCombat += DeactivateCombatEntity;
        
        CombatEntity.GetHitWithAttack += GetHitWithAttack;
        CombatEntity.GetHitWithBuff += GetHitWithBuff;
        CombatEntity.GetHitWithDeBuff += GetHitWithDeBuff;
        
        CombatEntity.GetHealed += GetHealed;

        EqMM = GetComponentInChildren<EquipmentModelManager>();
        
        
        //_weapons = EC.CreateAllWeapons(10);
        //_spellScrolls = EC.CreateAllSpellScrolls(10);
        //_spellScrolls = EC.CreateAllSpellScrolls(10);
        

        if (isPlayerCharacter)
        {
            //_weapons = EC.CreateAllWeapons(_level);
            //_spellScrolls = EC.CreateAllSpellScrolls(_level);
            // _weapons.Add(EC.CreateWeapon(_level,1,Equipment.Slot.OneHander, Weapon.SpellTypes.Dagger3));
            // _weapons.Add(EC.CreateWeapon(_level,0,Equipment.Slot.OneHander, Weapon.SpellTypes.Sword3));
            // _spellScrolls.Add(EC.CreateSpellScroll(_level,1,Weapon.SpellTypes.Blood1));
            // _spellScrolls.Add(EC.CreateSpellScroll(_level,1,Weapon.SpellTypes.Blood2));
        }
        else
        {
            if (isDragon)
            {
                //gameObject.GetComponent<Dragon>().InitializeDragon();
            }
            else if( isElite)
            {
                //elite
            }
            else
            {
                EqMM.RandomCharacter();
                //_weapons = EC.CreateAllWeapons(_level);
                //_spellScrolls = EC.CreateAllSpellScrolls(_level);
                _equipment = EC.CreateAllEquipment(_level);

           
                _spellScrolls.Add(EC.CreateRandomSpellScroll(_level));
                _spellScrolls.Add(EC.CreateRandomSpellScroll(_level));
                //_spellScrolls.Add(EC.CreateRandomSpellScroll(_level));

                if (_level <= 5)
                {
                    _weapons.Add(EC.CreateWeapon(_level,Mathf.FloorToInt(_level/5f),Equipment.Slot.OneHander, Weapon.SpellTypes.Shield2));
                }
                else
                {
                    _weapons.Add(EC.CreateRandomWeapon(_level, false));

                }

                if (!HasDamageSpell(_spellScrolls) && !HasDamageSpell(_weapons))
                {
                    // if we have no damage abilitys yet, give em one
                    _weapons.Add(EC.CreateRandomWeaponWithSpell(_level, (Weapon.SpellTypes)GetRandomDamageSpell()));
                }
                else
                {
                    _weapons.Add(EC.CreateRandomWeapon(_level, false));
                }
            
                EqMM.UpdateWeapon(_weapons[0], _weapons[1]);
                //_weapons.Add(EC.CreateWeapon(_level,1,Equipment.Slot.OneHander, Weapon.SpellTypes.Hammer3));
                //_weapons.Add(EC.CreateWeapon(_level,1,Equipment.Slot.OneHander, Weapon.SpellTypes.Fire2));
                //_spellScrolls.Add(EC.CreateSpellScroll(_level,1,Weapon.SpellTypes.Blood1));
                //_spellScrolls.Add(EC.CreateSpellScroll(_level,1,Weapon.SpellTypes.Nature4));

                _gold = _level * 2 + (Random.Range(-_level, _level+1));
            }
            

        }

        if (!isDragon && !isElite)
        {
            foreach (var eq in _equipment)
            {
                EqMM.UpdateSlot(eq, showHelm);
            }
        
        
            _equipment.AddRange(_weapons);
            _equipment.AddRange(_spellScrolls);
        }
        
        UpdateStats();
        _currentHealth = _maxHealth;
        StartRun = true;

    }
    public List<(Weapon.SpellTypes, Weapon)> GetWeaponSpells()
    {
        //spell 1, spell2, weapon1, weapon2
        List<(Weapon.SpellTypes, Weapon)> Spells = new List<(Weapon.SpellTypes, Weapon)>();

        foreach (var weapon in _weapons)
        {
            Spells.Add((weapon.GetSpellTypes().Item1, weapon));
        }
        
        // switch (_weapons.Count)
        // {
        //     case 0:
        //         break;
        //     case 1:
        //         if (_weapons[0].slot == Equipment.Slot.TwoHander)
        //         {
        //             spells.Item1 = _weapons[0].GetSpellTypes().Item1;
        //             spells.Item2 = _weapons[0].GetSpellTypes().Item2;
        //             spells.Item3 = _weapons[0];
        //             spells.Item4 = _weapons[0];
        //         }
        //         else
        //         {
        //             spells.Item1 = _weapons[0].GetSpellTypes().Item1;
        //             spells.Item3 = _weapons[0];
        //         }
        //         break;
        //     case 2:
        //         spells.Item1 = _weapons[0].GetSpellTypes().Item1;
        //         spells.Item3 = _weapons[0];
        //         spells.Item2 = _weapons[1].GetSpellTypes().Item1;
        //         spells.Item4 = _weapons[1];
        //
        //         break;
        //         
        //         
        // }

        
        return Spells;
    }

    public List<(Weapon.SpellTypes, Weapon)> GetSpells()
    {
        List<(Weapon.SpellTypes, Weapon)> Spells = new List<(Weapon.SpellTypes, Weapon)>();
        foreach (var scroll in _spellScrolls)
        {
            Spells.Add((scroll.GetSpellTypes().Item1, scroll));
        }

        return Spells;
    }

    public (Weapon.SpellTypes, Weapon.SpellTypes, Weapon, Weapon) GetScollSpells()
    {
        (Weapon.SpellTypes, Weapon.SpellTypes, Weapon, Weapon) spells = (Weapon.SpellTypes.None, Weapon.SpellTypes.None, null, null);
        switch (_spellScrolls.Count)
        {
            case 0:
                break;
            case 1:
                spells.Item1 = _spellScrolls[0].GetSpellTypes().Item1;
                spells.Item3 = _spellScrolls[0];
                break;
            case 2:
                spells.Item1 = _spellScrolls[0].GetSpellTypes().Item1;
                spells.Item3 = _spellScrolls[0];
                spells.Item2 = _spellScrolls[1].GetSpellTypes().Item1;
                spells.Item4 = _spellScrolls[1];

                break;
                
                
        }

        return spells;
    }

    public Dictionary<Equipment.Stats, int> GetStats()
    {
        return _stats;
    }

    public void UpdateEnergyCount(int amount)
    {
        
        _currentEnergy += amount;

        if (_currentEnergy < 0)
            _currentEnergy = 0;

        UpdateEnergy(this, _currentEnergy, _maxEnergy, amount);
    }

    public void UsePrepStack()
    {
        //Debug.Log("use prep");
        UsePrep(this);
    }
    


    private void OnDestroy()
    {
        CombatController.ActivateCombatEntities -= ActivateCombatEntity;
        CombatTrigger.EndCombat -= DeactivateCombatEntity;
        CombatEntity.GetHitWithAttack -= GetHitWithAttack;
        CombatEntity.GetHealed -= GetHealed;
        
        CombatEntity.GetHitWithBuff -= GetHitWithBuff;
        CombatEntity.GetHitWithDeBuff -= GetHitWithDeBuff;
    }

    private void GetHitWithBuff(Character c, CombatEntity.BuffTypes buff, int turns, float amount)
    {
        if(c != this)
            return;
        
        Debug.Log("HIT WITH BUFF + " + c.name);


        int i = GetIndexOfBuff(buff);
        switch (buff)
        {
            case CombatEntity.BuffTypes.Block:
                if (i == -1)
                {
                    Buffs.Add((buff,turns,amount));
                    UpdateBlock(this, Mathf.RoundToInt(amount), Mathf.RoundToInt(amount));

                }
                else
                {
                    Buffs[i] = (buff,turns, amount + Buffs[i].Item3);
                    UpdateBlock(this, Mathf.RoundToInt(Buffs[i].Item3), Mathf.RoundToInt(amount));
                
                    if (Buffs[i].Item3 <=0)
                    {
                        Buffs.RemoveAt(i);
                    }
                

                }
                break;
            case CombatEntity.BuffTypes.Rejuvenate:
                if (i == -1)
                {
                    Buffs.Add((buff,turns,amount));

                }
                else
                {
                    float tempAmount = Buffs[i].Item3;
                    if (amount > tempAmount)
                        tempAmount = amount;
                    Buffs[i] = (buff, Buffs[i].Item2 + turns, tempAmount);
                    
                    if (Buffs[i].Item3 <=0)
                    {
                        Buffs.RemoveAt(i);
                    }
                }
                break;
            case CombatEntity.BuffTypes.Thorns:
                if (i == -1)
                {
                    Buffs.Add((buff,turns,amount));

                }
                else
                {
                    Buffs[i] = (buff, Buffs[i].Item2 + turns, amount + Buffs[i].Item3);
                
                    if (Buffs[i].Item3 <=0)
                    {
                        Buffs.RemoveAt(i);
                    }
                }
                break;
            case CombatEntity.BuffTypes.Immortal:
                if (i == -1)
                {
                    Buffs.Add((buff,turns,amount));
                }
                else
                {
                    Buffs[i] = (buff,turns + Buffs[i].Item2, amount);
                }
                break;
            case CombatEntity.BuffTypes.Invulnerable:
                if (i == -1)
                {
                    Buffs.Add((buff,turns,amount));
                }
                else
                {
                    Buffs[i] = (buff,turns + Buffs[i].Item2, amount);
                }
                break;
            case CombatEntity.BuffTypes.Prepared:
                if (i == -1)
                {
                    Buffs.Add((buff,turns,amount));
                }
                else
                {
                    Buffs[i] = (buff,turns + Buffs[i].Item2, amount);
                }
                break;
            case CombatEntity.BuffTypes.Shatter:
                UpdateStatsEvent(this);
                if (i == -1)
                {
                    Buffs.Add((buff,turns,amount));
                }
                else
                {
                    float a = amount + Buffs[i].Item3;
                    if (a > 30)
                    {
                        a = 30;
                    }
                    //Debug.Log(amount + Buffs[i].Item3);
                    Buffs[i] = (buff, Buffs[i].Item2 + 1,  a);
                }
                break;
            case CombatEntity.BuffTypes.Empowered:
                if (i != -1)
                {
                    float a = amount + Buffs[i].Item3;
                    if (a > 75)
                    {
                        a = 75;
                    }
                    Buffs[i] = (buff, Buffs[i].Item2 + 1,  a);
                    
                }
                else
                {
                    Buffs.Add((buff,turns,amount));

                }
                break;
        }
    }
    public int GetIndexOfBuff(CombatEntity.BuffTypes buff)
    {
        int i = -1;
        for (int j = 0; j < Buffs.Count; j++)
        {
            if (Buffs[j].Item1 == buff)
            {
                i = j;
                break;
            }
        }

        return i;
    }
    public int GetIndexOfDebuff(CombatEntity.DeBuffTypes debuff)
    {
        int i = -1;
        for (int j = 0; j < DeBuffs.Count; j++)
        {
            if (DeBuffs[j].Item1 == debuff)
            {
                i = j;
                break;
            }
        }

        return i;
    }
    
    private void GetHitWithDeBuff(Character c, CombatEntity.DeBuffTypes deBuff, int turns, float amount)
    {
        if(c != this)
            return;

        Debug.Log("HIT WITH DEBUFF + " + c.name);
        int i = GetIndexOfDebuff(deBuff);
        switch (deBuff)
        {
            case CombatEntity.DeBuffTypes.Bleed:
                if (i == -1)
                {
                    DeBuffs.Add((deBuff,turns,amount));

                }
                else
                {
                    DeBuffs[i] = (deBuff, DeBuffs[i].Item2 + turns, amount + DeBuffs[i].Item3);
                
                    if (DeBuffs[i].Item3 <=0)
                    {
                        DeBuffs.RemoveAt(i);
                    }
                }
                break;
            case CombatEntity.DeBuffTypes.Burn:
                if (i == -1)
                {
                    DeBuffs.Add((deBuff,turns,amount));
                }
                else
                {
                    DeBuffs[i] = (deBuff, DeBuffs[i].Item2 + turns, amount + DeBuffs[i].Item3);
                
                    if (DeBuffs[i].Item3 <=0)
                    {
                        DeBuffs.RemoveAt(i);
                    }
                }
                break;
            case CombatEntity.DeBuffTypes.Chilled:
                //check if we already have it
                if (i == -1)
                {
                    DeBuffs.Add((deBuff,turns,amount));

                }
                else
                {
                    DeBuffs[i] = (deBuff, DeBuffs[i].Item2 + turns, amount);
                    
                    if (DeBuffs[i].Item3 <=0)
                    {
                        DeBuffs.RemoveAt(i);
                    }

                }
                break;
            case CombatEntity.DeBuffTypes.Weakened:
                //check if we already have it
                if (i != -1)
                {
                    float reduct = amount + DeBuffs[i].Item3;
                    if (reduct > 75)
                    {
                        reduct = 75;
                    }
                    
                    DeBuffs[i] = (deBuff, DeBuffs[i].Item2 + 1, reduct);
                }
                else
                {
                    DeBuffs.Add((deBuff,turns,amount));

                }
                break;
            case CombatEntity.DeBuffTypes.Exposed:
                //check if we already have it
                if (i != -1)
                {
                    float reduct = DeBuffs[i].Item3 + 10;
                    if (reduct > 50)
                    {
                        reduct = 50;
                    }
                    
                    DeBuffs[i] = (deBuff, DeBuffs[i].Item2 + 1, reduct);
                }
                else
                {
                    DeBuffs.Add((deBuff,turns,10));
                }
                break;
            case CombatEntity.DeBuffTypes.Wounded:
                //check if we already have it
                if (i != -1)
                {
                    DeBuffs[i] = (deBuff, DeBuffs[i].Item2 + 1, amount);
                }
                else
                {
                    DeBuffs.Add((deBuff,turns,amount));

                }
                break;
        }
    }

    private void GetHealed(Character c, int healAmount)
    { 
        if(c != this)
            return;

        _currentHealth += healAmount;
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    public void GetGold(int amount)
    {
        if (RelicManager._instance.CheckRelic(RelicType.DragonRelic11))
        {
            return;
        }
        _gold += amount;
        NotificationGold(ErrorMessageManager.Errors.GetGold, amount);
        UpdateStats();
    }

    private void GetHitWithAttack(Character c, CombatEntity.AbilityTypes abilityTypes, int amount, int reduction = 0)
    {
        if(c != this)
            return;
        
        // take damage
        // possibly die
        if (amount > 0)
        {
            _am.SetTrigger(TheSpellBook.AnimationTriggerNames.SmallHit.ToString());
            
        }

        _currentHealth -= amount;

        if (_currentHealth < _maxHealth / 2f && !RelicManager._instance.UsedRelic8)
        {
            if (RelicManager._instance.CheckRelic(RelicType.Relic8))
            {
                _combatEntity.Buff(_combatEntity, CombatEntity.BuffTypes.Block, 1, Mathf.RoundToInt(_maxHealth/4f));
                RelicManager._instance.UsedRelic8 = true;
            }
        }

        if (_currentHealth <= 0 && isPlayerCharacter)
        {
            if (!RelicManager._instance.UsedRelic23 && RelicManager._instance.CheckRelic(RelicType.Relic23))
            {
                _currentHealth = 0;
                _combatEntity.Heal(_combatEntity,Mathf.RoundToInt(c._maxHealth/2f), 0);
                _combatEntity.Buff(_combatEntity,CombatEntity.BuffTypes.Invulnerable, 1, 1);
                RelicManager._instance.UsedRelic23 = true;
            }
            
        }
        
        if (_currentHealth <= 0)
        {
            // die
            _am.SetTrigger(TheSpellBook.AnimationTriggerNames.Die.ToString());
            _currentHealth = 0;
            if (!isPlayerCharacter)
            {
                CombatController._instance.entitiesInCombat.Remove(this.GetComponent<CombatEntity>());

                if (CombatController._instance.entitiesInCombat.Count == 1)
                {
                    ToolTipManager._instance.HideToolTipAll();
                    if (CombatController._instance.Player._level == 30)
                    {
                        // victory
                        UIController._instance.ActivateVictoryScreen();
                        UIController._instance.ToggleInventoryUI(0);

                    }
                    else
                    {
                        UIController._instance.ToggleInventoryUI(1);
                        SelectionManager._instance.RandomSelectionFromEquipment(this);
                        //NotificationGold(ErrorMessageManager.Errors.GetGold, _gold);
                        Notification(ErrorMessageManager.Errors.Victory);
                    }
                    //CombatController._instance.Player._level += 1;
                    CombatController._instance.Player._currentHealth += 50;
                    CombatController._instance.Player._currentEnergy = 0;

                    
                    
                    CombatController._instance.Player._currentHealth = CombatController._instance.Player._maxHealth;
                    CombatController._instance.Player.Buffs = new List<(CombatEntity.BuffTypes, int, float)>();
                    CombatController._instance.Player.DeBuffs = new List<(CombatEntity.DeBuffTypes, int, float)>();
                    
                    CombatController._instance.Player.UpdateStats();

                    if (isDragon)
                    {
                        MapManager._instance.GenerateNewMap();
                    }
                    
                    Destroy(_combatEntity);

                    //CombatController._instance.Player._gold += _gold;

                    CombatController._instance.EndCombat();
                    
                    
                    StartCoroutine(WaitThenDestroy());
                }
                
            }
            else
            {
                //GameOver
                CombatController._instance.entitiesInCombat[1].myCharacter._am.SetTrigger("Victory");
                Notification(ErrorMessageManager.Errors.YouHaveDied);
                UIController._instance.ToggleInventoryUI(0);
                _am.SetTrigger("die");
                UIController._instance.RestartButton.SetActive(true);
                //CombatController._instance.
                CombatController._instance.EndCombat();

                //UI controller place restart button on screen

                

                
            }
            
        }
    }

    private IEnumerator WaitThenDestroy()
    {
        yield return new WaitForSeconds(3);
        
        Destroy(this.gameObject);

        
    }

    private void ActivateCombatEntity(Character player, Character enemy)
    {
        //Debug.Log("its mee thats calling it");
        if (player == this)
        {
            _combatEntity.enabled = true;
            _combatEntity.Target = enemy._combatEntity;
            UpdateEnergyCount(_maxEnergy);
            

        }
        else if (enemy == this)
        {
            _combatEntity.enabled = true;
            _combatEntity.Target = player._combatEntity;
            UpdateEnergyCount(_maxEnergy);

            
            //activate friends and set their target to the player
        }

    }
    private void DeactivateCombatEntity()
    {
        if (_combatEntity.enabled == true)
        {
            _combatEntity.enabled = false;
        }
        
    }


    public void UpdateStats()
    {
        _stats = new Dictionary<Equipment.Stats, int>();

        //base stats
        for (int i = 2; i < 18; i++)
        {
            //Debug.Log((Equipment.Stats)i);
            _stats.Add((Equipment.Stats)i, 0);
        }
        
        

        foreach (Equipment e in _equipment)
        {
            foreach (var stat in e.stats)
            {
                if (_stats.ContainsKey(stat.Key))
                {
                    _stats[stat.Key] += stat.Value;
                }
                else
                {
                    _stats.Add(stat.Key, stat.Value);
                }
            }
        }
        
        //PrintEquip();
        
        // max health = 50 * level + 50 + hp from stats
        SetMaxHealth();
        //_currentEnergy = 0;
        //UpdateEnergyCount(_currentEnergy);
        
        PrettyPrintStats();
        UpdateStatsEvent(this);
    }

    private void PrintEquip()
    {
        string OutS = "";

        OutS += "------------eq------\n";

        foreach (var VARIABLE in _equipment)
        {
            OutS += VARIABLE.name + "\n";
        }

        OutS += "------------wep------\n";
        foreach (var VARIABLE in _weapons)
        {
            OutS += VARIABLE.name + "\n";
        }
        OutS += "------------scrol------\n";

        foreach (var VARIABLE in _spellScrolls)
        {
            OutS += VARIABLE.name + "\n";
        }
        Debug.Log(OutS);
        
        
    }

    private void SetMaxHealth()
    {
        int hp = 0;
        if (isDragon)
        {
            hp = 150 * _level + 150;
        }
        else if (isElite)
        {
            hp = 75 * _level + 75;
        }
        else
        {
            hp = 50 * _level + 50;
        }
        int hpFromStats = 0;
        _stats.TryGetValue(Equipment.Stats.Health, out hpFromStats);
        hp += hpFromStats;
        _maxHealth = hp;

        _stats[Equipment.Stats.Health] += hp;

        _currentHealth = _maxHealth;


    }
    
    private void PrettyPrintStats()
    {
        string Output = "";
        Output += name + "\n";
        Output += GetWeaponSpellsNames();
        //Output += "Level: " + level + "\n";
        foreach (KeyValuePair<Equipment.Stats,int> kvp in _stats)
        {
            Output += kvp.Key.ToString() + ": " + kvp.Value + "\n";

        }
        
        //Debug.Log(Output);
    }
    
    
    public string GetWeaponSpellsNames()
    {

        string printString = "Spells:\n";

        switch (_weapons.Count)
        {
            case 0:
                break;
            case 1:
                if (_weapons[0].slot == Equipment.Slot.TwoHander)
                {
                    printString += _weapons[0].GetSpellTypes().Item1 +"\n";
                    printString += _weapons[0].GetSpellTypes().Item2 +"\n";

                    
                }
                else
                {
                    printString += _weapons[0].GetSpellTypes().Item1+"\n";
                }
                break;
            case 2:
                printString += _weapons[0].GetSpellTypes().Item1 +"\n";
                printString += _weapons[1].GetSpellTypes().Item1+"\n";
                break;
                
                
        }

        return printString;
    }
    
    
    bool HasDamageSpell(List<Weapon> equipments)
    {
        bool hasDamage = false;
        foreach (var eq in equipments)
        {
            Weapon wep = (Weapon)eq;
            // get the spell
            int spellIndex = (int)wep.spellType1;
            List<List<object>> scaling = DataReader._instance.GetWeaponScalingTable();
            IList abilities = (IList)scaling[(int)spellIndex][4];

            if (abilities.Contains(0) || abilities.Contains(1))
            {
                hasDamage = true;
            }
        }

        return hasDamage;
    }
    
    int GetRandomDamageSpell()
    {
        int[] damageSpells = new[] { 0,1,2,3,6,7,8,9,10,11,12,13,14, 17, 18, 21, 22, 23, 27, 28, 30, 31, 37  };
        return damageSpells[Random.Range(0, damageSpells.Length)];
    }
    
    
    

    
}
