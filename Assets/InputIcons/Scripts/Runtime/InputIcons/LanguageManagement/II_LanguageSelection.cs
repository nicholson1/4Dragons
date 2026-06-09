using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace InputIcons
{
    public class II_LanguageSelection : MonoBehaviour
    {
        private TMP_Dropdown dropdown;

        private readonly string option1 = "English";
        private readonly string option2 = "System Language";

        private void OnEnable()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            if (dropdown != null)
            {
                dropdown.options = new List<TMP_Dropdown.OptionData>
                {
                    new TMP_Dropdown.OptionData(option1),
                    new TMP_Dropdown.OptionData(option2)
                };

                if (InputIconsManagerSO.Instance.textDisplayLanguage == InputIconsManagerSO.TextDisplayLanguage.EnglishOnly)
                    dropdown.value = 0;
                else
                    dropdown.value = 1;
            }

            dropdown.onValueChanged.AddListener(RefreshLanguage);
        }

        private void RefreshLanguage(int dropdownValue)
        {
            if (dropdownValue == 0)
                InputIconsManagerSO.SetDisplayLanguageType(InputIconsManagerSO.TextDisplayLanguage.EnglishOnly);
            if(dropdownValue == 1)
                InputIconsManagerSO.SetDisplayLanguageType(InputIconsManagerSO.TextDisplayLanguage.SystemLanguage);
        }
    }
}

