using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownTogglesListener : MonoBehaviour
{
    [SerializeField]private List<Toggle> dropdownOptionToggles = new List<Toggle>();

    public void RegisterSelf(Toggle toggle)
    {
        if(dropdownOptionToggles.Contains(toggle))
        {
            Debug.LogError($"Error: {toggle.gameObject.name} attempted to register self more than once!");
            return;
        }

        dropdownOptionToggles.Add(toggle);
    }

    public void UnregisterSelf(Toggle toggle)
    {
        if(!dropdownOptionToggles.Contains(toggle))
        {
            Debug.LogError($"Error: {toggle.gameObject.name} attempted to unregister self more than once!");
            return;
        }

        dropdownOptionToggles.Remove(toggle);
    }
}
