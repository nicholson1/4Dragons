using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class RelicTester : MonoBehaviour
{

    public TextMeshProUGUI InputField;

    public void Click()
    {
        if (int.TryParse(InputField.text, out int intValue))
        {
            Debug.Log("Integer Value: " + intValue);
            RelicManager._instance.GetRelic(intValue);
        }
        else
        {
            Debug.LogError("Failed to parse string as integer.");
        }
        
    }
}
