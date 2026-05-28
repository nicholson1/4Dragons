using DFG.UIHandling;
using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zak.UISystem;

public class PotionHolder : ButtonListener, IDragListener
{
    public PotionDrag Potion => potionDrag;
    public bool IsOccupied => potionDrag != null;

    private PotionDrag potionDrag = null;
    private UIScreenCombat combatScreen = null;
    private UIStateMonitor stateMonitor = null;

    private RectTransform rt => transform as RectTransform;
    private Vector2 gamepadDragOffset = new Vector2(-50f, 50f);
    private Coroutine draggingRoutine = null;

    private InputHandler inputHandler = null;

    public void SetupPotion(PotionDrag potion)
    {
        potionDrag = potion;
    }

    public void UsePotion(CombatEntity target)
    {
        potionDrag.UsePotion(target);
        EquipmentManager._instance.PoolPotion(potionDrag);
        RemovePotion(potionDrag);
        UIController._instance.PlayPotionSound();
        //SoundManager.Instance.Play2DSFX(usePotion, usePotionVol, 1, .05f);
    }

    public void StorePotion(PotionDrag potion)
    {
        

        potionDrag = potion;
        potionDrag.currentLocation = this;
        potionDrag.RT.position = rt.position;
    }

    public void RemovePotion(PotionDrag potion)
    {
        combatScreen.RemoveActivePotion(potionDrag);
        potionDrag = null;
    }

    public bool CanBeginDrag(out IDraggablePayload payload)
    {
        payload = null;

        if (potionDrag == null) return false;

        payload = potionDrag as IDraggablePayload;

        if (payload == null) return false;

        stateMonitor.SetItemOnGamepad(payload);
        combatScreen.SetMousePotionTargettingMode(true, potionDrag);
        return true;

    }

    public void OnDragCompleted(DragResult result)
    {
        stateMonitor.ClearItemOnGamepad();

        var droppedItem = result.Payload as PotionDrag;

        if(!result.Success)
        {
            CancelDragPotion();
            return;
        }

        var destination = (result.DropDestination as TargettingButtonListener);
        var entity = destination.GetComponentInParent<HealthBar>().displayCharacter._combatEntity;

        UsePotion(destination.Entity);
        combatScreen.SetMousePotionTargettingMode(false);

    }

    public void OnHandleInterruption()
    {
        if (stateMonitor.GetUINavigationMode() == NavigationMode.ItemDrag &&
            combatScreen.CurrentCombatNavigation != CombatUINavigationMode.Targetting)
        {
            OnCancelPerformed();
        }
    }

    private void OnCancelPerformed()
    {
        inputHandler.OnNo.RemoveListener(OnCancelPerformed);

        if (stateMonitor.GetUINavigationMode() != NavigationMode.ItemDrag) return;

        if (!stateMonitor.TryGetItemOnGamepad(out var item)) return;

        var result = new DragResult(false, item, null);

        OnDragCompleted(result);

    }

    private void CancelDragPotion()
    {
        potionDrag.RT.position = rt.position;
        combatScreen.SetMousePotionTargettingMode(false);
    }

    private void Awake()
    {
        combatScreen = GetComponentInParent<UIScreenCombat>();

        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        stateMonitor = UIController._instance.StateMonitor;
    }

    private void StartGamepadDrag(IDraggablePayload itemToDrag)
    {
        if (draggingRoutine != null)
        {
            StopCoroutine(draggingRoutine);
            draggingRoutine = null;
        }

        stateMonitor.SetItemOnGamepad(itemToDrag);
        inputHandler.OnNo.AddListener(OnCancelPerformed);
        combatScreen.InitiateGamepadPotionTargetting(Potion);

        draggingRoutine = StartCoroutine(GamepadDragRoutine(itemToDrag));
    }

    public void EndGamepadDragRoutine()
    {
        if (draggingRoutine != null)
        {
            StopCoroutine(draggingRoutine);
            draggingRoutine = null;
        }
    }

    private IEnumerator GamepadDragRoutine(IDraggablePayload payload)
    {
        var potionToDrag = payload as PotionDrag;
        //potionToDrag.RT.anchoredPosition += gamepadDragOffset;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        //RectTransform currentRectTransform = currentSelected.transform as RectTransform;
        while(stateMonitor.GetUINavigationMode() == NavigationMode.ItemDrag)
        {
            if (currentSelected != EventSystem.current.currentSelectedGameObject)
            {
                currentSelected = EventSystem.current.currentSelectedGameObject;

                potionToDrag.transform.position = currentSelected.transform.position;
                potionToDrag.RT.anchoredPosition += gamepadDragOffset;
            }

            yield return null;
        }

        EndGamepadDragRoutine();
    }

    public override void OnButtonSelected(Selectable selectable)
    {
        if (!IsOccupied)
            return;

        Potion.HandleGamepadButtonSelected(selectable);
    }

    public override void OnButtonDeselected(Selectable selectable)
    {
        if (!IsOccupied)
            return;

        Potion.HandleGamepadButtonDeselected(selectable);
    }

    public override void OnButtonPressed(Selectable selectable, InputSource source)
    {
        if (source == InputSource.MouseKeyboard) return;

        //Start selection here
        Debug.LogError($"Potion button pressed!");
        StartGamepadDrag(Potion);
    }


}
