using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pickup : MonoBehaviour
{

    public PickupType type;

    public void PickupAction()
    {
        switch (type)
        {
            case PickupType.Freeze:
                Freeze();
                break;
            case PickupType.Speed:
                StartCoroutine(SpeedBoost());
                break;
        }

        transform.position = GetRandomPoint(Vector3.zero, 20f); 
        transform.position = new Vector3(transform.position.x, 1, transform.position.z);
        
    }

    private void Freeze()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            enemy.GetComponent<Enemy>().Freeze();
        }
    }

    public IEnumerator SpeedBoost()
    {
        CharacterMovement player = GameObject.FindWithTag("Player").GetComponent<CharacterMovement>();
        player.speed += 5;
        yield return new WaitForSeconds(5);
        player.speed -= 5;
        

    }
    
    
    public enum PickupType
    {
        Freeze,
        Speed,
        
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
