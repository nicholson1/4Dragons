using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject CombatUI;
    [SerializeField] private GameObject ShopUI;

    public GameObject RestartButton;


    private bool haveInitializedEquipmentItems = false;
    private bool moving;


    private bool InventoryOn = false;
    public static UIController _instance;

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    public void ToggleInventoryUI(int force = -1)
    {
        EquipmentManager._instance.c.UpdateStats();
        if (force == 0)
        {
            if (InventoryOn == false)
            {
                return;
            }
        }
        if (force == 1)
        {
            if (InventoryOn == true)
            {
                return;
            }
        }
        
        if (!moving)
        {
            InventoryOn = !InventoryOn;
            StartCoroutine(MoveObject(inventoryUI));
            

        }
        
        if (!haveInitializedEquipmentItems)
        {
            EquipmentManager._instance.InitializeEquipmentAndInventoryItems();
            haveInitializedEquipmentItems = true;
        }

        if (CombatController._instance.entitiesInCombat.Count > 1)
        {
            CombatController._instance.UpdateUiButtons();

        }
        //ToggleShopUI();
        
    }
    bool shopMoving = false;

    public void ToggleShopUI(int force = -1)
    {
        if(!shopMoving)
            StartCoroutine(MoveShopObject(ShopUI));
    }

    IEnumerator MoveObject(GameObject moveObj)
    { 
        moving = true;
        
        Vector2 startpos = moveObj.transform.position;
        Vector2 endpos = new Vector2(-startpos.x, startpos.y);
        
            
        float t = 0;
        while (t < 1)
        {
            moveObj.transform.position = Vector2.Lerp(startpos, endpos, t);
            t = t + Time.deltaTime / .75f;
            yield return new WaitForEndOfFrame();
        }

        moveObj.transform.position = endpos;

        moving = false;
    }
    IEnumerator MoveShopObject(GameObject moveObj)
    {
        
        shopMoving = true;
        
        Vector2 startpos = moveObj.GetComponent<RectTransform>().anchoredPosition;
        Vector2 endpos = new Vector2(-startpos.x, startpos.y);
        
        //Debug.Log(startpos);
        //Debug.Log(endpos);
            
        float t = 0;
        while (t < 1)
        {
            moveObj.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startpos, endpos, t);
            t = t + Time.deltaTime / .75f;
            yield return new WaitForEndOfFrame();
        }

        moveObj.GetComponent<RectTransform>().anchoredPosition = endpos;

        shopMoving = false;
    }


}
