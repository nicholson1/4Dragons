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
    
    //have colors for each abilitytypes
    [SerializeField] private Color[] abilityColors;


    public void InitializeStatusText(int amount, CombatEntity.AbilityTypes abilityTypes, int reduction = 0)
    {
        switch (abilityTypes)
        {
            case CombatEntity.AbilityTypes.PhysicalAttack:
                AmountText.text = amount.ToString();
                AmountText.color = abilityColors[abilityTypes.GetHashCode()];
                //Set Icon to attack damage icon
                ReductionText.text = "(" + reduction + ")";
                ReductionText.color = Color.gray;
                break;
            case CombatEntity.AbilityTypes.SpellAttack:
                AmountText.text = amount.ToString();
                AmountText.color = abilityColors[abilityTypes.GetHashCode()];
                //Set Icon to spell damage icon
                
                ReductionText.text = "(" + reduction + ")";
                ReductionText.color = Color.gray;
                break;
                
            case CombatEntity.AbilityTypes.Heal:
                AmountText.text = amount.ToString();
                AmountText.color = abilityColors[abilityTypes.GetHashCode()];
                ReductionText.text = "";
                
                break;
        }
    }

    private void Start()
    {
        StartCoroutine(LerpPos(transform.position, transform.position + new Vector3(0, 100, 0), 2f));
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
        //todo object pooler
        Destroy(this.gameObject);
    }
}
