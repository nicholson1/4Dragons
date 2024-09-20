using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleMod : MonoBehaviour
{
    public Text Text;
    public Mods mod;
    public Toggle toggle;

    public void ValueChanged()
    {
        Modifiers._instance.AdjustMod(mod, toggle.isOn);
    }
}
