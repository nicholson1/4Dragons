using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject BackgroundGlow;
    public GameObject textBox;
    public TextMeshProUGUI Text;
    [SerializeField] private TutorialNames _tutorialID;
    public static event Action<TutorialNames> CloseAll;

    [SerializeField] private CanvasGroup _canvasGroup;

    [SerializeField] private RectTransform _rectTransform;
    
    void Awake()
    {
        TutorialManager.TriggerTutorial += ShowTutorial;
        TutorialDisplay.CloseAll += CloseAllTutorial;
        TutorialManager.CloseTutorial += CloseOverride;
    }

    private void LateUpdate()
    {
        if (_rectTransform.rotation != Quaternion.identity)
        {
            _rectTransform.rotation = Quaternion.identity;
        }
    }

    public void CloseTip()
    {
        BackgroundGlow.SetActive(false);
        textBox.SetActive(false);
        TutorialManager.Instance.showingTip = false;
        TutorialManager.Instance.ShowTip();
        


        if (_tutorialID == TutorialNames.Abilities || _tutorialID == TutorialNames.EquipmentRarity || _tutorialID == TutorialNames.Stats)
        {
            CloseAll(_tutorialID);
        }
        //PlayUIClick();

    }

    private void OnDisable()
    {
        CloseTip();
    }

    public void CloseAllTutorial(TutorialNames id)
    {
        if(id != _tutorialID)
            return;
        
        BackgroundGlow.SetActive(false);
        textBox.SetActive(false);
    }

    public void ShowTutorial(TutorialNames id)
    {
        if(id != _tutorialID)
            return;

        if (gameObject.activeInHierarchy == false)
        {
            return;
        }
        
        TutorialManager.Instance.MoveSprite(id);

        BackgroundGlow.gameObject.SetActive(true);

        if(!TutorialManager.Instance.showingTip || id == TutorialNames.SkipSelection)
        {
            textBox.gameObject.SetActive(true);
            Text.text = TutorialManager.Instance.GetText(id);
        }
        TutorialManager.Instance.showingTip = true;

        TutorialManager.Instance.CheckAndSaveTooltipShown(id);


        /*if(gameObject.activeInHierarchy)
        {
        */
            
        StartCoroutine(FadeCanvasGroup(_canvasGroup, 1, 1));
        //}

    }

    public void CloseOverride(TutorialNames id)
    {
        if(id != _tutorialID)
            return;
        CloseTip();
    }

    private void OnDestroy()
    {
        TutorialManager.TriggerTutorial -= ShowTutorial;
        TutorialManager.CloseTutorial -= CloseOverride;

        TutorialDisplay.CloseAll -= CloseAllTutorial;


    }
    
    public IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        // Check if the CanvasGroup is valid
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null.");
            yield break;
        }

        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(true);

        // Store the initial alpha value
        float startAlpha = canvasGroup.alpha;

        // Track the time elapsed
        float elapsedTime = 0f;
        
        

        // Gradually change the alpha value
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        // Set the final alpha value to ensure it reaches the target
        canvasGroup.alpha = targetAlpha;
        if(targetAlpha == 0)
            canvasGroup.gameObject.SetActive(false);

        if (targetAlpha >= 1)
        {
            //yield return new WaitForSeconds(.25f);
            TutorialManager.Instance.DequeueTipOnSuccess(_tutorialID);

        }

    }
    public AudioClip _buttonClickSFX;
    [SerializeField] private float clickVol = .25f;
    public void PlayUIClick()
    {
        SoundManager.Instance.Play2DSFX(_buttonClickSFX, clickVol, 1, .05f);
    }
}
