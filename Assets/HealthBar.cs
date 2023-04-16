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


    private Dictionary<Equipment.Stats, int> stats = new Dictionary<Equipment.Stats, int>();

    [SerializeField] private IntentDisplay intentPrefab;
    [SerializeField] private Transform intentDisplay;
    [SerializeField] private Transform statusDisplay;


   


    private void Start()
    {
        CombatEntity.GetHitWithAttack += GetHitWithAttack;
        CombatEntity.GetHitWithBuff += GetHitWithBuff;
        CombatEntity.GetHitWithDeBuff += GetHitWithDeBuff;

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
        StartCoroutine(FixTheBar(displayCharacter._currentHealth, bar.value, .5f));

    }

    private void ReduceDebuffCount(Character c)
    {
        if (c != displayCharacter)
            return;
        for (int i = buffDebuffLayoutGroup.childCount - 1; i >= 0; i--)
        {
            BuffDebuffElement debuff = buffDebuffLayoutGroup.GetChild(i).GetComponent<BuffDebuffElement>();

            if (debuff.isDebuff)
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

                st.transform.localPosition += new Vector3(0, 350, 0);
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
            
            

            if (!buff.isDebuff)
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

                st.transform.localPosition += new Vector3(0, 350, 0);
                st.InitializeStatusText(-1, -1, buff._buff, this);
                


            }
        }
    }

    private void AddIntent(Character c, Weapon.SpellTypes spell)
    {
        if (c != displayCharacter)
            return;

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
            intentDisplay.GetChild(i).gameObject.SetActive(false);
            UIPooler._instance.IntentPool.Add(intentDisplay.GetChild(i).gameObject);
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

        st.transform.localPosition += new Vector3(0, 350, 0);
        // we really want the change
        st.InitializeStatusText(1, change, CombatEntity.BuffTypes.Block, this);


    }
    private void UsePrepared(Character c)
    {
        if (c != displayCharacter)
            return;

       

        StatusText st = GetStatus();

        st.transform.localPosition += new Vector3(0, 350, 0);
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
        if (MakeNewOne(buff, CombatEntity.DeBuffTypes.None))
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

                    if (buff == CombatEntity.BuffTypes.Rejuvenate || buff == CombatEntity.BuffTypes.Thorns)
                    {
                        b._amount += amount;
                    }
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
        st.transform.localPosition += new Vector3(0, 350, 0);
        st.InitializeStatusText(turns, Mathf.RoundToInt(amount), buff, this);
    }

    private void GetHitWithDeBuff(Character c, CombatEntity.DeBuffTypes debuff, int turns, float amount)
    {
        if (c != displayCharacter)
            return;


        // make the icon
        if (MakeNewOne(CombatEntity.BuffTypes.None,debuff ))
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
                    if (debuff == CombatEntity.DeBuffTypes.Burn || debuff == CombatEntity.DeBuffTypes.Bleed)
                    {
                        b._amount += amount;
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
                Debug.Log("making a new one");
                BuffDebuffElement icon = GetBuffDebuff();

                icon.InitializeDisplay(debuff, turns, amount);
            }
        }

        StatusText st = GetStatus();
        st.transform.localPosition += new Vector3(0, 350, 0);
        st.InitializeStatusText(turns, Mathf.RoundToInt(amount), debuff, this);
    }

    private void GetHealed(Character c, int heal)
    {
        //Debug.Log("we are doing the healing with the bar no?");
        StopAllCoroutines();
        if (c != displayCharacter)
            return;

        tempBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = HealColor;

        tempBar.value = c._currentHealth;
        StartCoroutine(LerpValueHeal(bar.value, (float)c._currentHealth, 2));
        text.text = c._currentHealth + "/" + c._maxHealth;

        StatusText st = GetStatus();
        st.transform.localPosition += new Vector3(0, 350, 0);
        st.InitializeStatusText(heal, CombatEntity.AbilityTypes.Heal, this);


    }

    private void GetHitWithAttack(Character c, CombatEntity.AbilityTypes abilityTypes, int amount, int reduction = 0)
    {

        StopAllCoroutines();
        if (c != displayCharacter)
            return;
        
        bar.value = bar.value - amount;
        tempBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = DamageColor;


        //start of coroutine to reduce the temp bar
        if (!this.gameObject.activeInHierarchy)
        {
            return;
        }
        //Debug.Log("Active? "+gameObject.activeInHierarchy);
        StartCoroutine(LerpValueDamage(tempBar.value, (float)bar.value, 2));
        text.text = c._currentHealth + "/" + c._maxHealth;

        StatusText st = GetStatus();
        st.transform.localPosition += new Vector3(0, 350, 0);
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
        bar.maxValue = c._maxHealth;
        bar.value = c._currentHealth;
        text.text = c._currentHealth + "/" + c._maxHealth;

        tempBar.maxValue = c._maxHealth;
        tempBar.value = c._currentHealth;


    }

    public bool MakeNewOne(CombatEntity.BuffTypes buff, CombatEntity.DeBuffTypes debuff)
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
                case CombatEntity.DeBuffTypes.Bleed:
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
