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

    private void Start()
    {
        ShowHelm.SetIsOnWithoutNotify(PlayerPrefsManager.GetShowHelm() == 1);
        ScreenShake.SetIsOnWithoutNotify(PlayerPrefsManager.getScreenShake() == 1);

        int diff = PlayerPrefsManager.GetDifficulty();
        difficultySlider.SetValueWithoutNotify(diff);
        OnValueChangeSlider();
        

        this.gameObject.SetActive(false);
    }

    private string[] DifficultyTexts = new[]
    {
        "",
        "Enemies have more health",
        "Reduce the chance you find a potion",
        "Enemies drop less gold",
        "You have less max Health",
        "Enemies do more damage",
        "Increase the chance of elites",
        "Shops are more expensive",
        "Enemies have even more health",
        "Enemies do even more damage",
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
    }

}
