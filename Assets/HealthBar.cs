using System;
using System.Collections;
using System.Collections.Generic;
using ImportantStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider bar;
    [SerializeField] private Slider tempBar;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private Color DamageColor;
    [SerializeField] private Color HealColor;

    public Camera cam;
    [SerializeField]public Character displayCharacter;
    [SerializeField] private StatusText statusTextPrefab;

    private Dictionary<Equipment.Stats, int> stats = new Dictionary<Equipment.Stats, int>();

    private void Start()
    {
        CombatEntity.GetHitWithAttack += GetHitWithAttack;
        CombatEntity.GetHealed += GetHealed;
        
    }

    private void OnDestroy()
    {
        CombatEntity.GetHitWithAttack -= GetHitWithAttack;
        CombatEntity.GetHealed -= GetHealed;
    }

    private void GetHealed(Character c, int heal)
    {
        //Debug.Log("we are doing the healing with the bar no?");
        StopAllCoroutines();
        if (c != displayCharacter)
            return;
        
        tempBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = HealColor;

        tempBar.value =  c._currentHealth;
        StartCoroutine(LerpValueHeal(bar.value, (float)c._currentHealth, 2));
        text.text = c._currentHealth + "/" + c._maxHealth;
        
        StatusText st = Instantiate(statusTextPrefab, this.transform);
        st.transform.localPosition += new Vector3(0, 300, 0);
        st.InitializeStatusText(heal, CombatEntity.AbilityTypes.Heal);

        
    }

    private void GetHitWithAttack(Character c, CombatEntity.AbilityTypes abilityTypes, int amount, int reduction = 0)
    {

        StopAllCoroutines();
        if (c != displayCharacter)
            return;
        bar.value = bar.value - amount;
        tempBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = DamageColor;

        
        //start of coroutine to reduce the temp bar
        StartCoroutine(LerpValueDamage(tempBar.value, (float)bar.value, 2));
        text.text = c._currentHealth + "/" + c._maxHealth;

        StatusText st = Instantiate(statusTextPrefab, this.transform);
        st.transform.localPosition += new Vector3(0, 300, 0);
        st.InitializeStatusText(amount, abilityTypes, reduction);
    }
    
    IEnumerator LerpValueDamage(float start, float end, float timeToMove)
    {
        yield return new WaitForSeconds(1);
        float t = 0;
        while(t < 1)
        {
            tempBar.value = Mathf.Lerp(start,end,t);
            t = t + Time.deltaTime / timeToMove;
            yield return new WaitForEndOfFrame();
        }
        tempBar.value = end;
        
    }
    IEnumerator LerpValueHeal(float start, float end, float timeToMove)
    {
        yield return new WaitForSeconds(1);
        float t = 0;
        while(t < 1)
        {
            bar.value = Mathf.Lerp(start,end,t);
            t = t + Time.deltaTime / timeToMove;
            yield return new WaitForEndOfFrame();
        }
        bar.value = end;
        
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
        
        Vector3 screenPos = cam.WorldToScreenPoint(c.transform.position) - new Vector3(0,50, 0);
        this.transform.position = screenPos;
        bar.maxValue = c._maxHealth;
        bar.value = c._currentHealth;
        text.text = c._currentHealth + "/" + c._maxHealth;
        
        tempBar.maxValue = c._maxHealth;
        tempBar.value = c._currentHealth;


    }
}
