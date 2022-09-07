using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    
    [SerializeField] public NavMeshAgent navMeshAgent;
    [SerializeField] public GameObject player;
    

    private bool _frozen;
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
}
