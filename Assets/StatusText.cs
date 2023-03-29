using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusText : MonoBehaviour
{
    public TextMeshProUGUI AmountText;
    public TextMeshProUGUI ReductionText;
    public Image Icon;

    private HealthBar _healthBar;
    
    //have colors for each abilitytypes
    

    public Sprite blockIcon;
    public Sprite physicalAttackIcon;
    public Sprite healIcon;
    public Sprite spellAttackIcon;
    
    
    

    public void InitializeStatusText(int amount, CombatEntity.AbilityTypes abilityTypes, HealthBar hb, int reduction = 0)
    {
        _healthBar = hb;
        Icon.sprite = null;
        Icon.color = Color.white;

        switch (abilityTypes)
        {
            case CombatEntity.AbilityTypes.PhysicalAttack:
                AmountText.text = amount.ToString();
                AmountText.color = TheSpellBook._instance.abilityColors[(int)abilityTypes];
                ReductionText.text = "(" + reduction + ")";
                ReductionText.color = Color.gray;
                Icon.sprite = physicalAttackIcon;
                break;
            case CombatEntity.AbilityTypes.SpellAttack:
                AmountText.text = amount.ToString();
                AmountText.color = TheSpellBook._instance.abilityColors[(int)abilityTypes];
                Icon.sprite = spellAttackIcon;
                //Icon.color = abilityColors[abilityTypes.GetHashCode()];

                ReductionText.text = "(" + reduction + ")";
                ReductionText.color = Color.gray;
                break;
                
            case CombatEntity.AbilityTypes.Heal:
                AmountText.text = amount.ToString();
                AmountText.color = TheSpellBook._instance.abilityColors[(int)abilityTypes];
                ReductionText.text = "";
                Icon.sprite = healIcon;
                
                break;
        }
        StartMovement();
    }
    
    public void InitializeStatusText(int turns, int amount, CombatEntity.BuffTypes buffTypes, HealthBar hb)
    {
        _healthBar = hb;

        Icon.sprite = TheSpellBook._instance.GetSprite(buffTypes);
        
        if(buffTypes == CombatEntity.BuffTypes.Block){
            AmountText.text = amount.ToString();
            AmountText.color = TheSpellBook._instance.abilityColors[5];
            ReductionText.text = "";
        }
        else
        {
            AmountText.text = buffTypes.ToString();
            AmountText.color = TheSpellBook._instance.abilityColors[2];
            ReductionText.text = "(" + turns +")";
            ReductionText.color = TheSpellBook._instance.abilityColors[2];
        }
        StartMovement();

    }
    public void InitializeStatusText(int turns, int amount, CombatEntity.DeBuffTypes debuffTypes, HealthBar hb)
    {
        _healthBar = hb;

        Icon.sprite = TheSpellBook._instance.GetSprite(debuffTypes);
        AmountText.text = debuffTypes.ToString();
        AmountText.color = TheSpellBook._instance.abilityColors[3];
        ReductionText.text = "(" + turns +")";
        
        ReductionText.color = TheSpellBook._instance.abilityColors[3];

        StartMovement();
    }

    private void StartMovement()
    {
        Icon.color = Color.white;
        if (!this.isActiveAndEnabled)
        {
            return;
        }
        StartCoroutine(LerpPos(transform.position, transform.position + new Vector3(0, 200, 0), 4f));
        StartCoroutine(Fade(1f, 1f, AmountText));
        StartCoroutine(Fade(1f, 1f, ReductionText));
        StartCoroutine(Fade(1f, 1f, Icon));


    }
    
    IEnumerator LerpPos(Vector3 start, Vector3 end, float timeToMove)
    {
        float t = 0;
        while(t < 1)
        {
            transform.position = Vector3.Lerp(start,end,t);
            t = t + Time.deltaTime / timeToMove;
            yield return new WaitForEndOfFrame();
        }
        transform.position = end;
        
    }
    private IEnumerator Fade(float initialWait, float fadeDuration, TextMeshProUGUI text)
    {
        yield return new WaitForSeconds(initialWait);
        Color initialColor = text.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            text.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
            yield return null;
        }
    }
    private IEnumerator Fade(float initialWait, float fadeDuration, Image icon)
    {
        yield return new WaitForSeconds(initialWait);
        Color initialColor = icon.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            icon.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
            yield return null;
        }
        MoveFinished();
    }
    

    private void MoveFinished()
    {
        UIPooler._instance.StatusTextsPool.Add(this.gameObject);
        this.transform.SetParent(UIPooler._instance.transform);
        this.gameObject.SetActive(false);

    }
}
