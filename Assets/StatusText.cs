using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CombatEntity;

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

    bool _isCrit = false;
    bool _isBuff = false;

    const int _shakeCount = 20;
    const float _shakeWidth = 20f;
    const float _critSizeMultiplier = 1.5f;
    
    const float _attackDivisor = 3000f;
    const float _buffDivisor = 50000f;
    const float _maxNumerator = 5000f;
    const float _bigRatioAttack = 1f;
    const float _bigRatioBuff = .25f;
    const float _smallRatio = 0.1f;
    const float _bigTime = 0.2f;
    const float _initialWait = 1f;
    const float _fadeDuration = 1f;

    public void InitializeStatusText(int amount, CombatEntity.AbilityTypes abilityTypes, HealthBar hb, bool isCrit, int reduction = 0)
    {
        _healthBar = hb;
        _isCrit = isCrit;
        _isBuff = false;
        Icon.sprite = null;
        Icon.color = Color.white;

        switch (abilityTypes)
        {
            case CombatEntity.AbilityTypes.PhysicalAttack:
                AmountText.text = amount.ToString();
                AmountText.color = TheSpellBook._instance.abilityColors[(int)abilityTypes];
                if (reduction > 0)
                    ReductionText.text = "(" + reduction + ")";
                else
                    ReductionText.text = "";
                ReductionText.color = Color.gray;
                Icon.sprite = physicalAttackIcon;

                break;

            case CombatEntity.AbilityTypes.SpellAttack:
                AmountText.text = amount.ToString();
                AmountText.color = TheSpellBook._instance.abilityColors[(int)abilityTypes];
                Icon.sprite = spellAttackIcon;
                //Icon.color = abilityColors[abilityTypes.GetHashCode()];
                if (reduction > 0)
                    ReductionText.text = "(" + reduction + ")";
                else
                    ReductionText.text = "";
                ReductionText.color = Color.gray;

                break;
                
            case CombatEntity.AbilityTypes.Heal:
                AmountText.text = amount.ToString();
                AmountText.color = TheSpellBook._instance.abilityColors[(int)abilityTypes];
                ReductionText.text = "";
                Icon.sprite = healIcon;
                
                break;
        }
        //Debug.Log(abilityTypes + ": " + Mathf.Abs(amount));
        SetSize(amount, _attackDivisor);
        StartMovement();
    }

    public void InitializeStatusText(int turns, int amount, CombatEntity.BuffTypes buffTypes, HealthBar hb)
    {
        _healthBar = hb;
        _isCrit = false;
        _isBuff = true;

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
        //Debug.Log(buffTypes + ": " + Mathf.Abs(amount));
        SetSize(amount, _buffDivisor);
        StartMovement();
    }

    public void InitializeStatusText(int turns, int amount, CombatEntity.DeBuffTypes debuffTypes, HealthBar hb)
    {
        _healthBar = hb;
        _isCrit = false;
        _isBuff = true;

        Icon.sprite = TheSpellBook._instance.GetSprite(debuffTypes);
        AmountText.text = debuffTypes.ToString();
        AmountText.color = TheSpellBook._instance.abilityColors[3];
        ReductionText.text = "(" + turns +")";
        
        ReductionText.color = TheSpellBook._instance.abilityColors[3];
        //Debug.Log(debuffTypes + ": " + Mathf.Abs(amount));
        SetSize(Mathf.Abs(amount), _buffDivisor);
        StartMovement();
    }

    private void SetSize(float numerator, float divisor)
    {
        divisor = Mathf.Max(divisor, 1);
        numerator = Mathf.Min(Mathf.Abs(numerator), _maxNumerator);
        Vector3 size = Vector3.one * (1f + numerator / divisor);

        if (_isCrit)
            size *= _critSizeMultiplier;

        transform.localScale = size;
    }

    private void StartMovement()
    {
        // if i have siblings increase my starting pos by 30
        transform.position += new Vector3(0, (transform.parent.childCount - 1) * -70, 0);
        Icon.color = Color.white;

        if (!this.isActiveAndEnabled)
            return;

        StartCoroutine(LerpPos(transform.position, transform.position + new Vector3(0, 200, 0), 4f));
        StartCoroutine(Fade(_initialWait, _fadeDuration, AmountText));
        StartCoroutine(Fade(_initialWait, _fadeDuration, ReductionText));
        StartCoroutine(Fade(_initialWait, _fadeDuration, Icon));

        float bigRatio = _isBuff ? _bigRatioBuff : _bigRatioAttack;

        StartCoroutine(AnimateScale(_initialWait + _fadeDuration, bigRatio));
        StartCoroutine(AnimateScale(_initialWait + _fadeDuration, bigRatio));
        StartCoroutine(AnimateScale(_initialWait + _fadeDuration, bigRatio));
    }
    
    IEnumerator LerpPos(Vector3 start, Vector3 end, float timeToMove)
    {
        float t = 0;
        float originalStartX = start.x;
        float originalEndX = end.x;

        while(t < 1)
        {
            if (_isCrit)
            {
                float shake = GetShake(t);
                Debug.Log(shake);
                start.x = originalStartX + shake;
                end.x = originalEndX + shake;
            }

            transform.position = Vector3.Lerp(start,end,t);
            t = t + Time.deltaTime / timeToMove;
            yield return new WaitForEndOfFrame();
        }

        transform.position = end;        
    }

    private float GetShake(float t)
    {
        return Mathf.Sin(t * Mathf.PI * 2 * _shakeCount) * _shakeWidth;
    }

    private IEnumerator Fade(float initialWait, float fadeDuration, TextMeshProUGUI text)
    {
        yield return new WaitForSeconds(initialWait);
        Color initialColor = text.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

        float elapsedTime = 0f;
        Vector3 startingScale = transform.localScale;

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

    private IEnumerator AnimateScale(float fadeDuration, float bigRatio)
    {
        Vector3 startingScale = transform.localScale; 
        float elapsedTime = 0f;
        float scaleRatio;
        float totalTime;        
        float smallTime;        
        float passedPercentage; 
        float differenceRatio;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime < _bigTime)
            {
                scaleRatio = 1f + (elapsedTime / _bigTime) * bigRatio;
                //Debug.Log("Up: " + scaleRatio);
            }
            else
            {
                totalTime = fadeDuration - _bigTime;
                smallTime = elapsedTime - _bigTime;
                passedPercentage = smallTime / totalTime;
                differenceRatio = 1f + bigRatio - _smallRatio;
                scaleRatio = _smallRatio + differenceRatio * (1f - passedPercentage);
                //Debug.Log("Down: " + scaleRatio+"\n"+
                //    _smallRatio+" + "+differenceRatio+" * (1f - "+passedPercentage+")");
            }

            transform.localScale = scaleRatio * startingScale;

            yield return null;
        }
    }

    private void MoveFinished()
    {
        UIPooler._instance.StatusTextsPool.Add(this.gameObject);
        this.transform.SetParent(UIPooler._instance.transform);
        this.gameObject.SetActive(false);
        _isCrit = false;
    }
}
