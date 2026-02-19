using Map;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenMap : UIScreen
{
    public bool AreNodesClickable => areNodesClickable;

    private bool areNodesClickable = false;

    [SerializeField] private MapView mapView;
    private Dictionary<MapNode, int> mapNodes = new Dictionary<MapNode, int>();
    private MapNode currenOccupiedNode;

    public void SetNodesClickable(bool clickable) => areNodesClickable = clickable;
    public void SetCurrentOccupiedNode(MapNode mapNode) => currenOccupiedNode = mapNode;


    public override void Activate(bool navigatableOnActivated = true)
    {
        PopulateMapNodes();

        base.Activate(navigatableOnActivated);
    }


    public override Selectable GetSelectableToSelectOnActivated()
    {
        if (mapNodes.Count == 0)
            return base.GetSelectableToSelectOnActivated();

        if (currenOccupiedNode == null)
        {
            var mapNode = mapNodes.Where(n => n.Value == 0).FirstOrDefault().Key;
            Selectable selectable = mapNode.GetComponentInChildren<Selectable>();
            return selectable;
        }            

        return currenOccupiedNode.GetComponentInChildren<Selectable>();
    }

    private void PopulateMapNodes()
    {
        mapNodes.Clear();
        for(int i = 0; i<mapView.MapNodes.Count; i++)
        {
            var mapNode = mapView.MapNodes[i];
            mapNodes.Add(mapNode, mapNode.Node.layer);
            
            var button = mapNode.GamepadButton;
            if(button.TryGetComponent(out ButtonBindingHandler bindHandler))
            {
                bindHandler.SetUIScreen(this);
            }
        }

        SetNodesNavigation();
    }

    private void SetNodesNavigation()
    {
        foreach(var kvp in mapNodes)
        {
            Selectable selectable = kvp.Key.GamepadButton;
            Navigation navi = selectable.navigation;
            if (navi.mode != Navigation.Mode.Explicit)
                navi.mode = Navigation.Mode.Explicit;

            navi.selectOnRight = GetHorizontalClosestNodeButton(kvp.Value, 1, kvp.Key);
            navi.selectOnLeft = GetHorizontalClosestNodeButton(kvp.Value, -1, kvp.Key);

            navi.selectOnUp = GetVerticalNodeButton(kvp.Key, 1);
            navi.selectOnDown = GetVerticalNodeButton(kvp.Key, -1);

            selectable.navigation = navi;
        }
    }

    private Selectable GetHorizontalClosestNodeButton(int currentLayer, int direction, MapNode current)
    {
        var targetLayer = currentLayer + direction;
        if(targetLayer > MapManager._instance.TotalLayer-1 || targetLayer < 0)
        {
            return null;
        }

        //list all the nodes on the next layer
        var candidates = mapNodes.Where(n => n.Value == targetLayer).ToList();

        MapNode firstConnectedCandidate = null;

        //list all the connected nodes on the next layer
        if(direction > 0)
            firstConnectedCandidate = candidates.Where(n => n.Key.Node.incoming.Contains(current.Node.point)).LastOrDefault().Key;
        else if(direction < 0)
            firstConnectedCandidate = candidates.Where(n => n.Key.Node.outgoing.Contains(current.Node.point)).LastOrDefault().Key;

        if (firstConnectedCandidate == null) return null;
        Selectable selectableToChoose = firstConnectedCandidate.GamepadButton;
        return selectableToChoose;
    }


    private Selectable GetVerticalNodeButton(MapNode current, int direction)
    {
        var candidates = mapNodes.Keys.Where(n => mapNodes[n] == current.Node.layer).ToList();

        if (candidates.Count <= 1)
            return null;


        int targetIndex = candidates.IndexOf(current) + direction;

        bool outOfRange = targetIndex >= candidates.Count || targetIndex < 0;

        if (outOfRange) return null;

        return candidates[targetIndex].GamepadButton;
    }

}
