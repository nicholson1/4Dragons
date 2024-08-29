using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private TextMeshProUGUI difficultyText;

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

        string Stars = "";
        for (int i = 0; i < diff; i++)
        {
            Stars += "<sprite=\"status_icon_star\" index=0>";
        }
        
        
        difficultyText.text = $"Difficulty: {Stars}\n {DifficultyTexts[diff]}";
    }

}
