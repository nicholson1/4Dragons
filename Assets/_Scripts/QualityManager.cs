using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QualityManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown  qualityDropdown;
    //public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    //public TMP_Dropdown textureQualityDropdown;
    //public Slider lodBiasSlider;
    //public Toggle shadowsToggle;
    //public TMP_Dropdown antiAliasingDropdown;

    private Resolution[] resolutions;

    void Start()
    {
        // Populate the resolution dropdown
        resolutions = Screen.resolutions;
        // resolutionDropdown.ClearOptions();
        //
        // foreach (Resolution resolution in resolutions)
        // {
        //     resolutionDropdown.options.Add(new Dropdown.OptionData(resolution.width + "x" + resolution.height));
        // }
        //
        // resolutionDropdown.RefreshShownValue();
        //
        // // Load saved settings
        LoadSettings();

        // Add listeners to UI elements
        qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
        // resolutionDropdown.onValueChanged.AddListener(SetResolution);
        // fullscreenToggle.onValueChanged.AddListener(SetFullScreen);
        // textureQualityDropdown.onValueChanged.AddListener(SetTextureQuality);
        // lodBiasSlider.onValueChanged.AddListener(SetLODBias);
        //shadowsToggle.onValueChanged.AddListener(SetShadowQuality);
        // antiAliasingDropdown.onValueChanged.AddListener(SetAntiAliasing);
        //SetQualityLevel(0);
    }

    //private float timer = 10;
    //private int level = 0;
    // private void Update()
    // {
    //     timer -= Time.deltaTime;
    //
    //     if (timer < 0)
    //     {
    //         timer = 5;
    //         SetQualityLevel(level);
    //         level += 1;
    //     }
    // }

    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
        SaveSettings();
    }

    public void SetResolution(int index)
    {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, fullscreenToggle.isOn);
        //SaveSettings();
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        //SaveSettings();
    }

    public void SetTextureQuality(int level)
    {
        QualitySettings.globalTextureMipmapLimit = level;
        //SaveSettings();
    }

    public void SetLODBias(float bias)
    {
        QualitySettings.lodBias = bias;
        //SaveSettings();
    }

    public void SetShadowQuality(bool enableShadows)
    {
        Debug.Log("shadows: " + enableShadows);
        QualitySettings.shadows = enableShadows ? ShadowQuality.All : ShadowQuality.Disable;
        SaveSettings();
    }

    public void SetAntiAliasing(int level)
    {
        QualitySettings.antiAliasing = level == 0 ? 0 : (int)Mathf.Pow(2, level); // Dropdown options: 0=Off, 1=2x, 2=4x, 3=8x
        //SaveSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
        //PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        //PlayerPrefs.SetInt("FullScreen", fullscreenToggle.isOn ? 1 : 0);
        //PlayerPrefs.SetInt("TextureQuality", QualitySettings.globalTextureMipmapLimit);
        //PlayerPrefs.SetFloat("LODBias", QualitySettings.lodBias);
        //PlayerPrefs.SetInt("Shadows", QualitySettings.shadows == ShadowQuality.All ? 1 : 0);
        //PlayerPrefs.SetInt("AntiAliasing", antiAliasingDropdown.value);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        // Load quality level
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", 2); // Default to Medium
        QualitySettings.SetQualityLevel(qualityLevel);
        qualityDropdown.value = qualityLevel;

        // Load resolution
        // int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        // resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
        // Resolution resolution = resolutions[resolutionIndex];
        // Screen.SetResolution(resolution.width, resolution.height, PlayerPrefs.GetInt("FullScreen", 1) == 1);
        // resolutionDropdown.value = resolutionIndex;

        // Load fullscreen
        // fullscreenToggle.isOn = PlayerPrefs.GetInt("FullScreen", 1) == 1;

        // Load texture quality
        // int textureQuality = PlayerPrefs.GetInt("TextureQuality", 0); // Default Full Res
        // QualitySettings.globalTextureMipmapLimit = textureQuality;
        // textureQualityDropdown.value = textureQuality;

        // Load LOD bias
        // float lodBias = PlayerPrefs.GetFloat("LODBias", 1.0f); // Default 1.0
        // QualitySettings.lodBias = lodBias;
        // lodBiasSlider.value = lodBias;

        // Load shadow quality
        // bool enableShadows = PlayerPrefs.GetInt("Shadows", 1) == 1;
        // QualitySettings.shadows = enableShadows ? ShadowQuality.All : ShadowQuality.Disable;
        // shadowsToggle.isOn = enableShadows;

        // Load anti-aliasing
        // int antiAliasing = PlayerPrefs.GetInt("AntiAliasing", 1); // Default 2x
        // QualitySettings.antiAliasing = antiAliasing == 0 ? 0 : (int)Mathf.Pow(2, antiAliasing);
        // antiAliasingDropdown.value = antiAliasing;
    }
}
