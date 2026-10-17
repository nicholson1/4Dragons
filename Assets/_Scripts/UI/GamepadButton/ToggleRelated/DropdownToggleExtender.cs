using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownToggleExtender : MonoBehaviour
{
    private DropdownTogglesListener dropdownToggleListener = null;
    private Toggle toggle = null;
    private ToggleBindingHandler toggleBindingHandler = null;

    private void Start()
    {
        toggle = GetComponent<Toggle>();     
        
        dropdownToggleListener = GetComponentInParent<DropdownTogglesListener>();
        dropdownToggleListener?.RegisterSelf(toggle);    
        
        if(!TryGetComponent(out toggleBindingHandler))
        {
            toggleBindingHandler = gameObject.AddComponent<ToggleBindingHandler>();
            toggleBindingHandler.ManualBindInput(true);
        }
    }

    private void OnDestroy()
    {
        if(toggleBindingHandler != null)
            toggleBindingHandler.ManualBindInput(false);

        dropdownToggleListener?.UnregisterSelf(toggle);
    }
}
