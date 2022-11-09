using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TreePoolerBasic : MonoBehaviour
{
    [SerializeField] private Transform PlayerPos;
    public GameObject TreePrefab;

     public int TreeCountMin;
    public int PlacementRadiusMax;
    public int PlacementRadiusMin;
    public Slider slider;

    private List<GameObject> HiddenTrees = new List<GameObject>();
    private List<GameObject> ActiveTrees = new List<GameObject>();

    public float timeBetweenchecks;
    

    private void HideTree(GameObject tree)
    {
        ActiveTrees.Remove(tree);
        HiddenTrees.Add(tree);
        tree.SetActive(false);
  
    }

    private void ActivateTree(GameObject tree)
    {
        HiddenTrees.Remove(tree);
        ActiveTrees.Add(tree);
        tree.SetActive(true);

    }

    private void PlaceRandomTree()
    {
        //get random pos inside max rad, but not in inner rad;
        Vector2 point = Random.insideUnitCircle.normalized * Random.Range(PlacementRadiusMin, PlacementRadiusMax);
        Vector3 pos = new Vector3(point.x, -.25f, point.y) + new Vector3(PlayerPos.position.x, 0, PlayerPos.position.z);

        GameObject t;
        if (HiddenTrees.Count > 0)
        {
            t = HiddenTrees[0];
            ActivateTree(t);
            t.transform.position = pos;
        }
        else
        {
            t = Instantiate(TreePrefab, pos, TreePrefab.transform.rotation);
            ActiveTrees.Add(t);
            t.transform.SetParent(this.transform);
        }
        
        t.GetComponent<TreeManger>().RandomizeTree();

        
    }

    private void PlaceTreeInitial()
    {
        for(int i = 0; i < TreeCountMin; i++)
        {
            
            Vector2 point = Random.insideUnitCircle.normalized * Random.Range(5, PlacementRadiusMax);;


            Vector3 pos = new Vector3(point.x, -.25f, point.y) + new Vector3(PlayerPos.position.x, 0, PlayerPos.position.z);
            //Vector3 pos = new Vector3(Random.Range(PlayerPos.position.x - PlacementRadiusMax -1, PlayerPos.position.x + PlacementRadiusMax -1), 0, Random.Range(PlayerPos.position.z - PlacementRadiusMax -1, PlayerPos.position.z + PlacementRadiusMax -1));
            GameObject t = Instantiate(TreePrefab, pos, TreePrefab.transform.rotation);
            t.transform.SetParent(this.transform);
            t.GetComponent<TreeManger>().RandomizeTree();

            ActiveTrees.Add(t);
        }
    }

    private void Start()
    {
        PlaceTreeInitial();
        slider.value = TreeCountMin;
    }

    private float timer = 0;

    private void Update()
    {
        // timer += Time.deltaTime;
        //
        // if(timer > timeBetweenchecks)
        // {
        //Debug.Log("Active: "+ActiveTrees.Count + " inactive: " + HiddenTrees.Count);
        
        for(int i = 0; i < ActiveTrees.Count; i++ )
        {
            GameObject tree = ActiveTrees[i];
            Vector3 temp = tree.transform.position - PlayerPos.position;
            temp.y = 0;
            
            if (temp.magnitude > PlacementRadiusMax + 10)
            {
                //Debug.Log(Vector3.Distance(tree.transform.position, PlayerPos.position));
                HideTree(tree);
                i--;
                
                //PlaceRandomTree();
            }
        }

        if (ActiveTrees.Count < slider.value)
        {
            PlaceRandomTree();
        }

        if (ActiveTrees.Count > slider.value)
        {
            CullTrees();
        }
            
        
        
        



        
    }

    public void CullTrees()
    {
        while (ActiveTrees.Count > slider.value)
        {
            HideTree(ActiveTrees[^1]);
        }
    }
    public Vector2 RandomPointInAnnulus(Vector2 origin, float minRadius, float maxRadius){
 
        var randomDirection = (Random.insideUnitCircle * origin).normalized;
 
        var randomDistance = Random.Range(minRadius, maxRadius);
 
        var point = origin + randomDirection * randomDistance;
 
        return point;
    }
}
