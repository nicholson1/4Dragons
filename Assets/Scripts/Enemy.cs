using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    
    [SerializeField] public NavMeshAgent navMeshAgent;
    [SerializeField] public CharacterMovement player;
    public GameObject prefab;
    private EnemyPooler thePool;



    public bool _frozen;
    private float _frozenTimer = 5f;

    [SerializeField] private Material frozenMaterial;
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private MeshRenderer _meshRenderer;


    private SoundObserver SO;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMovement>();
        thePool = FindObjectOfType<EnemyPooler>();
        SO = FindObjectOfType<SoundObserver>();
    }

    void Update()
    {



        if (!_frozen)
        {
            if (navMeshAgent.isActiveAndEnabled)
            {

            

            navMeshAgent.SetDestination(player.transform.position);

            if (player.feared)
            {
                Vector3 toPlayer = player.transform.position - transform.position;
                if (Vector3.Distance(player.transform.position, transform.position) < 3)
                {
                    Vector3 targetPosition = toPlayer.normalized * -7f;
                    navMeshAgent.destination = targetPosition;
                    navMeshAgent.isStopped = false;
                }
            }
        }

        //navMeshAgent.
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
        
       thePool.SpawnEnemy();
       thePool.SpawnEnemy();

       if (_frozen)
       {
           SO.playEnemyDeath();
       }
       else
       {
           SO.playEnemyDeathFire();
       }
       
       thePool.KillEnemy(this);
        
        
        
        GameObject.FindObjectOfType<GameManager>().GainPoints();

        gameObject.SetActive(false);


    }

    public void Damage()
    {
        GameObject.FindObjectOfType<GameManager>().lives -= 1;
        SO.playPlayerHit();
        thePool.KillEnemy(this);
        

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("fire"))
        {
            Eaten();
        }
    }
}
