using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeControl : MonoBehaviour
{
    [SerializeField] private Transform PlayerPos;
    public GameObject TreePrefab;

    public int TreeCountMax;
    public int PlacementRadiusMax;
    public int PlacementRadiusMin;

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
        Vector3 pos = PlayerPos.position;
        while (Vector3.Distance(pos, PlayerPos.position) < PlacementRadiusMin)
        {
            pos = new Vector3(Random.Range(PlayerPos.position.x - PlacementRadiusMax, PlayerPos.position.x + PlacementRadiusMax), 0, Random.Range(PlayerPos.position.z - PlacementRadiusMax, PlayerPos.position.z + PlacementRadiusMax));

        }

        GameObject t = Instantiate(TreePrefab, pos, TreePrefab.transform.rotation);
        t.transform.SetParent(this.transform);
        t.GetComponent<TreeManger>().RandomizeTree();

    }
    

    private void PlaceTreeInitial()
    {
        for(int i = 0; i < TreeCountMax; i++)
        {
            Vector3 pos = new Vector3(Random.Range(PlayerPos.position.x - PlacementRadiusMax, PlayerPos.position.x + PlacementRadiusMax), 0, Random.Range(PlayerPos.position.z - PlacementRadiusMax, PlayerPos.position.z + PlacementRadiusMax));
            GameObject t = Instantiate(TreePrefab, pos, TreePrefab.transform.rotation);
            t.transform.SetParent(this.transform);
            t.GetComponent<TreeManger>().RandomizeTree();


            ActiveTrees.Add(t);
        }
    }

    private void Start()
    {
        PlaceTreeInitial();
    }

    private float timer = 0;

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer > timeBetweenchecks)
        {
            foreach (GameObject tree in ActiveTrees)
            {
                if (Vector3.Distance(tree.transform.position, PlayerPos.position) > PlacementRadiusMax)
                {
                    HideTree(tree);
                }
            }

            if (ActiveTrees.Count < TreeCountMax)
            {
                foreach (GameObject tree in HiddenTrees)
                {
                    if (Vector3.Distance(tree.transform.position, PlayerPos.position) < PlacementRadiusMax)
                    {
                        ActivateTree(tree);
                    }
                }

                while (ActiveTrees.Count < TreeCountMax)
                {
                    PlaceRandomTree();
                }

            }
            timer = 0;
        }



        
    }




}
