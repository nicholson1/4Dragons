using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Image;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIScreenCustomize : UIScreen
{
    [SerializeField] private List<Selectable> cosmeticAllSelectables, characterAllSelectables;

    [SerializeField] private List<Selectable> cosmeticLeftSelectables = new List<Selectable>();
    [SerializeField] private List<Selectable> cosmeticRightSelectables = new List<Selectable>();
    [SerializeField] private List<Selectable> cosmeticLockSelectables = new List<Selectable>();

    [SerializeField] private List<Selectable> characterLeftSelectable = new List<Selectable>();
    [SerializeField] private List<Selectable> characterRightSelectable = new List<Selectable>();

    private Button backButton;
    private bool panningDone = false;


    [ContextMenu("Populate Cosmetics")]
    private void PopulateCosmeticLeftSelectables()
    {
        cosmeticLeftSelectables.Clear();
        cosmeticRightSelectables.Clear();
        cosmeticLockSelectables.Clear();

        cosmeticLeftSelectables = cosmeticAllSelectables.Where(s => s.gameObject.name == "Icon (1)").ToList();
        cosmeticRightSelectables = cosmeticAllSelectables.Where(s => s.gameObject.name == "Icon").ToList();
        cosmeticLockSelectables = cosmeticAllSelectables.Where(s => s.gameObject.name == "Button").ToList();

#if UNITY_EDITOR
        SavePrefabChanges();
#endif

    }

    [ContextMenu("Populate Characters")]
    private void PopulateCharacterSelectables()
    {
        characterLeftSelectable.Clear();
        characterRightSelectable.Clear();

        characterLeftSelectable = characterAllSelectables.Where(s => s.gameObject.name == "Icon (1)").ToList();
        characterRightSelectable = characterAllSelectables.Where(s => s.gameObject.name == "Icon").ToList();

#if UNITY_EDITOR
        SavePrefabChanges();
#endif
    }


    [ContextMenu("Setup Navigation")]
    private void SetupNavigation()
    {

        SetupVerticalNavigation(characterLeftSelectable);
        SetupVerticalNavigation(characterRightSelectable);
        SetupVerticalNavigation(cosmeticLeftSelectables);
        SetupVerticalNavigation(cosmeticRightSelectables);
        SetupVerticalNavigation(cosmeticLockSelectables);

        SetupLeftNavigation(cosmeticRightSelectables, cosmeticLeftSelectables);
        SetupRightNavigation(cosmeticLeftSelectables, cosmeticRightSelectables);
        SetupRightNavigation(cosmeticLockSelectables, cosmeticLeftSelectables);
        SetupLeftNavigation(cosmeticLeftSelectables, cosmeticLockSelectables);

        SetupRightNavigation(characterLeftSelectable, characterRightSelectable);
        SetupLeftNavigation(characterRightSelectable, characterLeftSelectable);

#if UNITY_EDITOR
        SavePrefabChanges();
#endif

    }

    [ContextMenu("TrimEdge")]
    private void TrimEdgeNavigation()
    {

        TrimLeftNavigation(cosmeticLockSelectables);
        TrimRightNavigations(cosmeticRightSelectables);
#if UNITY_EDITOR
        SavePrefabChanges();
#endif
    }

    [ContextMenu("Clear all Navi")]
    private void ClearAllNavi()
    {
        foreach(var s in cosmeticAllSelectables)
        {
            var navi = s.navigation;
            navi.selectOnRight = null;
            navi.selectOnLeft = null;
            navi.selectOnUp = null;
            navi.selectOnDown = null;
            navi.wrapAround = false;

            s.navigation = navi;
        }

        foreach(var s in characterAllSelectables)
        {
            var navi = s.navigation;
            navi.selectOnRight = null;
            navi.selectOnLeft = null;
            navi.selectOnUp = null;
            navi.selectOnDown = null;
            navi.wrapAround = false;

            s.navigation = navi;
        }

#if UNITY_EDITOR
        SavePrefabChanges();
#endif
    }



    private void TrimRightNavigations(List<Selectable> selectables)
    {
        foreach(var s in selectables)
        {
            var navi = s.navigation;
            navi.selectOnRight = null;
            s.navigation = navi;
        }


    }

    private void TrimLeftNavigation(List<Selectable> selectables)
    {
        foreach (var s in selectables)
        {
            var navi = s.navigation;
            navi.selectOnLeft = null;
            s.navigation = navi;
        }
    }

#if UNITY_EDITOR
    private void SavePrefabChanges()
    {
        // If this object is part of a prefab asset or prefab stage.
        GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);

        if (prefabRoot != null)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(prefabRoot);
            EditorUtility.SetDirty(prefabRoot);
        }

        AssetDatabase.SaveAssets();
    }
#endif

    private void SetupRightNavigation(List<Selectable> origin, List<Selectable> target)
    {
        for (int i = 0; i<origin.Count; i++)
        {
            var selectable = origin[i];
            var navi = selectable.navigation;

            navi.selectOnRight = target[i];

            selectable.navigation = navi;
        }
    }

    private void SetupLeftNavigation(List<Selectable> origin, List<Selectable> target)
    {
        for (int i = 0; i < origin.Count; i++)
        {
            var selectable = origin[i];
            var navi = selectable.navigation;

            navi.selectOnLeft = target[i];

            selectable.navigation = navi;
        }
    }

    private void SetupVerticalNavigation(List<Selectable> selectables)
    {
        for (int i = 0; i < selectables.Count; i++)
        {
            var selectable = selectables[i];
            var navi = selectable.navigation;
            navi.mode = Navigation.Mode.Explicit;

            if (i - 1 > 0)
            {
                navi.selectOnUp = selectables[i - 1];
            }
            if (i + 1 < selectables.Count)
            {
                navi.selectOnDown = selectables[i + 1];
            }

            if (i == 0)
            {
                navi.selectOnUp = null;
            }
            else if (i == selectables.Count - 1)
            {
                navi.selectOnDown = null;
            }

            selectable.navigation = navi;
        }
    }

    private IEnumerator ActivationRoutine(Action onRoutineEnd = null)
    {
        while (!panningDone)
        {
            yield return null;
        }

        onRoutineEnd?.Invoke();
    }


}
