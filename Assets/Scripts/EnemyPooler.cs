using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPooler : MonoBehaviour
{
    public Enemy EnemyPrefab;
    
    
    
    List<Enemy> DeadList = new List<Enemy>();

    public void SpawnEnemy()
    {
        if (DeadList.Count != 0)
        {
            Vector3 spawnPos = GetRandomPoint(Vector3.zero, 30f);
            DeadList[0].transform.position = spawnPos;
            DeadList[0]._frozen = true;
            DeadList[0].gameObject.SetActive(true);
            DeadList.RemoveAt(0);
        }
        else
        {
            Vector3 spawnPos = GetRandomPoint(Vector3.zero, 30f);
            Enemy spawn = Instantiate(EnemyPrefab).GetComponent<Enemy>();
            spawn.transform.position = spawnPos;
            spawn.Freeze();
            
        }
        
    }

    public void KillEnemy(Enemy enemy)
    {
        DeadList.Add(enemy);
        enemy.gameObject.SetActive(false);
        
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
