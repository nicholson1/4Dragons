using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleControl : MonoBehaviour
{
    public float DelayTime;
    public float LifeTime;
    public ParticleType particleType;

    [SerializeField] private GameObject ParticleSystem;
    private bool isOn = false;
    private bool isMoving = false;
    public void Initialize(Transform location, float delay, float duration)
    {
        transform.position = location.position;
        DelayTime = delay;
        LifeTime = duration;
        isOn = false;
        ParticleSystem.SetActive(false);
    }


    private Vector3 target;
    private float movespeed = 8;
    public void Initialize(Transform start, Transform end, float delay, float duration, float moveSpeed = 8)
    {
        movespeed = moveSpeed;
        target = end.position;
        target += new Vector3(0f,1f,0f);
        // find faceing direction
        // start pos + 1
        transform.LookAt(end);
        transform.position = start.position + new Vector3(0f,1f,0f); ;
        DelayTime = delay;
        LifeTime = duration;
        ParticleSystem.SetActive(false);
        isOn = false;
        isMoving = true;
    }
    
    
    private void LateUpdate()
    {
        
        if (!isOn)
        {
            DelayTime -= Time.deltaTime;
            if (DelayTime < 0)
            {
                isOn = true;
                ParticleSystem.SetActive(true);
            }

            if (isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, -(movespeed-1)* Time.deltaTime);
            }

        }

        if (isOn)
        {
            LifeTime -= Time.deltaTime;
            if (LifeTime <= 0)
            {

                ParticleSystem.SetActive(false);
                isOn = false;
                // pool it
                ParticleManager._instance.PoolParticle(this);
            }
        }
        


        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, movespeed* Time.deltaTime );

            // if (Vector3.Distance(transform.position, target) < .2f)
            // {
            //     // wait for a sec
            //     ParticleSystem.SetActive(false);
            //     isOn = false;
            //     // pool it
            //     ParticleManager._instance.PoolParticle(this);
            // }
        }
        
    }


    public enum ParticleType
    {
        NatureCircle,
        RejuvHealing,
        Empower,
        Weaken,
        Blizzard,
        Shatter,
        Thorns,
        Meditate,
        Burn,
        Bleed,
        IceBlock,
        Exposed,
        Wounded,
        Immortal,
        Invulnerable,
        LifeTap,
        Chill,
        MeteorStrike,
        LifeLeech,
        ShadowBolt,
        FrostBolt,
        FireBall,
        Wrath,
        Smelt,
        BloodLifeTap,

        
        
        Purge
        
        
        
    }
}
