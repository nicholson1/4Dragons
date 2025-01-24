using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private TextMeshProUGUI difficultyText;

    [SerializeField] private Toggle ShowHelm;
    [SerializeField] private Toggle ScreenShake;
    [SerializeField] private Toggle Tutorials;
    [SerializeField] private Toggle Keybinds;

    [SerializeField] private TextMeshProUGUI difficultyTextHUD;


    private void Start()
    {
        ShowHelm.SetIsOnWithoutNotify(PlayerPrefsManager.GetShowHelm() == 1);
        ScreenShake.SetIsOnWithoutNotify(PlayerPrefsManager.getScreenShake() == 1);
        Tutorials.SetIsOnWithoutNotify(PlayerPrefsManager.GetTutorialEnabled() == 1);
        Keybinds.SetIsOnWithoutNotify(PlayerPrefsManager.GetKeyBindEnabled() == 1);


        int diff = PlayerPrefsManager.GetDifficulty();
        difficultySlider.SetValueWithoutNotify(diff);
        OnValueChangeSlider();
        

        this.gameObject.SetActive(false);
    }

    private string[] DifficultyTexts = new[]
    {
        "Beginner",
        "Enemies are Tougher",
        "Potions less common",
        "Enemies drop less gold",
        "You have less max Health",
        "Enemies are Mightier",
        "Increase the chance of elites",
        "Shops are more expensive",
        "Enemies are Relentless",
        "Enemies are Unyielding",
        "Dragons are more difficult"
    };

    public void OnValueChangeSlider()
    {
        int diff =  Mathf.RoundToInt(difficultySlider.value);
        CombatController._instance.Difficulty = diff;

        string Stars = $"{diff} <sprite=\"status_icon_star\" index=0> ";
        // for (int i = 0; i < diff; i++)
        // {
        //     Stars += "<sprite=\"status_icon_star\" index=0>";
        // }
        
        
        difficultyText.text = $"Difficulty: {Stars}\n {DifficultyTexts[diff]}";
        
        PlayerPrefsManager.SetDifficulty(diff);
        difficultyTextHUD.text = diff.ToString();
    }

}
