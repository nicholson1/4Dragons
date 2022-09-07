using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    
    [SerializeField] public NavMeshAgent navMeshAgent;
    [SerializeField] public GameObject player;
    

    public bool _frozen;
    private float _frozenTimer = 5f;

    [SerializeField] private Material frozenMaterial;
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private MeshRenderer _meshRenderer;
    void Update()
    {
        if (!_frozen)
        {
            navMeshAgent.SetDestination(player.transform.position);
        }
        else
        {
            _frozenTimer -= Time.deltaTime;
            if (_frozenTimer < 0)
            {
                UnFreeze();
            }
        }
    }

    public void Freeze()
    {
        navMeshAgent.isStopped = true;
        _frozen = true;
        _frozenTimer = 5;
        _meshRenderer.material = frozenMaterial;
    }

    public void UnFreeze()
    {
        _frozen = false;
        navMeshAgent.isStopped = false;
        _meshRenderer.material = enemyMaterial;
    }

    public void Eaten()
    {
        transform.position = GetRandomPoint(Vector3.zero, 30f);
        UnFreeze();
        Vector3 spawnPos = GetRandomPoint(Vector3.zero, 30f);
        GameObject spawn = Instantiate(this.gameObject);
        spawn.transform.position = spawnPos;

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
