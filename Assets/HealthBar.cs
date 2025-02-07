using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider bar;
    [SerializeField] private Slider tempBar;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private Color DamageColor;
    [SerializeField] private Color HealColor;

    public Camera cam;
    [SerializeField] public Character displayCharacter;
    [SerializeField] private StatusText statusTextPrefab;
    [SerializeField] private BuffDebuffElement buffDebuffElementPrefab;
    [SerializeField] private Transform buffDebuffLayoutGroup;

    [SerializeField] private GameObject BlockIcon;
    [SerializeField] private TextMeshProUGUI BlockText;


    private Dictionary<Stats, int> stats = new Dictionary<Stats, int>();

    [SerializeField] private IntentDisplay intentPrefab;
    [SerializeField] private Transform intentDisplay;
    [SerializeField] private Transform statusDisplay;

    [SerializeField] private GameObject normalFrame;
    [SerializeField] private GameObject eliteFrame;
    [SerializeField] private GameObject dragonFrame;

    public int YValueStatusText = 550;

    public int displayMax;
    public int displayCurrent;

    private void Start()
    {
        CombatEntity.GetHitWithAttack += GetHitWithAttack;
        CombatEntity.GetHitWithBuff += GetHitWithBuff;
        CombatEntity.GetHitWithDeBuff += GetHitWithDeBuff;
        CombatEntity.GetHitWithBlessing += GetHitWithBlessing;


        CombatEntity.GetHealed += GetHealed;
        Character.UpdateBlock += UpdateBlock;
        Character.UsePrep += UsePrepared;


        CombatEntity.AddIntent += AddIntent;
        CombatEntity.RemoveIntent += RemoveIntent;
        CombatEntity.RemoveAllIntent += RemoveAllIntent;

        CombatEntity.ReduceDebuffCount += ReduceDebuffCount;
        CombatEntity.ReduceBuffCount += ReduceBuffCount;

        CombatController.EndTurn += FixBars;


    }

    private void OnDestroy()
    {
        CombatEntity.GetHitWithBuff -= GetHitWithBuff;
        CombatEntity.GetHitWithAttack -= GetHitWithAttack;
        CombatEntity.GetHitWithDeBuff -= GetHitWithDeBuff;
        CombatEntity.GetHitWithBlessing -= GetHitWithBlessing;

        
        CombatEntity.GetHealed -= GetHealed;
        Character.UpdateBlock -= UpdateBlock;
        Character.UsePrep -= UsePrepared;


        CombatEntity.AddIntent -= AddIntent;
        CombatEntity.RemoveIntent -= RemoveIntent;
        CombatEntity.RemoveAllIntent -= RemoveAllIntent;

        CombatEntity.ReduceDebuffCount -= ReduceDebuffCount;
        CombatEntity.ReduceBuffCount -= ReduceBuffCount;

        CombatController.EndTurn -= FixBars;




    }

    private void FixBars()
    {
        //StartCoroutine(FixTheBar(displayCharacter._currentHealth, bar.value, .5f));

    }

    private void ReduceDebuffCount(Character c)
    {
        if (c != displayCharacter)
            return;
        for (int i = buffDebuffLayoutGroup.childCount - 1; i >= 0; i--)
        {
            BuffDebuffElement debuff = buffDebuffLayoutGroup.GetChild(i).GetComponent<BuffDebuffElement>();

            if (debuff.isDebuff && !debuff.isBlessing)
            {
                debuff._turns -= 1;
                debuff.UpdateValues();

                if (debuff._turns <= 0)
                {
                    UIPooler._instance.buffDebuffPool.Add(debuff.gameObject);
                    debuff.transform.SetParent(UIPooler._instance.transform);
                    debuff.gameObject.SetActive(false);

                }
                StatusText st = GetStatus();

                st.transform.localPosition += new Vector3(0, YValueStatusText, 0);
                st.InitializeStatusText(-1, -1, debuff._debuff, this);
            }
           


        }
    }

    private void ReduceBuffCount(Character c, bool includePrep)
    {
        if (c != displayCharacter)
            return;

        for (int i = buffDebuffLayoutGroup.childCount - 1; i >= 0; i--)
        {
            BuffDebuffElement buff = buffDebuffLayoutGroup.GetChild(i).GetComponent<BuffDebuffElement>();
            
            

            if (!buff.isDebuff && !buff.isBlessing)
            {
                if (buff._buff == CombatEntity.BuffTypes.Block)
                {
                    continue;
                }
                if (buff._buff == CombatEntity.BuffTypes.Prepared && !includePrep)
                {
                    continue;
                }
                buff._turns -= 1;
                buff.UpdateValues();

                if (buff._turns <= 0)
                {
                    

                    UIPooler._instance.buffDebuffPool.Add(buff.gameObject);
                    buff.transform.SetParent(UIPooler._instance.transform);
                    buff.gameObject.SetActive(false);
                    

                }
                
                StatusText st = GetStatus();

                st.transform.localPosition += new Vector3(0, YValueStatusText, 0);
                st.InitializeStatusText(-1, -1, buff._buff, this);
                


            }
        }
    }

    private void AddIntent(Character c, SpellTypes spell)
    {
        if (c != displayCharacter)
            return;

        if (RelicManager._instance.CheckRelic(RelicType.DragonRelic12))
        {
            return;
        }
        
        //Debug.Log(spell);

        IntentDisplay intent = GetIntent();
        intent.UpdateInfo(spell);
    }

    private void RemoveIntent(Character c)
    {
        if (c != displayCharacter)
            return;

        int count = intentDisplay.childCount;
        if (count == 0)
        {
            return;
        }
        intentDisplay.GetChild(count - 1).gameObject.SetActive(false);
        UIPooler._instance.IntentPool.Add(intentDisplay.GetChild(count - 1).gameObject);
        intentDisplay.GetChild(count - 1).SetParent(UIPooler._instance.transform);
        
    }
    private void RemoveAllIntent(Character c)
    {
        if (c != displayCharacter)
            return;


        for (int i = intentDisplay.childCount - 1 ; i >= 0; i--)
        {
            //Debug.Log("removing all intents");

            UIPooler._instance.IntentPool.Add(intentDisplay.GetChild(i).gameObject);
            intentDisplay.GetChild(i).gameObject.SetActive(false);
            intentDisplay.GetChild(i).SetParent(UIPooler._instance.transform);


        }
    }


    private void UpdateBlock(Character c, int amount, int change)
    {
        if (c != displayCharacter)
            return;

        if (amount <= 0)
        {
            // remove icon
            BlockIcon.SetActive(false);
        }
        else
        {
            BlockIcon.SetActive(true);
            BlockText.text = amount.ToString();
        }

        StatusText st = GetStatus();

        st.transform.localPosition += new Vector3(0, YValueStatusText, 0);
        // we really want the change
        st.InitializeStatusText(1, change, CombatEntity.BuffTypes.Block, this);


    }
    private void UsePrepared(Character c)
    {
        if (c != displayCharacter)
            return;

       

        StatusText st = GetStatus();

        st.transform.localPosition += new Vector3(0, YValueStatusText, 0);
        // we really want the change
        st.InitializeStatusText(-1, -1, CombatEntity.BuffTypes.Prepared, this);
        
        ReduceSpecificBuff(CombatEntity.BuffTypes.Prepared);


    }

    private void ReduceSpecificBuff(CombatEntity.BuffTypes buff)
    {
        // loop through buffs
        for (int i = buffDebuffLayoutGroup.childCount - 1; i >= 0; i--)
        {
            BuffDebuffElement buffDisplay = buffDebuffLayoutGroup.GetChild(i).GetComponent<BuffDebuffElement>();

            if (!buffDisplay.isDebuff)
            {
                if (buffDisplay._buff == buff)
                {
                    buffDisplay._turns -= 1;
                    buffDisplay.UpdateValues();

                    if (buffDisplay._turns <= 0)
                    {
                    

                        UIPooler._instance.buffDebuffPool.Add(buffDisplay.gameObject);
                        buffDisplay.transform.SetParent(UIPooler._instance.transform);
                        buffDisplay.gameObject.SetActive(false);
                    

                    }
                    return;
                }
                
                
            }
        }
        
    }

    private void GetHitWithBuff(Character c, CombatEntity.BuffTypes buff, int turns, float amount)
    {
        if (c != displayCharacter)
            return;

        if (buff == CombatEntity.BuffTypes.Block)
        {
            //do nothing, covered elsewhere
            return;
        }

        //decide if we make new or add to existing
        // make the icon
        if (MakeNewOne(buff, CombatEntity.DeBuffTypes.None, CombatEntity.BlessingTypes.None))
        {
            BuffDebuffElement icon = GetBuffDebuff();
            icon.InitializeDisplay(buff, turns, amount);
        }
        else
        {
            bool found = false;
            // find the instance, add 1 to the turns
            for (int i = 0; i < buffDebuffLayoutGroup.childCount; i++)
            {
                BuffDebuffElement b = buffDebuffLayoutGroup.GetChild(i).GetComponent<BuffDebuffElement>();
                if (b._buff == buff)
                {
                    if (buff == CombatEntity.BuffTypes.Empowered)
                    {
                        b._amount += amount;
                        if (b._amount > 75)
                        {
                            b._amount = 75;
                        }
                    }
                    if (buff == CombatEntity.BuffTypes.Shatter)
                    {
                        b._amount += amount;
                        if (b._amount > 30)
                        {
                            b._amount = 30;
                        }
                    }

                    if (buff == CombatEntity.BuffTypes.Thorns)
                    {
                        float tempAmount = b._amount;
                        if (amount > tempAmount)
                            tempAmount = amount;
                        b._amount = tempAmount;
                    }
                    //if(buff == CombatEntity.BuffTypes.Rejuvenate || 
                    b._turns += turns;
                    b.UpdateValues();
                    found = true;

                }
            }

            if (!found)
            {
                BuffDebuffElement icon = GetBuffDebuff();

                icon.InitializeDisplay(buff, turns, amount);
            }
        }

        StatusText st = GetStatus();
        st.transform.localPosition += new Vector3(0, YValueStatusText, 0);
        st.InitializeStatusText(turns, Mathf.RoundToInt(amount), buff, this);
    }

    private void GetHitWithDeBuff(Character c, CombatEntity.DeBuffTypes debuff, int turns, float amount)
    {
        if (c != displayCharacter)
            return;


        //Debug.Log("turns are 3 = " + turns);
        // make the icon
        if (MakeNewOne(CombatEntity.BuffTypes.None,debuff , CombatEntity.BlessingTypes.None))
        {
            BuffDebuffElement icon = GetBuffDebuff();

            icon.InitializeDisplay(debuff, turns, amount);
        }
        else
        {
            bool found = false;
            // find the instance, add 1 to the turns
            for (int i = 0; i < buffDebuffLayoutGroup.childCount; i++)
            {
                BuffDebuffElement b = buffDebuffLayoutGroup.GetChild(i).GetComponent<BuffDebuffElement>();
                if (b._debuff == debuff)
                {
                    if (debuff == CombatEntity.DeBuffTypes.Weakened)
                    {
                        b._amount += amount;
                        if (b._amount > 75)
                        {
                            b._amount = 75;
                        }
                    }
                    if (debuff == CombatEntity.DeBuffTypes.Burn || debuff == CombatEntity.DeBuffTypes.Lacerate)
                    {
                        
                        
                        float tempAmount = b._amount;
                        if (amount > tempAmount)
                            tempAmount = amount;
                        b._amount = tempAmount;
                    }
                    if (debuff == CombatEntity.DeBuffTypes.Exposed)
                    {
                        b._amount += 10;
                        if (b._amount > 50)
                        {
                            b._amount = 50;
                        }
                    }
                    b._turns += turns;
                    b.UpdateValues();
                    found = true;
                }
                
                
            }

            if (!found)
            {
                //Debug.Log("making a new one");
                BuffDebuffElement icon = GetBuffDebuff();

                icon.InitializeDisplay(debuff, turns, amount);
            }
        }

        StatusText st = GetStatus();
        st.transform.localPosition += new Vector3(0, YValueStatusText, 0);
        st.InitializeStatusText(turns, Mathf.RoundToInt(amount), debuff, this);
    }
    
    private void GetHitWithBlessing(Character c, CombatEntity.BlessingTypes blessing, int turns, float amount)
    {
        if (c != displayCharacter)
            return;


        // make the icon
        if (MakeNewOne(CombatEntity.BuffTypes.None,CombatEntity.DeBuffTypes.None,blessing ))
        {
            BuffDebuffElement icon = GetBuffDebuff();

            icon.InitializeDisplay(blessing, turns, amount);
        }
        else
        {
            bool found = false;
            // find the instance, add 1 to the turns
            for (int i = 0; i < buffDebuffLayoutGroup.childCount; i++)
            {
                BuffDebuffElement b = buffDebuffLayoutGroup.GetChild(i).GetComponent<BuffDebuffElement>();
                if (b._blessing == blessing)
                {
                    b._amount += amount;
                    //b._turns += turns;
                    b.UpdateValues();
                    found = true;
                    if (b._amount <= 0)
                    {
                        UIPooler._instance.buffDebuffPool.Add(b.gameObject);
                        b.transform.SetParent(UIPooler._instance.transform);
                        b.gameObject.SetActive(false);
                    }
                }
                
                
            }

            if (!found)
            {
                //Debug.Log("making a new one");
                BuffDebuffElement icon = GetBuffDebuff();

                icon.InitializeDisplay(blessing, turns, amount);
            }
        }

        //todo blessing status text?
        //StatusText st = GetStatus();
        //st.transform.localPosition += new Vector3(0, 350, 0);
        //st.InitializeStatusText(turns, Mathf.RoundToInt(amount), blessing, this);
    }

    private bool MoveBar = false;
    private bool MoveTempBar = false;

    private float TargetValue;
    private float TargetValueStart;
    private float timer = 0;
    
    private float TempBarTarget;
    private float TempBarStart;
    private float tempTimer = 0;

    private float waitTimer = 1.5f;
    
    private float ratio;

    
    private void Update()
    {
        if(displayCharacter._maxHealth != displayMax || displayCharacter._currentHealth != displayCurrent)
        {
            if (displayCharacter == CombatController._instance.Player)
            {
                if (displayCharacter._maxHealth != displayMax)
                    Debug.Log("Fix Max Health: " + displayMax + " -> " + displayCharacter._maxHealth);

                if (displayCharacter._currentHealth != displayCurrent)
                    Debug.Log("Fix Max Health: " + displayCurrent + " -> " + displayCharacter._currentHealth);
            }

            SetBarAndText(displayCharacter._currentHealth, displayCharacter._maxHealth);
        }        

        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }
        
        if (MoveBar)
        {
            timer += Time.deltaTime;
            bar.value = Mathf.Lerp(TargetValueStart, TargetValue, timer );
            if (bar.value >= TargetValue)
            {
                bar.value = TargetValue;
                MoveBar = false;
                timer = 0;
                TargetValueStart = 0;
                TempBarStart = 0;

            }
        }
        else if (MoveTempBar)
        {
            tempTimer += Time.deltaTime;
            tempBar.value = Mathf.Lerp(TempBarStart, TempBarTarget, tempTimer );
            if (tempBar.value <= TempBarTarget)
            {
                tempBar.value = TempBarTarget;
                MoveTempBar = false;
                tempTimer = 0;
                TempBarStart = 0;
                timer = 0;

                
            }
        }
        else if(!Mathf.Approximately(tempBar.value,bar.value))
        {
            MoveTempBar = true;
            TempBarTarget = bar.value;
        }
    }

    //private Coroutine MovingBar;

    private void GetHealed(Character c, int heal)
    {
        //Debug.Log("we are doing the healing with the bar no?");
        // if(MovingBar != null)
        //     StopCoroutine(MovingBar);
        if (c != displayCharacter)
            return;

        tempBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = HealColor;

        tempBar.value = c._currentHealth;
        //MovingBar = StartCoroutine(LerpValueHeal(bar.value, (float)c._currentHealth, 2));
        MoveBar = true;
        TargetValue = c._currentHealth;
        if (TargetValueStart == 0)
            TargetValueStart = bar.value;

        SetBarAndText(c._currentHealth, c._maxHealth);
        waitTimer = 1.5f;
        timer = 0;
        MoveTempBar = false;
        TempBarStart = 0;

        StatusText st = GetStatus();
        st.transform.localPosition += new Vector3(0, YValueStatusText, 0);
        st.InitializeStatusText(heal, CombatEntity.AbilityTypes.Heal, this);


    }

    private void SetBarAndText(int current, int max)
    {
        displayCurrent = current;
        displayMax = max;
        bar.value = displayCurrent;
        bar.maxValue = displayMax;
        text.text = displayCurrent + "/" + displayMax;
    }

    private void GetHitWithAttack(Character c, CombatEntity.AbilityTypes abilityTypes, int amount, int reduction = 0)
    {

        // if(MovingBar != null)
        //     StopCoroutine(MovingBar);
        if (c != displayCharacter)
            return;
        bar.value -= amount;

        tempBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = DamageColor;


        //start of coroutine to reduce the temp bar
        if (!this.gameObject.activeInHierarchy)
        {
            return;
        }
        //Debug.Log("Active? "+gameObject.activeInHierarchy);
        //MovingBar = StartCoroutine(LerpValueDamage(tempBar.value, (float)bar.value, 2));
        if(TempBarStart == 0)
            TempBarStart = (c._currentHealth + amount);
        TempBarTarget = c._currentHealth;
        //Debug.Log("start: "+TempBarStart + " target: " + TempBarTarget +" "+ bar.value);
        waitTimer = 1.5f;
        tempTimer = 0;


        MoveTempBar = true;
        SetBarAndText(c._currentHealth , c._maxHealth);

        TargetValueStart = 0;
        MoveBar = false;

        StatusText st = GetStatus();
        st.transform.localPosition += new Vector3(0, YValueStatusText, 0);
        st.InitializeStatusText(amount, abilityTypes, this,reduction);
    }

    IEnumerator LerpValueDamage(float start, float end, float timeToMove)
    {
        yield return new WaitForSeconds(1);
        float t = 0;
        while (t < 1)
        {
            tempBar.value = Mathf.Lerp(start, end, t);
            t = t + Time.deltaTime / timeToMove;
            yield return new WaitForEndOfFrame();
        }

        tempBar.value = end;
        //MovingBar = null;

    }

    IEnumerator LerpValueHeal(float start, float end, float timeToMove)
    {
        yield return new WaitForSeconds(1);
        float t = 0;
        while (t < 1)
        {
            bar.value = Mathf.Lerp(start, end, t);
            t = t + Time.deltaTime / timeToMove;
            yield return new WaitForEndOfFrame();
        }

        bar.value = end;
        //MovingBar = null;


    }
    IEnumerator FixTheBar(float currentHP, float BarHP, float timeToMove)
    {
        
        if (currentHP == BarHP)
        {
            yield break;
        }

        //Debug.Log("Fixing the bar");
        if (currentHP > BarHP)
        {
            float t = 0;
            while (t < 1)
            {
                bar.value = Mathf.Lerp(BarHP, currentHP, t);
                t = t + Time.deltaTime / timeToMove;
                yield return new WaitForEndOfFrame();
            }

            bar.value = currentHP;
        }
        else
        {
            float t = 0;
            while (t < 1)
            {
                tempBar.value = Mathf.Lerp(BarHP, currentHP, t);
                t = t + Time.deltaTime / timeToMove;
                yield return new WaitForEndOfFrame();
            }

            tempBar.value = currentHP;
        }
        
        

    }

    public void Initialize(Character c, Camera cam)
    {
        displayCharacter = c;
        stats = c.GetComponent<Character>().GetStats();


        // foreach (var i in stats)
        // {
        //     Debug.Log(i.Key + " : " + i.Value);
        // }

        // get screen posiiton

        Vector3 screenPos = cam.WorldToScreenPoint(c.transform.position) - new Vector3(0, 50, 0);
        this.transform.position = screenPos;

        
        //ratio = 100f / c._maxHealth;
        SetBarAndText(c._currentHealth, c._maxHealth);

        tempBar.maxValue = c._maxHealth;
        tempBar.value = c._currentHealth;

        normalFrame.gameObject.SetActive(false);
        eliteFrame.gameObject.SetActive(false);
        dragonFrame.gameObject.SetActive(false);
        if (c.isElite)
        {
            eliteFrame.gameObject.SetActive(true);
        }else if (c.isDragon)
        {
            dragonFrame.gameObject.SetActive(true);
        }
        else
        {
            normalFrame.gameObject.SetActive(true);
        }

    }

    public bool MakeNewOne(CombatEntity.BuffTypes buff, CombatEntity.DeBuffTypes debuff, CombatEntity.BlessingTypes blessing)
    {
        if (buff != CombatEntity.BuffTypes.None)
        {
            switch (buff)
            {
                case CombatEntity.BuffTypes.Rejuvenate:
                    return false;
                case CombatEntity.BuffTypes.Thorns:
                    return false;
                case CombatEntity.BuffTypes.Invulnerable:
                    return false;
                case CombatEntity.BuffTypes.Empowered:
                    return false;
                case CombatEntity.BuffTypes.Shatter:
                    return false;
                case CombatEntity.BuffTypes.Immortal:
                    return false;
                case CombatEntity.BuffTypes.Prepared:
                    return false;
            }
        }

        if (debuff != CombatEntity.DeBuffTypes.None)
        {
            switch (debuff)
            {
                case CombatEntity.DeBuffTypes.Lacerate:
                    return false;
                case CombatEntity.DeBuffTypes.Burn:
                    return false;
                case CombatEntity.DeBuffTypes.Wounded:
                    return false;
                case CombatEntity.DeBuffTypes.Weakened:
                    return false;
                case CombatEntity.DeBuffTypes.Chilled:
                    return false;
                case CombatEntity.DeBuffTypes.Exposed:
                    return false;
            }
        }
        if (blessing != CombatEntity.BlessingTypes.None)
        {
            switch (blessing)
            {
                default:
                    return false;
            }
        }

        else
        {
            Debug.LogWarning("Invalid Input");
            
        }

        return false;
    }

    private void OnDisable()
    {
        Destroy(this.gameObject);
    }

    private StatusText GetStatus()
    {
        StatusText st;
        if (UIPooler._instance.StatusTextsPool.Count != 0)
        {
            st = UIPooler._instance.StatusTextsPool[0].GetComponent<StatusText>();
            UIPooler._instance.StatusTextsPool.Remove(st.gameObject);
            st.gameObject.SetActive(true);
            st.transform.SetParent(statusDisplay);
            st.transform.localPosition = Vector3.zero;
            
        }
        else
        {
            st = Instantiate(statusTextPrefab, statusDisplay);

        }

        return st;
    }

    private IntentDisplay GetIntent()
    {
        IntentDisplay intent;
        if (UIPooler._instance.IntentPool.Count != 0)
        {
            intent = UIPooler._instance.IntentPool[0].GetComponent<IntentDisplay>();
            UIPooler._instance.IntentPool.Remove(intent.gameObject);
            intent.gameObject.SetActive(true);
            intent.transform.SetParent(intentDisplay);
            
            
        }
        else
        {
            intent = Instantiate(intentPrefab, intentDisplay);

        }

        return intent;
    }
    private BuffDebuffElement GetBuffDebuff()
    {
        BuffDebuffElement buffDebuff;
        if (UIPooler._instance.buffDebuffPool.Count != 0)
        {
            buffDebuff = UIPooler._instance.buffDebuffPool[0].GetComponent<BuffDebuffElement>();
            buffDebuff._debuff = CombatEntity.DeBuffTypes.None;
            buffDebuff._buff = CombatEntity.BuffTypes.None;
            UIPooler._instance.buffDebuffPool.Remove(buffDebuff.gameObject);
            buffDebuff.gameObject.SetActive(true);
            buffDebuff.transform.SetParent(buffDebuffLayoutGroup);
            
        }
        else
        {
            buffDebuff = Instantiate(buffDebuffElementPrefab, buffDebuffLayoutGroup);

        }

        return buffDebuff;
    }
    
}
