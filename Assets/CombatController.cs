using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] private GameObject CombatUI;
    
    [SerializeField] private Camera TransitionCamera;
    [SerializeField] private Camera CombatCamera;
    [SerializeField] private GameObject CombatCamParent;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private LayerMask Ground;

    [SerializeField] private GameObject _healthBarPrefab;


    public Vector3 playerOffset = new Vector3();

    private Character Player;

    private void TransitionToCombat(Vector3 DirVect)
    {
        //set transition camera to starting point
        TransitionCamera.transform.position = MainCamera.transform.position;
        TransitionCamera.transform.rotation = MainCamera.transform.rotation;
        
        //set combat camera position to player offset + rotation
        //CombatCamParent.transform.position = Player.transform.position +  (Player.transform.rotation * Vector3.forward * 2.5f);
        CombatCamParent.transform.position = Player.transform.position +  ((DirVect * 2.5f));

        
        CombatCamParent.transform.LookAt(new Vector3(Player.transform.position.x, CombatCamParent.transform.position.y, Player.transform.position.z));
        //CombatCamera.transform
        
        StartCoroutine(TransitionToCombatCamera(2, 2));

    }
    private void TransitionFromCombat()
    {
        //set transition camera to starting point
        TransitionCamera.transform.position = CombatCamera.transform.position;
        TransitionCamera.transform.rotation = CombatCamera.transform.rotation;
        
        //set combat camera position to player offset + rotation
        //CombatCamParent.transform.position = Player.transform.position +  (Player.transform.rotation * Vector3.forward * 2.5f);
        //CombatCamParent.transform.position = Player.transform.position +  ((DirVect * 2.5f));

        
        //CombatCamParent.transform.LookAt(new Vector3(Player.transform.position.x, CombatCamParent.transform.position.y, Player.transform.position.z));
        //CombatCamera.transform
        
        StartCoroutine(TransitionFromCombatCamera(2, 2));

    }
    private void EndCombat()
    {
        TransitionFromCombat();
    }
    private IEnumerator TransitionFromCombatCamera(float moveTime, float rotateTime)
    {
        CombatUI.SetActive(false);

        CombatCamera.gameObject.SetActive(false);
        TransitionCamera.gameObject.SetActive(true);
        float elapsedTime = 0;
         
        while (elapsedTime < moveTime)
        {
            TransitionCamera.transform.position = Vector3.Lerp(CombatCamera.transform.position,MainCamera.transform.position, (elapsedTime / moveTime));
            TransitionCamera.transform.rotation = Quaternion.Lerp( CombatCamera.transform.rotation,MainCamera.transform.rotation, (elapsedTime / rotateTime));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        TransitionCamera.gameObject.SetActive(false);
        MainCamera.gameObject.SetActive(true);

    }

    private IEnumerator TransitionToCombatCamera(float moveTime, float rotateTime)
    {
        //deactivate main camera
        MainCamera.gameObject.SetActive(false);
        // activate transition camera
        TransitionCamera.gameObject.SetActive(true);
        // move transition camera to combat cam position
        float elapsedTime = 0;
         
        while (elapsedTime < moveTime)
        {
            TransitionCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, CombatCamera.transform.position, (elapsedTime / moveTime));
            TransitionCamera.transform.rotation = Quaternion.Lerp(MainCamera.transform.rotation, CombatCamera.transform.rotation, (elapsedTime / rotateTime));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        TransitionCamera.gameObject.SetActive(false);
        CombatCamera.gameObject.SetActive(true);

        CombatUI.SetActive(true);
        ActivateHealthBars();
        // activate health bars
        
        


        

        // move transition camera to combat cam rotation
        // deactivate transition camera
        // activate combat cam

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

        Debug.LogWarning("No Ground Found");
        return Vector3.zero;
    }

    private void StartCombat(Character player, Character enemy)
    {
        Player = player;
        Vector3 DirVect = GetDirectionVector();
        StartCoroutine(LerpMoveEnemy(enemy.transform, 1, Player.transform.position + (DirVect * 5)));
        
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
