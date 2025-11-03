using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using ImportantStuff;
using Map;
using PlayFab.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;
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
    [SerializeField] private bool isBlacksmith;

    public int inventorySize = 6;
    public List<Equipment> _equipment = new List<Equipment>();
    public Equipment[] equipmentArray;
    public List<Equipment> _inventory = new List<Equipment>();
    public List<Equipment> _Relics = new List<Equipment>();


    public List<Weapon> _weapons = new List<Weapon>();
    public List<Weapon> _spellScrolls = new List<Weapon>();

    public List<(CombatEntity.BuffTypes, int, float)> Buffs = new List<(CombatEntity.BuffTypes, int, float)>();
    public List<(CombatEntity.DeBuffTypes, int, float)> DeBuffs = new List<(CombatEntity.DeBuffTypes, int, float)>();
    public List<(CombatEntity.BlessingTypes, int, float)> Blessings = new List<(CombatEntity.BlessingTypes, int, float)>();

    public bool showHelm = true;


    Dictionary<Stats, int> _stats;


    [SerializeField] public CombatEntity _combatEntity;

    public static event Action<Character,int, int> UpdateBlock; 
    public static event Action<Character> UsePrep; 

    public static event Action<Character,int, int, int> UpdateEnergy; 
    public static event Action<Character> UpdateStatsEvent;
    public static event Action<ErrorMessageManager.Errors> Notification;
    public static event Action<ErrorMessageManager.Errors, int> NotificationGold;


    public EquipmentModelManager EqMM;
    
    [SerializeField] private AudioClip getGold;
    [SerializeField] private float getGoldVol;
    [SerializeField] private float getGoldpitch;
    [SerializeField] private ButtonGlow EnergyGlow;

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
        CombatController.ActivateCombatEntities += ActivateCombatEntity;
        CombatTrigger.EndCombat += DeactivateCombatEntity;
        
        CombatEntity.GetHitWithAttack += GetHitWithAttack;
        CombatEntity.GetHitWithBuff += GetHitWithBuff;
        CombatEntity.GetHitWithDeBuff += GetHitWithDeBuff;
        CombatEntity.GetHitWithBlessing += GetHitWithBlessing;

        
        CombatEntity.GetHealed += GetHealed;

        EqMM = GetComponentInChildren<EquipmentModelManager>();
        
        
        //_weapons = EC.CreateAllWeapons(10);
        //_spellScrolls = EC.CreateAllSpellScrolls(10);
        //_spellScrolls = EC.CreateAllSpellScrolls(10);
        

        if (isPlayerCharacter)
        {
            //_weapons = EC.CreateAllWeapons(_level);
            //_spellScrolls = EC.CreateAllSpellScrolls(_level);
            // _weapons.Add(EC.CreateWeapon(_level,1,Equipment.Slot.OneHander, SpellTypes.Dagger3));
            // _weapons.Add(EC.CreateWeapon(_level,0,Equipment.Slot.OneHander, SpellTypes.Sword3));
            // _spellScrolls.Add(EC.CreateSpellScroll(_level,1,SpellTypes.Blood1));
            // _spellScrolls.Add(EC.CreateSpellScroll(_level,1,SpellTypes.Blood2));
        }
        else
        {
            if (isDragon)
            {
                //gameObject.GetComponent<Dragon>().InitializeDragon();

                if (CombatController._instance.Difficulty >= 10)
                {
                    _maxEnergy = 4 + CombatController._instance.TrialCounter;
                }
            }
            else if( isElite)
            {
                //elite
                if (CombatController._instance.Difficulty >= 10)
                {
                    _maxEnergy = 3 + CombatController._instance.TrialCounter;
                }
            }
            else
            {
                Random.InitState(CombatController._instance.LastNodeClicked.nodeSeed);
                EquipmentCreator._instance.GetCharacterAbilities();
                EqMM.RandomCharacter();
                //_weapons = EC.CreateAllWeapons(_level);
                //_spellScrolls = EC.CreateAllSpellScrolls(_level);
                _equipment = EquipmentCreator._instance.CreateAllEquipment(_level);

           
                _spellScrolls.Add(EquipmentCreator._instance.CreateRandomSpellScroll(_level));
                _spellScrolls.Add(EquipmentCreator._instance.CreateRandomSpellScroll(_level));
                //_spellScrolls.Add(EC.CreateRandomSpellScroll(_level));

                if (_level <= 5)
                {
                    if(!Modifiers._instance.CurrentMods.Contains(Mods.NoShieldSpells))
                        _weapons.Add(EquipmentCreator._instance.CreateWeapon(_level,Mathf.FloorToInt(_level/5f),Equipment.Slot.OneHander, SpellTypes.Shield2));
                    else
                        _weapons.Add(EquipmentCreator._instance.CreateRandomWeapon(_level, false));
                }
                else
                {
                    _weapons.Add(EquipmentCreator._instance.CreateRandomWeapon(_level, false));

                }

                if (!HasDamageSpell(_spellScrolls) && !HasDamageSpell(_weapons))
                {
                    // if we have no damage abilitys yet, give em one
                    _weapons.Add(EquipmentCreator._instance.CreateRandomWeaponWithSpell(_level, (SpellTypes)GetRandomDamageSpell()));
                }
                else
                {
                    _weapons.Add(EquipmentCreator._instance.CreateRandomWeapon(_level, false));
                }
            
                EqMM.UpdateWeapon(_weapons[0], _weapons[1]);
                //_weapons.Add(EC.CreateWeapon(_level,1,Equipment.Slot.OneHander, SpellTypes.Hammer3));
                //_weapons.Add(EC.CreateWeapon(_level,1,Equipment.Slot.OneHander, SpellTypes.Fire2));
                //_spellScrolls.Add(EC.CreateSpellScroll(_level,1,SpellTypes.Blood1));
                //_spellScrolls.Add(EC.CreateSpellScroll(_level,1,SpellTypes.Nature4));

                _gold = _level * 2 + (Random.Range(-_level, _level+1));

                if (CombatController._instance.Difficulty >= 3)
                {
                    _gold = Mathf.CeilToInt(_gold * .25f);
                }

                if (Modifiers._instance.CurrentMods.Contains(Mods.DoubleGold))
                    _gold *= 2;
                if (Modifiers._instance.CurrentMods.Contains(Mods.HalfGold))
                    _gold = Mathf.RoundToInt(_gold * .5f);
                
                
            }
            

        }

        if (!isDragon && !isElite)
        {
            foreach (var eq in _equipment)
            {
                EqMM.UpdateSlot(eq);
            }
        
        
            _equipment.AddRange(_weapons);
            _equipment.AddRange(_spellScrolls);
            EqMM.FixHead();
        }
        
        UpdateStats();
        _currentHealth = _maxHealth;
        StartRun = true;

    }
    public List<(SpellTypes, Weapon)> GetWeaponSpells()
    {
        //spell 1, spell2, weapon1, weapon2
        List<(SpellTypes, Weapon)> Spells = new List<(SpellTypes, Weapon)>();

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

        for (int i = 0; i < 2; i++)
        {
            if (Spells.Count < 2)
            {
                Spells.Add((SpellTypes.None,null));
            }
        }

        
        return Spells;
    }

    public List<(SpellTypes, Weapon)> GetSpells()
    {
        List<(SpellTypes, Weapon)> Spells = new List<(SpellTypes, Weapon)>();
        foreach (var scroll in _spellScrolls)
        {
            Spells.Add((scroll.GetSpellTypes().Item1, scroll));
        }
        
        for (int i = 0; i < 2; i++)
        {
            if (Spells.Count < 2)
            {
                Spells.Add((SpellTypes.None,null));
            }
        }

        return Spells;
    }

    public (SpellTypes, SpellTypes, Weapon, Weapon) GetScollSpells()
    {
        (SpellTypes, SpellTypes, Weapon, Weapon) spells = (SpellTypes.None, SpellTypes.None, null, null);
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

    public Dictionary<Stats, int> GetStats()
    {
        return _stats;
    }

    public void UpdateEnergyCount(int amount)
    {
        _currentEnergy += amount;

        if (_currentEnergy < 0)
            _currentEnergy = 0;

        UpdateEnergy(this, _currentEnergy, _maxEnergy, amount);

        if (amount > 0 && isPlayerCharacter && EnergyGlow != null)
        {
            EnergyGlow.TriggerEffect(new Color(1,.9f,.2f));
            UIController._instance.PlayEnergySound();
        }
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
        CombatEntity.GetHitWithBlessing -= GetHitWithBlessing;

        CombatEntity.GetHealed -= GetHealed;
        
        CombatEntity.GetHitWithBuff -= GetHitWithBuff;
        CombatEntity.GetHitWithDeBuff -= GetHitWithDeBuff;
    }

    private void GetHitWithBuff(Character c, CombatEntity.BuffTypes buff, int turns, float amount)
    {
        if(c != this)
            return;
        
        if(c._currentHealth <= 0)
        {
            Debug.LogError("THIS BROKE SOMETHING MAYBE?");
            return;
        }
        
        //Debug.Log("HIT WITH BUFF + " + c.name);


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
    
    public int GetIndexOfBlessing(CombatEntity.BlessingTypes blessing)
    {
        int i = -1;
        for (int j = 0; j < Blessings.Count; j++)
        {
            if (Blessings[j].Item1 == blessing)
            {
                i = j;
                break;
            }
        }

        return i;
    }
    private void GetHitWithBlessing(Character c, CombatEntity.BlessingTypes blessing, int turns, float amount)
    {
        if(c != this)
            return;

        //Debug.Log("HIT WITH Blessing + " + blessing +" " + c.name);
        int i = GetIndexOfBlessing(blessing);
        switch (blessing)
        {
            default:
                if (i == -1)
                {
                    Blessings.Add((blessing,turns,amount));
                }
                else
                {
                    Blessings[i] = (blessing, Blessings[i].Item2 + turns, amount + Blessings[i].Item3);
                
                    if (Blessings[i].Item3 <=0)
                    {
                        Blessings.RemoveAt(i);
                    }
                }
                break;

        }
        // update the stats
        /*if (blessing == CombatEntity.BlessingTypes.Health)
            Debug.Log("Health with Blessing: " + blessing+" @ "+ amount);*/

        UpdateStats();

    }
    
    private void GetHitWithDeBuff(Character c, CombatEntity.DeBuffTypes deBuff, int turns, float amount)
    {
        if(c != this)
            return;

        //Debug.Log("HIT WITH DEBUFF + " + c.name);
        int i = GetIndexOfDebuff(deBuff);
        switch (deBuff)
        {
            case CombatEntity.DeBuffTypes.Lacerate:
                if (i == -1)
                {
                    DeBuffs.Add((deBuff,turns,amount));

                }
                else
                {
                    float tempAmount = DeBuffs[i].Item3;
                    if (amount > tempAmount)
                        tempAmount = amount;
                    DeBuffs[i] = (deBuff, DeBuffs[i].Item2 + turns, tempAmount);
                
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
                    float tempAmount = DeBuffs[i].Item3;
                    if (amount > tempAmount)
                        tempAmount = amount;
                    DeBuffs[i] = (deBuff, DeBuffs[i].Item2 + turns, tempAmount);
                
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
                    float reduct = DeBuffs[i].Item3 + amount;
                    if (reduct > 50)
                    {
                        reduct = 50;
                    }
                    
                    DeBuffs[i] = (deBuff, DeBuffs[i].Item2 + 1, reduct);
                }
                else
                {
                    DeBuffs.Add((deBuff,turns,amount));
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

    private void GetHealed(Character c, int healAmount, bool isCrit)
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
        if (amount > 0 && RelicManager._instance.CheckRelic(RelicType.DragonRelic11))
        {
            return;
        }
        _gold += amount;
        if (amount > 0)
        {
            NotificationGold(ErrorMessageManager.Errors.GetGold, amount);
        }
        else
        {
            NotificationGold(ErrorMessageManager.Errors.LoseGold, amount);

        }
        UpdateStats();
        
        SoundManager.Instance.Play2DSFX(getGold, getGoldVol, getGoldpitch);

    }

    private void GetHitWithAttack(Character c, CombatEntity.AbilityTypes abilityTypes, (int,int) amountAndReduction, bool isCrit = false)
    {
        if(c != this)
            return;
        
        // take damage
        // possibly die
        if (amountAndReduction.Item1 > 0)
        {
            _am.SetTrigger(TheSpellBook.AnimationTriggerNames.SmallHit.ToString());
            
        }

        _currentHealth -= amountAndReduction.Item1;

        if(_currentHealth < _maxHealth / 2f && isPlayerCharacter)
        {
            if (!RelicManager._instance.UsedRelic8)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic8))
                {
                    _combatEntity.Buff(_combatEntity, CombatEntity.BuffTypes.Block, 1,
                        Mathf.RoundToInt(_maxHealth / 4f));
                    RelicManager._instance.UsedRelic8 = true;
                }
            }
            if (!RelicManager._instance.UsedRelic7)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic7))
                {
                    _combatEntity.GetHitWithBlessingDirect(CombatEntity.BlessingTypes.SpellPower, 1, _level * 5);
                    _combatEntity.GetHitWithBlessingDirect(CombatEntity.BlessingTypes.Strength, 1, _level * 5);
                    RelicManager._instance.UsedRelic7 = true;
                }
            }
            if (!RelicManager._instance.UsedRelic6)
            {
                if (RelicManager._instance.CheckRelic(RelicType.Relic6))
                {
                    _combatEntity.GetHitWithBlessingDirect(CombatEntity.BlessingTypes.MagicResist, 1, _level * 10);
                    _combatEntity.GetHitWithBlessingDirect(CombatEntity.BlessingTypes.Armor, 1, _level * 10);
                    RelicManager._instance.UsedRelic6 = true;
                }
            }
        }

        if (_currentHealth <= 0 && isPlayerCharacter)
        {
            if (!RelicManager._instance.UsedRelic22 && RelicManager._instance.CheckRelic(RelicType.Relic22))
            {
                _currentHealth = 1;
                //Debug.Log(Mathf.RoundToInt(c._maxHealth/2f));
                
                //Debug.Log("phenoix blessing " + " current max = "  + _maxHealth + " current health = " + _currentHealth);
                
                
                _combatEntity.Buff(_combatEntity,CombatEntity.BuffTypes.Invulnerable, 1, 1);
                _combatEntity.Heal(_combatEntity,Mathf.RoundToInt(c._maxHealth/2f), 0);
                RelicManager._instance.UsedRelic22 = true;
                
                //Debug.Log("phenoix blessing after " + " current max = "  + _maxHealth + " current health = " + _currentHealth);

                //Debug.Log(_currentHealth);
            }
            
        }
        
        if (_currentHealth <= 0)
        {
            // die
            
            _currentHealth = 0;
            if (!isPlayerCharacter)
            {
                CombatController._instance.entitiesInCombat.Remove(this.GetComponent<CombatEntity>());

                if (CombatController._instance.entitiesInCombat.Count == 1)
                {
                    ToolTipManager._instance.HideToolTipAll();
                    if (CombatController._instance.Player._level == 10)
                    {
                        //todo This is where we start a new combat with the final boss?
                        // victory
                        UIController._instance.ActivateVictoryScreen();
                        UIController._instance.ToggleInventoryUI(0); 
                        PlayFabManager._instance.SubmitRunData(true);

                    }
                    else
                    {
                        // UIController._instance.ToggleInventoryUI(1);
                        // SelectionManager._instance.RandomSelectionFromEquipment(this);
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
                        CombatController._instance.IncreaseTrialCounter();
                        MapManager._instance.GenerateNewMap();

                    }
                    

                    //CombatController._instance.Player._gold += _gold;
                    UIController._instance.PlayVictorySound();
                    MusicManager.Instance.PlayAdventureMusic();

                    if(isDragon)
                    {
                        _am.SetTrigger(TheSpellBook.AnimationTriggerNames.Die.ToString());
                    }
                    // else
                    //{
                    if(!isBlacksmith)
                    {
                        TutorialManager.Instance.QueueTip(TutorialNames.Cleanse);
                        CombatController._instance.Guide.MoveToCleanse(this.transform);
                        this.GetComponent<DarknessController>().Cleanse();
                        _am.SetTrigger(TheSpellBook.AnimationTriggerNames.Dizzy.ToString());
                        Destroy(_combatEntity);
                        StartCoroutine(WaitThenDestroy(10));


                    }else
                    {
                        // todo make unique animation?
                        //_am.SetTrigger(TheSpellBook.AnimationTriggerNames.Wave.ToString());
                    }
                    CombatController._instance.EndCombat();
                    
                    if(!isBlacksmith)
                    {
                        WaitEndCombat = StartCoroutine(WaitThenEndCombat(7f));
                        LootButtonManager._instance.SkipButton.gameObject.SetActive(true);
                        LootButtonManager._instance.SkipButton.GetComponent<Button>().onClick.AddListener(SkipWaitEndCombat);
                    }
                    else
                    {
                        WaitEndCombat = StartCoroutine(WaitThenEndCombatBlacksmith(1f));
                    }
                    //}
                }
                
            }
            else
            {
                PlayFabManager._instance.SubmitRunData(false);
                //GameOver
                UIController._instance.PlayDeathSound();
                MusicManager.Instance.PlayDeathMusic();
                CombatController._instance.entitiesInCombat[1].myCharacter._am.SetTrigger("Victory");
                Notification(ErrorMessageManager.Errors.YouHaveDied);
                
                UIController._instance.ActivateDeathScreen();
                
                UIController._instance.ToggleInventoryUI(0);
                _am.SetTrigger("Die");
                //UIController._instance.EndOfGameScreen.SetActive(true);
                //CombatController._instance.
                CombatController._instance.EndCombat();

                //UI controller place restart button on screen

            }
            
        }
    }

    public IEnumerator WaitThenDestroy(float time = 1.5f)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
    
    public IEnumerator WaitThenEndCombat(float time = 1.5f)
    {
        yield return new WaitForSeconds(time);
        UIController._instance.ToggleInventoryUI(1);
        SelectionManager._instance.RandomSelectionFromEquipment(this);
        LootButtonManager._instance.SkipButton.gameObject.SetActive(false);
    }
    public IEnumerator WaitThenEndCombatBlacksmith(float time = 1.5f)
    {
        yield return new WaitForSeconds(time);
        UIController._instance.ToggleInventoryUI(1);
        SelectionManager._instance.CreateChestReward(false, SelectionManager.ChestType.BlackSmith, SelectionManager.ChestType.BlackSmith);
        UIController._instance.ToggleLootUI(1);
        LootButtonManager._instance.SkipButton.gameObject.SetActive(false);
    }
    

    private Coroutine WaitEndCombat;
    public void SkipWaitEndCombat()
    {
        if (WaitEndCombat != null)
        {
            StopCoroutine(WaitEndCombat);
        }
        UIController._instance.ToggleInventoryUI(1);
        SelectionManager._instance.RandomSelectionFromEquipment(this);
        LootButtonManager._instance.SkipButton.gameObject.SetActive(false);
        StartCoroutine(WaitThenDestroy(1.5f));

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
        equipmentArray = _equipment.ToArray();
        _stats = new Dictionary<Stats, int>();

        //base stats
        for (int i = 2; i < 18; i++)
            _stats.Add((Stats)i, 0);

        /*if (isPlayerCharacter)
            Debug.Log("isPlayerCharacter");*/

        foreach (Equipment e in _equipment)
            foreach (KeyValuePair<Stats, int> stat in e.stats)
                if (_stats.ContainsKey(stat.Key))
                    _stats[stat.Key] += stat.Value;
                else
                    _stats.Add(stat.Key, stat.Value);

        /*if (isPlayerCharacter && _stats.ContainsKey(Stats.Health))
            Debug.Log("Health After Equipment: " + _stats[Stats.Health]);*/

        for (int i = 2; i < 18; i++)
            AddBlessings((Stats)i);
        
        //check to see if we dont have any base spell power form items but we do have a spell power blessing
        int SpellPowerCheck = GetIndexOfBlessing(CombatEntity.BlessingTypes.SpellPower);
        
        if (_stats[Stats.SpellPower] == 0 && SpellPowerCheck != -1)
            _stats[Stats.SpellPower] = Mathf.RoundToInt(Blessings[SpellPowerCheck].Item3);
        
        int strengthCheck = GetIndexOfBlessing(CombatEntity.BlessingTypes.Strength);

        if (_stats[Stats.Strength] == 0 && strengthCheck != -1)
            _stats[Stats.Strength] = Mathf.RoundToInt(Blessings[strengthCheck].Item3);

        SetMaxHealth();    
        PrettyPrintStats();
        UpdateStatsEvent(this);
    }

    private void AddBlessings(Stats stat)
    {
        if (!_stats.ContainsKey(stat))
            return;

        int blessingIndex = GetIndexOfBlessing((CombatEntity.BlessingTypes)stat);

        if (blessingIndex == -1)
            return;

        //Debug.Log($"{stat.Key}");
        /*if (isPlayerCharacter && stat == Stats.Health)
            Debug.Log(_stats[stat] + " + " + Blessings[blessingIndex].Item3);*/
        _stats[stat] = Mathf.RoundToInt(_stats[stat] + Blessings[blessingIndex].Item3);
        /*if (isPlayerCharacter && stat == Stats.Health)
            Debug.Log(" = " + _stats[stat] + " after Blessing");*/
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
        int difficulty = CombatController._instance.Difficulty;
        int trial = CombatController._instance.TrialCounter;
        if (isDragon)
        {
            if(difficulty >= 9)
                hp = 200 * _level;
            else if (difficulty >= 8)
                hp = 175 * _level;
            else if (difficulty >= 5)
                hp = 150 * _level;
            else if (difficulty >= 1)
                hp = 125 * _level;
            else
                hp = 100 * _level;
        }
        else if (isElite)
        {
            if(difficulty >= 9)
                hp = 130 * _level;
            else if (difficulty >= 8)
                hp = 120 * _level;
            else if (difficulty >= 5)
                hp = 110 * _level;
            else if (difficulty >= 1)
                hp = 100 * _level;
            else
                hp = 90 * _level;
        }
        else if (isPlayerCharacter)
        {
            //character
            if(difficulty >= 4) 
                hp = 65 * _level;
            else
                hp = 75 * _level;
        }
        else
        {
            if(difficulty >= 9)
                hp = 100 * _level;
            else if (difficulty >= 8)
                hp = 90 * _level;
            else if (difficulty >= 5)
                hp = 85 * _level;
            else if (difficulty >= 1)
                hp = 80 * _level;
            else
                hp = 70 * _level;
        }

        if (!isPlayerCharacter)
        {
            hp += Mathf.RoundToInt(hp * (.05f * (trial -1)));
        }

        int hpFromStats = 0;
        _stats.TryGetValue(Stats.Health, out hpFromStats);

        hp += hpFromStats;
        
        
        
        _maxHealth = hp; // Set Max Health here
        _stats[Stats.Health] = hp;

        /*if (isPlayerCharacter)
        {
            Debug.Log("hpFromStats: " + hpFromStats + "\n" +
                "_stats[Stats.Health]: " + _stats[Stats.Health] + "\n" +
                "max hp:" + _maxHealth + "\n");
        }*/
        
        //only set current health to max if we are out of combat
        if(CombatController._instance.entitiesInCombat.Count <= 1)
            _currentHealth = _maxHealth;
    }
    
    private void PrettyPrintStats()
    {
        string Output = "";
        Output += name + "\n";
        Output += GetWeaponSpellsNames();
        //Output += "Level: " + level + "\n";
        foreach (KeyValuePair<Stats,int> kvp in _stats)
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
