using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI Text;

    public void InitializeError(string text, Color c)
    {
        Text.text = text;
        Text.color = c;
        StartCoroutine(Fade(1.5f, .5f, Text));
        
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
        UIPooler._instance.NotificationMessages.Add(this.gameObject);
        this.gameObject.transform.SetParent(UIPooler._instance.transform);
        this.gameObject.SetActive(false);
    }
    
}
