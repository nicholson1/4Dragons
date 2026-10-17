using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class NavigationAutomator : MonoBehaviour
{
    [SerializeField] private List<StatDisplay> statDisplays = new List<StatDisplay>();
    [SerializeField] private List<Button> currentActiveButtons = new List<Button>();

    [ContextMenu("AutomateNavigation")]
    public void AutomateNavigation()
    {
        Undo.RecordObject(this, "Automate Navigation");

        currentActiveButtons.Clear();

        for(int i = 0; i<statDisplays.Count; i++)
        {
            var button = statDisplays[i].GetComponentInChildren<Button>();
            currentActiveButtons.Add(button);
        }

        for (int i = 0; i < currentActiveButtons.Count; i++)
        {
            var button = currentActiveButtons[i];
            var navi = button.navigation;
            navi.mode = Navigation.Mode.Explicit;

            int moduloIndex = i % 8;
            navi.selectOnDown = moduloIndex < 7 ? currentActiveButtons[i + 1] : null;
            navi.selectOnUp = i > 0 ?currentActiveButtons[i - 1] : null;
            navi.selectOnRight = (i < 8) ? currentActiveButtons[i + 8] : null;
            navi.selectOnLeft = (i > 7) ? currentActiveButtons[i - 8] : null;
            if (i == 7 || i == 15)
                navi.selectOnDown = null;
            if (i == 0 || i == 8)
                navi.selectOnUp = null;

            button.navigation = navi;
        }

        EditorUtility.SetDirty(this);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);

    }
}

