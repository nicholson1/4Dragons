using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CombatController : MonoBehaviour
{
    public static CombatController _instance;
    [SerializeField] private GameObject CombatUI;
    
    [SerializeField] private Camera TransitionCamera;
    [SerializeField] private Camera CombatCamera;
    [SerializeField] private GameObject CombatCamParent;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private LayerMask Ground;

    [SerializeField] private GameObject _healthBarPrefab;

    [SerializeField] private Character EnemeyPrefab;
    [SerializeField] private Character DragonPrefab;
    [SerializeField] private Character ElitePrefab;


    [SerializeField] private Transform SpawnPos;

    public List<CombatEntity> entitiesInCombat = new List<CombatEntity>();
    public MultiImageButton NextCombatButton;
    public MultiImageButton EndTurnButton;

    public Vector3 playerOffset = new Vector3();

    public int turnCounter = 0;
    public static event Action EndTurn;
    // public static event Action EndCombatEvent;
    // public static event Action StartCombatEvent;

    public static event Action<Character, Character> UpdateUIButtons;
    public static event Action<Character, Character> ActivateCombatEntities;


    public static event Action<ErrorMessageManager.Errors> CombatNotifications;

    public Character Player;

    private int CurrentTurnIndex = 0;

    private int ShopChancePercent = 0;
    
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

    public void DecideShopOrCombat()
    {
        
        
        //do we hit a shop
        int roll = Random.Range(1, 100);
        if (roll < ShopChancePercent)
        {
            ShopChancePercent = 0;
            ShopManager._instance.RandomShop();
        }
        else
        {
            StartRandomCombat();
        }
        
        if (Player._level > 5)
        {
            ShopChancePercent += 10;
        }

    }
    
    private void TransitionToCombat(Vector3 DirVect)
    {
        //set transition camera to starting point
        //TransitionCamera.transform.position = MainCamera.transform.position;
        //TransitionCamera.transform.rotation = MainCamera.transform.rotation;
        
        //set combat camera position to player offset + rotation
        //CombatCamParent.transform.position = Player.transform.position +  (Player.transform.rotation * Vector3.forward * 2.5f);
        //CombatCamParent.transform.position = Player.transform.position +  ((DirVect * 2.5f));

        
        //CombatCamParent.transform.LookAt(new Vector3(Player.transform.position.x, CombatCamParent.transform.position.y, Player.transform.position.z));
        //CombatCamera.transform
        
        StartCoroutine(TransitionToCombatCamera(2, 2));

    }
    private void TransitionFromCombat()
    {
        //set transition camera to starting point
        //TransitionCamera.transform.position = CombatCamera.transform.position;
        //TransitionCamera.transform.rotation = CombatCamera.transform.rotation;
        
        //set combat camera position to player offset + rotation
        //CombatCamParent.transform.position = Player.transform.position +  (Player.transform.rotation * Vector3.forward * 2.5f);
        //CombatCamParent.transform.position = Player.transform.position +  ((DirVect * 2.5f));

        
        //CombatCamParent.transform.LookAt(new Vector3(Player.transform.position.x, CombatCamParent.transform.position.y, Player.transform.position.z));
        //CombatCamera.transform
        
        StartCoroutine(TransitionFromCombatCamera(2, 2));

    }
    public void EndCombat()
    {
        TransitionFromCombat();
    }
    private IEnumerator TransitionFromCombatCamera(float moveTime, float rotateTime)
    {
        CombatUI.SetActive(false);
        CurrentTurnIndex = 0;

        //EndCombatEvent();
        //CombatCamera.gameObject.SetActive(false);
        //TransitionCamera.gameObject.SetActive(true);
        float elapsedTime = 0;
         
        // while (elapsedTime < moveTime)
        // {
        //     TransitionCamera.transform.position = Vector3.Lerp(CombatCamera.transform.position,MainCamera.transform.position, (elapsedTime / moveTime));
        //     TransitionCamera.transform.rotation = Quaternion.Lerp( CombatCamera.transform.rotation,MainCamera.transform.rotation, (elapsedTime / rotateTime));
        //
        //     elapsedTime += Time.deltaTime;
        //     yield return null;
        // }
        
        //TransitionCamera.gameObject.SetActive(false);
        //MainCamera.gameObject.SetActive(true);
        yield return 1;

    }

    private IEnumerator TransitionToCombatCamera(float moveTime, float rotateTime)
    {
        //deactivate main camera
        //MainCamera.gameObject.SetActive(false);
        // activate transition camera
        //TransitionCamera.gameObject.SetActive(true);
        // move transition camera to combat cam position
        float elapsedTime = 0;
         
        // while (elapsedTime < moveTime)
        // {
        //     TransitionCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, CombatCamera.transform.position, (elapsedTime / moveTime));
        //     TransitionCamera.transform.rotation = Quaternion.Lerp(MainCamera.transform.rotation, CombatCamera.transform.rotation, (elapsedTime / rotateTime));
        //
        //     elapsedTime += Time.deltaTime;
        //     yield return null;
        // }
        
        //TransitionCamera.gameObject.SetActive(false);
        //CombatCamera.gameObject.SetActive(true);

        CombatUI.SetActive(true);
        
        // activate health bars
        ActivateHealthBars();
        
        // get all combat entities, put the player at position 1
        GetAllCombatEntities();
        // start the turn
        entitiesInCombat[0].isMyTurn = true;
        if (CurrentTurnIndex == 0)
        {
            EndTurnButton.interactable = true;
        }
        else
        {
            EndTurnButton.interactable = false;

        }

        ActivateCombatEntities(entitiesInCombat[0].myCharacter, entitiesInCombat[1].myCharacter);
        UpdateUIButtons(entitiesInCombat[0].myCharacter, entitiesInCombat[1].myCharacter);
        EquipmentManager._instance.InitializeEquipmentAndInventoryItems();

        //StartCombatEvent();
        yield return 1;

    }

    public void UpdateUiButtons()
    {
        UpdateUIButtons(entitiesInCombat[0].myCharacter, entitiesInCombat[1].myCharacter);

    }

    private bool CheckForIntentions()
    {
        bool no_Intentions_running = true;

        while (no_Intentions_running)
        {
            
        }

        return no_Intentions_running;

    }

    public void Tie()
    {
        ToolTipManager._instance.HideToolTipAll();
        Player._currentHealth = Player._maxHealth;
        Player._currentEnergy = 0;
        
        Player.Buffs = new List<(CombatEntity.BuffTypes, int, float)>();
        Player.DeBuffs = new List<(CombatEntity.DeBuffTypes, int, float)>();
                    
        Player.UpdateStats();

        EndCombat();
        
        // trigger its a tie
        CombatNotifications(ErrorMessageManager.Errors.Tie);
        // activate next button
        NextCombatButton.gameObject.SetActive(true);
        // deactivate enemy
        //todo pool
        Destroy(entitiesInCombat[1].gameObject);
        
        

    }


    public void EndCurrentTurn()
    {
        // wait till no combat entitiy is intentionsRunning

        //Debug.Log(turnCounter + " turn counter");
        if (turnCounter > 30)
        {
            if (!entitiesInCombat[1].myCharacter.isDragon)
            {
                Tie();
                return;
            }
            
        }

        turnCounter += 1;
        
        EndTurn();
        CurrentTurnIndex += 1;
        if (CurrentTurnIndex >= entitiesInCombat.Count)
        {
            CurrentTurnIndex = 0;
        }

        if (CurrentTurnIndex == 0)
        {
            EndTurnButton.interactable = true;
        }
        else
        {
            EndTurnButton.interactable = false;

        }
        Debug.Log(entitiesInCombat[CurrentTurnIndex] + " now their turn");
        entitiesInCombat[CurrentTurnIndex].StartTurn();
        
    }

    private void GetAllCombatEntities()
    {
        entitiesInCombat = new List<CombatEntity>();
        List<CombatEntity> OutOfOrderCe = FindObjectsOfType<CombatEntity>().ToList();

        // put the player character at the front
        foreach (var ce in OutOfOrderCe)
        {
            if (ce.myCharacter.isPlayerCharacter)
            {
                entitiesInCombat.Add(ce);
                OutOfOrderCe.Remove(ce);
                break;
            }
        }
        entitiesInCombat.AddRange(OutOfOrderCe);

        foreach (var npc in OutOfOrderCe)
        {
            npc.SetMyIntentions();
        }
        
        
    }

    private void ActivateHealthBars()
    {
        //get all characters
        Character[] _chars = FindObjectsOfType<Character>();

        foreach (var c in _chars)
        {
            HealthBar hb = Instantiate(_healthBarPrefab, CombatUI.transform).GetComponent<HealthBar>();
            hb.Initialize(c, CombatCamera);
            
        }


    }



    private IEnumerator LerpMoveEnemy(Transform enemy, float moveTime, Vector3 pos)
    {
        // why are we going into the floor
        float elapsedTime = 0;
        pos.y = enemy.position.y;
        Vector3 startPos = enemy.position;

        while (elapsedTime < moveTime)
        {
            enemy.transform.position = Vector3.Lerp(startPos, pos, (elapsedTime / moveTime));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
       
        enemy.LookAt(new Vector3(Player.transform.position.x, enemy.position.y, Player.transform.position.z));
        //Player.transform.LookAt(enemy);
        
    }
    private IEnumerator RotatePlayer(Transform player, float moveTime, Vector3 pos)
    {
        float elapsedTime = 0;
        Quaternion startRot = player.rotation;
        var targetRotation = Quaternion.LookRotation(pos - Player.transform.position);
        //Quaternion targetRotation = Quaternion.LookRotation(player.transform.position + pos);

        while (elapsedTime < moveTime)
        {
            player.transform.rotation = Quaternion.Lerp(startRot, targetRotation, (elapsedTime / moveTime));


            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
    }

    private Vector3 GetDirectionVector()
    {
        //raycast down to find the block
        // find the position at the top of the block
        // player
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        //Debug.DrawRay(transform.position +Vector3.down, transform.TransformDirection(Vector3.up));
        if (Physics.Raycast(Player.transform.position, transform.TransformDirection(Vector3.down), out hit, 3, Ground))
        {
            //get world pos
            
            Vector3 target = new Vector3(hit.transform.position.x, hit.transform.parent.localScale.y, hit.transform.position.y);
            return (target - Player.transform.position).normalized;
            //Player.transform.LookAt(target);


        }

        //Debug.LogWarning("No Ground Found");
        return Vector3.zero;
    }

    public void StartRandomCombat()
    {
        
        UIController._instance.ToggleInventoryUI(0);

        NextCombatButton.gameObject.SetActive(false);

        Character enemy;
        if (Player._level == 10 || Player._level == 20 || Player._level == 25 || Player._level == 30 || (Player._level > 30 && Player._level % 5 == 0))
        {
            enemy = Instantiate(DragonPrefab, SpawnPos.position, DragonPrefab.transform.rotation);

        }
        else if (Player._level % 3 == 0)
        {
            enemy = Instantiate(ElitePrefab, SpawnPos.position, ElitePrefab.transform.rotation);
        }
        else
        {
            enemy = Instantiate(EnemeyPrefab, SpawnPos.position, EnemeyPrefab.transform.rotation);
            //ALWAYS DRAGON
            //enemy = Instantiate(DragonPrefab, SpawnPos.position, DragonPrefab.transform.rotation);
        }
        
        enemy.transform.LookAt(Player.transform.position);
        enemy._level = Player._level;

        if (enemy.isDragon)
        {
            enemy.GetComponent<Dragon>().InitializeDragon();
        }
        if (enemy.isElite)
        {
            enemy.GetComponent<Elite>().InitializeElite();
        }

        turnCounter = 0;
        StartCoroutine(waitTheStartCombat(Player, enemy));
    }

    private IEnumerator waitTheStartCombat(Character p, Character e)
    {
        CombatNotifications(ErrorMessageManager.Errors.NewFoe);
        yield return new WaitForSeconds(.5f);
        StartCombat(p, e);
        Debug.Log("hide tool tip at begining of combat");
        ToolTipManager._instance.HideToolTipAll();
    }

    private void StartCombat(Character player, Character enemy)
    {
        Player = player;
        Vector3 DirVect = GetDirectionVector();
        //StartCoroutine(LerpMoveEnemy(enemy.transform, 1, Player.transform.position + (DirVect * 5)));
        //StartCoroutine(LerpMoveEnemy(enemy.transform, 1, Player.transform.position + (new Vector3(0,0,10))));

        //Debug.Log("?");
        //get all the enemies
        //Move enemies to right location
        // make them look at each other
        // make them know they are in combat?
        // move the camera
        // activate the ui
        TransitionToCombat(DirVect);
        //StartCoroutine(RotatePlayer(Player.transform, 1, DirVect));
        Vector3 lookDirection = player.transform.position + DirVect;
        Player.transform.LookAt(new Vector3(lookDirection.x, Player.transform.position.y, lookDirection.z));


    }

    
    private void Start()
    {
        CombatTrigger.TriggerCombat += StartCombat;
        CombatTrigger.EndCombat += EndCombat;
    }
    private void OnDestroy()
    {
        CombatTrigger.TriggerCombat -= StartCombat;
        CombatTrigger.EndCombat -= EndCombat;

    }
}
