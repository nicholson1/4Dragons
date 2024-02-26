using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicTester : MonoBehaviour
{

    public Slider InputField;

    private void Update()
    {
        this.GetComponentInChildren<TextMeshProUGUI>().text = "relic : " + (int)InputField.value;
    }

    public void Click()
    {
        RelicManager._instance.GetRelic((int)InputField.value);

        // if (int.TryParse(InputField.text, out int intValue))
        // {
        //     Debug.Log("Integer Value: " + intValue);
        //     RelicManager._instance.GetRelic(intValue);
        // }
        // else
        // {
        //     Debug.LogError("Failed to parse string as integer.");
        // }

    }
}
