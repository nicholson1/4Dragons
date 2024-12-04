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
    
    void Awake()
    {
        TutorialManager.TriggerTutorial += ShowTutorial;
        TutorialDisplay.CloseAll += CloseAllTutorial;

    }

    public void CloseTip()
    {
        BackgroundGlow.SetActive(false);
        textBox.SetActive(false);
        TutorialManager.Instance.showingTip = false;
        TutorialManager.Instance.ShowTip();

        if (_tutorialID == TutorialNames.Abilities || _tutorialID == TutorialNames.EquipmentRarity)
        {
            CloseAll(_tutorialID);
        }
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
        

        BackgroundGlow.gameObject.SetActive(true);

        if(!TutorialManager.Instance.showingTip)
        {
            textBox.gameObject.SetActive(true);
            Text.text = TutorialManager.Instance.GetText(id);
        }
        TutorialManager.Instance.showingTip = true;

    }

    private void OnDestroy()
    {
        TutorialManager.TriggerTutorial -= ShowTutorial;
        TutorialDisplay.CloseAll -= CloseAllTutorial;


    }
}
