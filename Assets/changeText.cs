using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class changeText : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;

    public void ChangeText()
    {
        text.text = "Trees: " + slider.value;
    }
}
