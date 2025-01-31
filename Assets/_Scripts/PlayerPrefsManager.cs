using System;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    // Keys for PlayerPrefs
    private const string MUSIC_VOL_KEY = "MusicVol";
    private const string SFX_VOL_KEY = "SFXVol";
    private const string AMBIANCE_VOL_KEY = "AmbianceVol";
    private const string DIFFICULTY_KEY = "Difficulty";
    private const string FACE_KEY = "Face";
    private const string HAIR_KEY = "Hair";

    private const string HELMET_KEY = "Helmet";
    private const string HELMET_LOCK_KEY = "HelmetLock";
    private const string SHOULDER_KEY = "Shoulder";
    private const string SHOULDER_LOCK_KEY = "ShoulderLock";
    private const string CHEST_KEY = "Chest";
    private const string CHEST_LOCK_KEY = "ChestLock";
    private const string GLOVES_KEY = "Gloves";
    private const string GLOVES_LOCK_KEY = "GlovesLock";
    private const string BOOTS_KEY = "Boots";
    private const string BOOTS_LOCK_KEY = "BootsLock";
    private const string WEAPON1_KEY = "Weapon1";
    private const string WEAPON1_LOCK_KEY = "Weapon1Lock";
    private const string WEAPON2_KEY = "Weapon2";
    private const string WEAPON2_LOCK_KEY = "Weapon2Lock";
    private const string SHOW_HELM_KEY = "ShowHelm";
    private const string SCREEN_SHAKE_KEY = "ScreenShake";
    private const string TUTORIAL_ENABLED_KEY = "TutorialEnabled";
    private const string KEYBINDS_ENABLED_KEY = "KeybindsEnabled";

    

    // Default values
    private const float DEFAULT_MUSIC_VOL = .25f;
    private const float DEFAULT_SFX_VOL = .25f;
    private const float DEFAULT_AMBIANCE_VOL = .25f;
    private const int DEFAULT_DIFFICULTY = 0;
    private const int DEFAULT_FACE = 5;
    private const int DEFAULT_HAIR = 7;
    private const int DEFAULT_SHOW_HELM = 1;
    private const int DEFAULT_SCREEN_SHAKE = 1;
    
    private const int DEFAULT_TUTORIAL_ENABLED = 1;
    private const int DEFAULT_KEYBINDS_ENABLED = 1;



    private const int DEFAULT_HELMET = 0;
    private const int DEFAULT_HELMET_LOCK = 0;
    private const int DEFAULT_SHOULDER = 7;
    private const int DEFAULT_SHOULDER_LOCK = 0;
    private const int DEFAULT_CHEST = 9;
    private const int DEFAULT_CHEST_LOCK = 0;
    private const int DEFAULT_GLOVES = 7;
    private const int DEFAULT_GLOVES_LOCK = 0;
    private const int DEFAULT_BOOTS = 6;
    private const int DEFAULT_BOOTS_LOCK = 0;
    private const int DEFAULT_WEAPON1 = 0;
    private const int DEFAULT_WEAPON1_LOCK = 0;
    private const int DEFAULT_WEAPON2 = 0;
    private const int DEFAULT_WEAPON2_LOCK = 0;
    

    // Setters
    public static void SetMusicVol(float value) => PlayerPrefs.SetFloat(MUSIC_VOL_KEY, value);
    public static void SetSFXVol(float value) => PlayerPrefs.SetFloat(SFX_VOL_KEY, value);
    public static void SetAmbianceVol(float value) => PlayerPrefs.SetFloat(AMBIANCE_VOL_KEY, value);
    public static void SetDifficulty(int value) => PlayerPrefs.SetInt(DIFFICULTY_KEY, value);
    public static void SetFace(int value) => PlayerPrefs.SetInt(FACE_KEY, value);
    public static void SetHair(int value) => PlayerPrefs.SetInt(HAIR_KEY, value);

    public static void SetHelmet(int value) => PlayerPrefs.SetInt(HELMET_KEY, value);
    public static void SetHelmetLock(int value) => PlayerPrefs.SetInt(HELMET_LOCK_KEY, value);
    public static void SetShoulder(int value) => PlayerPrefs.SetInt(SHOULDER_KEY, value);
    public static void SetShoulderLock(int value) => PlayerPrefs.SetInt(SHOULDER_LOCK_KEY, value);
    public static void SetChest(int value) => PlayerPrefs.SetInt(CHEST_KEY, value);
    public static void SetChestLock(int value) => PlayerPrefs.SetInt(CHEST_LOCK_KEY, value);
    public static void SetGloves(int value) => PlayerPrefs.SetInt(GLOVES_KEY, value);
    public static void SetGlovesLock(int value) => PlayerPrefs.SetInt(GLOVES_LOCK_KEY, value);
    public static void SetBoots(int value) => PlayerPrefs.SetInt(BOOTS_KEY, value);
    public static void SetBootsLock(int value) => PlayerPrefs.SetInt(BOOTS_LOCK_KEY, value);
    public static void SetWeapon1(int value) => PlayerPrefs.SetInt(WEAPON1_KEY, value);
    public static void SetWeapon1Lock(int value) => PlayerPrefs.SetInt(WEAPON1_LOCK_KEY, value);
    public static void SetWeapon2(int value) => PlayerPrefs.SetInt(WEAPON2_KEY, value);
    public static void SetWeapon2Lock(int value) => PlayerPrefs.SetInt(WEAPON2_LOCK_KEY, value);
    public static void SetShowHelm(int value) => PlayerPrefs.SetInt(SHOW_HELM_KEY, value);
    public static void SetScreenShake(int value) => PlayerPrefs.SetInt(SCREEN_SHAKE_KEY, value);
    public static void SetTutorialEnabled(int value) => PlayerPrefs.SetInt(TUTORIAL_ENABLED_KEY, value);
    public static void SetKeyBindEnabled(int value) => PlayerPrefs.SetInt(KEYBINDS_ENABLED_KEY, value);

    

    // Getters
    public static float GetMusicVol() => PlayerPrefs.GetFloat(MUSIC_VOL_KEY, DEFAULT_MUSIC_VOL);
    public static float GetSFXVol() => PlayerPrefs.GetFloat(SFX_VOL_KEY, DEFAULT_SFX_VOL);
    public static float GetAmbianceVol() => PlayerPrefs.GetFloat(AMBIANCE_VOL_KEY, DEFAULT_AMBIANCE_VOL);
    public static int GetDifficulty() => PlayerPrefs.GetInt(DIFFICULTY_KEY, DEFAULT_DIFFICULTY);
    public static int GetFace() => PlayerPrefs.GetInt(FACE_KEY, DEFAULT_FACE);
    public static int GetHair() => PlayerPrefs.GetInt(HAIR_KEY, DEFAULT_HAIR);

    public static int GetHelmet() => PlayerPrefs.GetInt(HELMET_KEY, DEFAULT_HELMET);
    public static int GetHelmetLock() => PlayerPrefs.GetInt(HELMET_LOCK_KEY, DEFAULT_HELMET_LOCK);
    public static int GetShoulder() => PlayerPrefs.GetInt(SHOULDER_KEY, DEFAULT_SHOULDER);
    public static int GetShoulderLock() => PlayerPrefs.GetInt(SHOULDER_LOCK_KEY, DEFAULT_SHOULDER_LOCK);
    public static int GetChest() => PlayerPrefs.GetInt(CHEST_KEY, DEFAULT_CHEST);
    public static int GetChestLock() => PlayerPrefs.GetInt(CHEST_LOCK_KEY, DEFAULT_CHEST_LOCK);
    public static int GetGloves() => PlayerPrefs.GetInt(GLOVES_KEY, DEFAULT_GLOVES);
    public static int GetGlovesLock() => PlayerPrefs.GetInt(GLOVES_LOCK_KEY, DEFAULT_GLOVES_LOCK);
    public static int GetBoots() => PlayerPrefs.GetInt(BOOTS_KEY, DEFAULT_BOOTS);
    public static int GetBootsLock() => PlayerPrefs.GetInt(BOOTS_LOCK_KEY, DEFAULT_BOOTS_LOCK);
    public static int GetWeapon1() => PlayerPrefs.GetInt(WEAPON1_KEY, DEFAULT_WEAPON1);
    public static int GetWeapon1Lock() => PlayerPrefs.GetInt(WEAPON1_LOCK_KEY, DEFAULT_WEAPON1_LOCK);
    public static int GetWeapon2() => PlayerPrefs.GetInt(WEAPON2_KEY, DEFAULT_WEAPON2);
    public static int GetWeapon2Lock() => PlayerPrefs.GetInt(WEAPON2_LOCK_KEY, DEFAULT_WEAPON2_LOCK);
    public static int GetShowHelm() => PlayerPrefs.GetInt(SHOW_HELM_KEY, DEFAULT_SHOW_HELM);
    public static int getScreenShake() => PlayerPrefs.GetInt(SCREEN_SHAKE_KEY, DEFAULT_SCREEN_SHAKE);
    public static int GetTutorialEnabled() => PlayerPrefs.GetInt(TUTORIAL_ENABLED_KEY, DEFAULT_TUTORIAL_ENABLED);
    public static int GetKeyBindEnabled() => PlayerPrefs.GetInt(KEYBINDS_ENABLED_KEY, DEFAULT_KEYBINDS_ENABLED);

    // Utility Functions
    public static void SavePreferences() => PlayerPrefs.Save();

    public static void ResetToDefaults()
    {
        SetMusicVol(DEFAULT_MUSIC_VOL);
        SetSFXVol(DEFAULT_SFX_VOL);
        SetAmbianceVol(DEFAULT_AMBIANCE_VOL);
        SetDifficulty(DEFAULT_DIFFICULTY);
        SetFace(DEFAULT_FACE);
        SetHair(DEFAULT_HAIR);

        SetHelmet(DEFAULT_HELMET);
        SetHelmetLock(DEFAULT_HELMET_LOCK);
        SetShoulder(DEFAULT_SHOULDER);
        SetShoulderLock(DEFAULT_SHOULDER_LOCK);
        SetChest(DEFAULT_CHEST);
        SetChestLock(DEFAULT_CHEST_LOCK);
        SetGloves(DEFAULT_GLOVES);
        SetGlovesLock(DEFAULT_GLOVES_LOCK);
        SetBoots(DEFAULT_BOOTS);
        SetBootsLock(DEFAULT_BOOTS_LOCK);
        SetWeapon1(DEFAULT_WEAPON1);
        SetWeapon1Lock(DEFAULT_WEAPON1_LOCK);
        SetWeapon2(DEFAULT_WEAPON2);
        SetWeapon2Lock(DEFAULT_WEAPON2_LOCK);
        SetShowHelm(DEFAULT_SHOW_HELM);
        SetScreenShake(DEFAULT_SCREEN_SHAKE);
        SetTutorialEnabled(DEFAULT_TUTORIAL_ENABLED);
        SetKeyBindEnabled(DEFAULT_KEYBINDS_ENABLED);
        
        SavePreferences();
    }
}