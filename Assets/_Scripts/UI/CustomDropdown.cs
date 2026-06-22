using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace DFG.UIHandling
{
    public class CustomDropdown : TMP_Dropdown
    {

        private List<Toggle> dropdownToggles = new List<Toggle>();

        private List<DropdownItem> dropdownItems = new List<DropdownItem>();

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);

        }
    }

}
