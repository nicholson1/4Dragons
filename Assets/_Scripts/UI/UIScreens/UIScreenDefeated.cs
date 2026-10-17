using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScreenDefeated : UIScreen
{
    public GameObject Header => headerPanel;
    public GameObject ButtonsHolder => buttonsHolderPanel;

    [SerializeField] private GameObject headerPanel;
    [SerializeField] private GameObject buttonsHolderPanel;

    [SerializeField] private Button retryCombat;
    [SerializeField] private Button backToMenu;


    public override void Activate(bool navigatableOnActivated = true)
    {
        base.Activate();

        backToMenu.gameObject.SetActive(true);
        backToMenu.interactable = true;

        if (CanRetryCombat())
        {
            retryCombat.gameObject.SetActive(true);
            retryCombat.interactable = true;
            TutorialManager.Instance.QueueTip(TutorialNames.Retry);
            EventSystem.current.SetSelectedGameObject(retryCombat.gameObject);
        }
        else
            EventSystem.current.SetSelectedGameObject(backToMenu.gameObject);

        SetupNavigation();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        DefaultRaycastBlocker.SetActive(false);

        retryCombat.interactable = false;
        retryCombat.gameObject.SetActive(false);

        backToMenu.interactable = false;
        backToMenu.gameObject.SetActive(false);
    }

    private void SetupNavigation()
    {
        var navi = backToMenu.navigation;
        navi.selectOnUp = CanRetryCombat() ? retryCombat : null;
    }

    private bool CanRetryCombat()
    {
        return CombatController._instance.retryAvailable >= 1;
    }
}
