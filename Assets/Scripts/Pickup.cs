using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public abstract class Pickup : MonoBehaviour
{

    //public PickupType type;

    

    public virtual void PickupAction()
    {
        //switch (type)
        //{
        //    case PickupType.Freeze:
        //        Freeze();
        //        break;
        //    case PickupType.Speed:
        //        StartCoroutine(SpeedBoost());
        //        break;
        //    case PickupType.Fire:
        //        StartCoroutine(Fire());
        //        break;
        //    case PickupType.DoublePoints:
        //        StartCoroutine(DoublePoints());
        //        break;
        //    case PickupType.Health:
        //        Health();
        //        break;
        //    default:
        //        Debug.LogWarning("No Case Found");
        //        break;
        //}

        transform.position = GetRandomPoint(Vector3.zero, 20f); 
        transform.position = new Vector3(transform.position.x, 1, transform.position.z);
        
    }

    //private void Freeze()
    //{
    //    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    //    foreach (var enemy in enemies)
    //    {
    //        enemy.GetComponent<Enemy>().Freeze();
    //    }
    //}

    //private void Health()
    //{
    //    GameObject.FindObjectOfType<GameManager>().lives += 1;
    //}

    //public IEnumerator SpeedBoost()
    //{
    //    CharacterMovement player = GameObject.FindWithTag("Player").GetComponent<CharacterMovement>();
    //    player.speed += 5;
    //    yield return new WaitForSeconds(5);
    //    player.speed -= 5;
    //}
    //public IEnumerator DoublePoints()
    //{
    //    GameManager gm = GameObject.FindObjectOfType<GameManager>();
    //    gm.doublePoints = true;
    //    yield return new WaitForSeconds(10);
    //    gm.doublePoints = false;
    //}
    //public IEnumerator Fire()
    //{
    //    CharacterMovement player = GameObject.FindWithTag("Player").GetComponent<CharacterMovement>();
    //    player.Fire.SetActive(true);
    //    player.feared = true;
    //    yield return new WaitForSeconds(5);
    //    player.Fire.SetActive(false);
    //    player.feared = false;

    //}
    
    
    public enum PickupType
    {
        Freeze,
        Speed,
        Fire,
        Health,
        DoublePoints
    }
    
    
    
    
    
    
    public static Vector3 GetRandomPoint(Vector3 center, float maxDistance) {
        // Get Random Point inside Sphere which position is center, radius is maxDistance
        Vector3 randomPos = Random.insideUnitSphere * maxDistance + center;

        NavMeshHit hit; // NavMesh Sampling Info Container

        // from randomPos find a nearest point on NavMesh surface in range of maxDistance
        NavMesh.SamplePosition(randomPos, out hit, maxDistance, NavMesh.AllAreas);

        return hit.position;
    }
}
