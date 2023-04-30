using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject CombatUI;
    [SerializeField] private GameObject ShopUI;
    [SerializeField] private GameObject SettingUI;
    [SerializeField] private GameObject VictoryUI;
    [SerializeField] private GameObject TitleScreen;



    [SerializeField] private GameObject CustomizeUI;

    [SerializeField] private GameObject BeginAdventureButton;
    [SerializeField] private GameObject CustomizeButton;

    [SerializeField] private GameObject MainCamera;
    [SerializeField] private GameObject TransitionCamera;
    [SerializeField] private GameObject UiCamera;
    
    
    
    public GameObject RestartButton;
    private bool haveInitializedEquipmentItems = false;
    private bool moving;


    private bool InventoryOn = false;
    public static UIController _instance;

    public void ToggleSettings()
    {
        SettingUI.gameObject.SetActive(!SettingUI.activeSelf);
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    public void LeaveMainMenu()
    {
        TitleScreen.SetActive(false);
    }


    public void ActivateCustomizeUI()
    {
        StartCoroutine(TransitionToUiCamera(1, 1));
    }
    
    public void DeactivateVictoryScreen()
    {
        StartCoroutine(TransitionToMainCameraFromVictory(1, 1));
    }
    public void ActivateVictoryScreen()
    {
        StartCoroutine(TransitionToVictoryUiCamera(1, 1));
    }
    

    public void ActivateMainMenu()
    {
        StartCoroutine(TransitionToMainCamera(1, 1));
    }

    private IEnumerator TransitionToUiCamera(float moveTime, float rotateTime)
    {
        TitleScreen.SetActive(false);
            

        //deactivate main camera
        MainCamera.gameObject.SetActive(false);
        // activate transition camera
        TransitionCamera.gameObject.SetActive(true);
        // move transition camera to combat cam position
        float elapsedTime = 0;

        while (elapsedTime < moveTime)
        {
            TransitionCamera.transform.position = Vector3.Lerp(MainCamera.transform.position,
                UiCamera.transform.position, (elapsedTime / moveTime));
            TransitionCamera.transform.rotation = Quaternion.Lerp(MainCamera.transform.rotation,
                UiCamera.transform.rotation, (elapsedTime / rotateTime));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        TransitionCamera.gameObject.SetActive(false);
        UiCamera.gameObject.SetActive(true);
        CustomizeUI.SetActive(true);

    }
    private IEnumerator TransitionToVictoryUiCamera(float moveTime, float rotateTime)
    {
        CombatController._instance.NextCombatButton.gameObject.SetActive(false);

        

        //deactivate main camera
        MainCamera.gameObject.SetActive(false);
        // activate transition camera
        TransitionCamera.gameObject.SetActive(true);
        // move transition camera to combat cam position
        float elapsedTime = 0;

        while (elapsedTime < moveTime)
        {
            TransitionCamera.transform.position = Vector3.Lerp(MainCamera.transform.position,
                UiCamera.transform.position, (elapsedTime / moveTime));
            TransitionCamera.transform.rotation = Quaternion.Lerp(MainCamera.transform.rotation,
                UiCamera.transform.rotation, (elapsedTime / rotateTime));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        TransitionCamera.gameObject.SetActive(false);
        UiCamera.gameObject.SetActive(true);
        VictoryUI.SetActive(true);
        CombatController._instance.Player._am.SetTrigger("Victory");

    }
    private IEnumerator TransitionToMainCameraFromVictory(float moveTime, float rotateTime)
    {
        VictoryUI.SetActive(false);


        //deactivate main camera
        UiCamera.gameObject.SetActive(false);
        // activate transition camera
        TransitionCamera.gameObject.SetActive(true);
        // move transition camera to combat cam position
        float elapsedTime = 0;

        while (elapsedTime < moveTime)
        {
            TransitionCamera.transform.position = Vector3.Lerp(UiCamera.transform.position,
                MainCamera.transform.position, (elapsedTime / moveTime));
            TransitionCamera.transform.rotation = Quaternion.Lerp(UiCamera.transform.rotation,
                MainCamera.transform.rotation, (elapsedTime / rotateTime));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        TransitionCamera.gameObject.SetActive(false);
        MainCamera.gameObject.SetActive(true);
        
        CombatController._instance.NextCombatButton.gameObject.SetActive(true);
        CombatController._instance.Player._am.SetTrigger("Reset");


    }
    private IEnumerator TransitionToMainCamera(float moveTime, float rotateTime)
    {
        CustomizeUI.SetActive(false);


        //deactivate main camera
        UiCamera.gameObject.SetActive(false);
        // activate transition camera
        TransitionCamera.gameObject.SetActive(true);
        // move transition camera to combat cam position
        float elapsedTime = 0;

        while (elapsedTime < moveTime)
        {
            TransitionCamera.transform.position = Vector3.Lerp(UiCamera.transform.position,
                MainCamera.transform.position, (elapsedTime / moveTime));
            TransitionCamera.transform.rotation = Quaternion.Lerp(UiCamera.transform.rotation,
                MainCamera.transform.rotation, (elapsedTime / rotateTime));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        TransitionCamera.gameObject.SetActive(false);
        MainCamera.gameObject.SetActive(true);
        TitleScreen.SetActive(true);

 

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
