using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject CombatUI;
    [SerializeField] private GameObject ShopUI;
    [SerializeField] private GameObject SettingUI;
    [SerializeField] private GameObject VictoryUI;
    [SerializeField] private GameObject TitleScreen;
    [SerializeField] private GameObject MapUI;
    
    [SerializeField] private GameObject LootUI;

    [SerializeField] private GameObject CustomizeUI;

    [SerializeField] private GameObject InventoryButton;
    [SerializeField] private GameObject MapButton;

    [SerializeField] private GameObject MainCamera;
    [SerializeField] private GameObject TransitionCamera;
    [SerializeField] private GameObject UiCamera;
    
    [SerializeField] private GameObject RelicTester;
    [SerializeField] private GameObject ModTester;

    [SerializeField] private GameObject DailyChallengeUI;
    [SerializeField] private TextMeshProUGUI DailyChallengeTitle;
    [SerializeField] private TextMeshProUGUI DailyChallengeDescription;
    [SerializeField] private TextMeshProUGUI ModDisplay;
    [SerializeField] private GameObject ModScroll;
    [SerializeField] public GameObject CinemaUI;

    
    public GameObject RestartButton;
    private bool haveInitializedEquipmentItems = false;
    private bool moving;


    private bool InventoryOn = false;
    public static UIController _instance;
    
    //========= Sound Fx ===========
    public AudioClip _hoverSFX;
    [SerializeField] private float hoverVol;
    public AudioClip _buttonClickSFX;
    [SerializeField] private float clickVol;
    [SerializeField] public AudioClip _OpenShopSFX;
    [SerializeField] private float openShopVol;
    public AudioClip _openLootSFX;
    [SerializeField] private float OpenLootVol;
    public AudioClip _openMapSFX;
    [SerializeField] private float openMapVol;
    public AudioClip _openInventorySFX;
    [SerializeField] private float openInventoryVol;
    
    
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
        InventoryButton.SetActive(true);
        MapButton.SetActive(true);
        DailyChallengeUI.gameObject.SetActive(false);
    }

    public void ActivateDailyChallengeUI()
    {
        TitleScreen.SetActive(false);
        DailyChallengeUI.gameObject.SetActive(true);

        List<List<object>> dailyChallenges = DataReader._instance.GetDailyChallengesTable();
        DateTime currentDateTime = DateTime.Now;
        
        int challengeID = currentDateTime.Second % 9;
        //challengeID = 8;
        List<int> mods = (List<int>)dailyChallenges[challengeID][3];

        foreach (Transform child in ModScroll.transform)
        {
            child.gameObject.SetActive(false);
        }
        // load modifiers
        foreach (int i in mods)
        {
            Modifiers._instance.AdjustMod((Mods)i, true);
            TextMeshProUGUI mod = Instantiate(ModDisplay, ModScroll.transform);
            mod.gameObject.SetActive(true);

            mod.text = CamelCaseToSpaced(((Mods)i).ToString());


        }
        // load name
        DailyChallengeTitle.text = (string)dailyChallenges[challengeID][1];

        // load descritpion
        DailyChallengeDescription.text = (string)dailyChallenges[challengeID][2];

    }
    public void CloseDailyChallengeUI()
    {
        DailyChallengeUI.gameObject.SetActive(false);
        TitleScreen.SetActive(true);
        Modifiers._instance.ClearMods();
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
                //Debug.Log("inventory is off, forced");
                return;
            }
        }
        if (force == 1)
        {
            if (InventoryOn == true)
            {
                //Debug.Log("inventory is on, forced");
                return;
            }
        }
        
        if (!moving)
        {
            InventoryOn = !InventoryOn;
            StartCoroutine(MoveObject(inventoryUI));
            //Debug.Log("starting coroutine " + InventoryOn);

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
        PlayOpenInventory();
        //ToggleShopUI();
        
    }
    bool mapMoving = false;
    private bool MapOn;

    public void ToggleMapUI(int force = -1)
    {
        if (force == 0)
        {
            if (MapOn == false)
            {
                return;
            }
        }
        if (force == 1)
        {
            if (MapOn == true)
            {
                return;
            }
        }
        
        if (!mapMoving)
        {
            MapOn = !MapOn;
            StartCoroutine(MoveMapObject(MapUI));
        }
        //ToggleShopUI();
        PlayOpenMap();


    }
    bool shopMoving = false;
    private bool shopOn = false;
    public void ToggleShopUI(int force = -1)
    {
        if (force == 0)
        {
            if (shopOn == false)
            {
                return;
            }
        }
        if (force == 1)
        {
            if (shopOn == true)
            {
                return;
            }
        }

        if(!shopMoving)
        {
            shopOn = !shopOn;
            StartCoroutine(MoveShopObject(ShopUI));
        }
        
        PlayOpenShop();

    }
    bool LootMoving = false;
    private bool lootOn = false;

    public void ToggleLootUI(int force = -1)
    {
        if (force == 0)
        {
            if (lootOn == false)
            {
                return;
            }
        }
        if (force == 1)
        {
            if (lootOn == true)
            {
                return;
            }
        }

        if(!LootMoving)
        {
            lootOn = !lootOn;
            StartCoroutine(MoveLootObject(LootUI));
        }
        
        PlayOpenLoot();
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
    IEnumerator MoveMapObject(GameObject moveObj)
    { 
        mapMoving = true;
        RectTransform rt = moveObj.GetComponent<RectTransform>();
        Vector2 startpos = rt.anchoredPosition;
        Vector2 endpos = new Vector2(startpos.x, -startpos.y);
        
        //Debug.Log(startpos.y + " == " + endpos.y);
            
        float t = 0;
        while (t < 1)
        {
            rt.anchoredPosition = Vector2.Lerp(startpos, endpos, t);
            t = t + Time.deltaTime / .75f;
            yield return new WaitForEndOfFrame();
        }

        rt.anchoredPosition = endpos;

        mapMoving = false;
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
    IEnumerator MoveLootObject(GameObject moveObj)
    {
        
        LootMoving = true;
        
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

        LootMoving = false;
    }


    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.R))
        {
            RelicTester.gameObject.SetActive(true);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.T))
        {
            ModTester.gameObject.SetActive(true);
        }
    }
    public static string CamelCaseToSpaced(string camelCaseString)
    {
        // Add spaces before each uppercase letter and capitalize the first letter
        return Regex.Replace(camelCaseString, "(\\B[A-Z])", " $1");
    }
    public void CloseApplication()
    {
#if UNITY_EDITOR
        // Stops playing in the editor
        EditorApplication.isPlaying = false;
#else
            // Closes the application in a standalone build
            Application.Quit();
#endif
    }

    public void PlayUIHover()
    {
        SoundManager.Instance.Play2DSFX(_hoverSFX, hoverVol, .75f, .05f);
    }
    public void PlayUIClick()
    {
        SoundManager.Instance.Play2DSFX(_buttonClickSFX, clickVol, 1, .05f);
    }
    
    public void PlayOpenShop()
    {
        SoundManager.Instance.Play2DSFX(_OpenShopSFX, openShopVol);
    }
    public void PlayOpenLoot()
    {
        SoundManager.Instance.Play2DSFX(_openLootSFX, OpenLootVol);
    }
    public void PlayOpenMap()
    {
        SoundManager.Instance.Play2DSFX(_openMapSFX, openMapVol);
    }
    public void PlayOpenInventory()
    {
        SoundManager.Instance.Play2DSFX(_openInventorySFX, openInventoryVol);
    }
}
