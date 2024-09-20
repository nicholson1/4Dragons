using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UI.Toggle;

public class Modifiers : MonoBehaviour
{
    public static Modifiers _instance;
    public RectTransform Scroller;
    public ToggleMod TogglePrefab;
    private List<Toggle> toggles = new List<Toggle>();

    private List<Mods> currentMods = new List<Mods>();

    public List<Mods> CurrentMods => currentMods;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        foreach (Mods mods in Enum.GetValues(typeof(Mods)))
        {
            ToggleMod t = Instantiate(TogglePrefab, Scroller);
            t.mod = mods;
            t.Text.text = mods.ToString();
            Toggle tog = t.GetComponent<Toggle>();
            t.toggle = tog;
            toggles.Add(tog);
        }
    }

    public void AdjustMod(Mods mod, bool add)
    {
        if (mod == Mods.None && add)
        {
            currentMods.Clear();

            for (int i = 1; i < toggles.Count; i++)
            {
                toggles[i].isOn = false;
            }
            toggles[0].isOn = true;
            currentMods.Add(mod);
            return;
        }
        if (add)
        {
            Debug.Log("adding " + mod);
            currentMods.Add(mod);
            if (currentMods.Contains(Mods.None))
            {
                currentMods.Remove(Mods.None);
                toggles[0].isOn = false;
            }
        }
        else
        {
            currentMods.Remove(mod);
        }
    }
}

public enum Mods
{
    None = 0,
    NoDaggerSpells = 1,
    NoShieldSpells = 2,
    NoSwordSpells = 3,
    NoAxeSpells = 4,
    NoHammerSpells = 5,
    NoNatureSpells = 6,
    NoFireSpells = 7,
    NoIceSpells = 8,
    NoBloodSpells = 9,
    NoShadowSpells = 10,
    DecreaseDaggerPower = 11,
    DecreaseShieldPower = 12,
    DecreaseSwordPower = 13,
    DecreaseAxePower = 14,
    DecreaseHammerPower = 15,
    DecreaseNaturePower = 16,
    DecreaseFirePower = 17,
    DecreaseIcePower = 18,
    DecreaseBloodPower = 19,
    DecreaseShadowPower = 20,
    IncreaseDaggerPower = 21,
    IncreaseShieldPower = 22,
    IncreaseSwordPower = 23,
    IncreaseAxePower = 24,
    IncreaseHammerPower = 25,
    IncreaseNaturePower = 26,
    IncreaseFirePower = 27,
    IncreaseIcePower = 28,
    IncreaseBloodPower = 29,
    IncreaseShadowPower = 30,
    NoDaggerStats = 31,
    NoShieldStats = 32,
    NoSwordStats = 33,
    NoAxeStats = 34,
    NoHammerStats = 35,
    NoNatureStats = 36,
    NoFireStats = 37,
    NoIceStats = 38,
    NoBloodStats = 39,
    NoShadowStats = 40,
    NoStrengthStats = 41,
    NoSpellPowerStats = 42,
    NoHealthStats = 43,
    NoCritStats = 44,
    NoMRStat = 45,
    NoArmStat = 46,
    NoSpellWeapons = 47,
    NoPhysicalWeapons = 48,
    DoubleGold = 49,
    HalfGold = 50,
    NoShopRerolls = 51,
    NoPotion = 52,
    AlwaysPotion = 53,
    NoNatureDragons = 54,
    NoFireDragons = 55,
    NoIceDragons = 56,
    NoBloodDragons = 57,
    NoShadowDragons = 58,
}
