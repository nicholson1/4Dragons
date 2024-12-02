using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;

public class TutorialDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject BackgroundGlow;
    public GameObject textBox;
    public TextMeshProUGUI Text;
    [SerializeField] private TutorialNames _tutorialID;

    void Awake()
    {
        TutorialManager.TriggerTutorial += ShowTutorial;
    }

    public void CloseTip()
    {
        BackgroundGlow.SetActive(false);
        textBox.SetActive(false);
        TutorialManager.Instance.ShowTip();
    }

    public void ShowTutorial(TutorialNames id)
    {
        if(id != _tutorialID)
            return;
        
        BackgroundGlow.gameObject.SetActive(true);
        textBox.gameObject.SetActive(true);
        Text.text = TutorialManager.Instance.GetText(id);
    }

    private void OnDestroy()
    {
        TutorialManager.TriggerTutorial -= ShowTutorial;

    }
}
