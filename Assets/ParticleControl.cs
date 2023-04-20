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

    public void Initialize(Transform location, float delay, float duration)
    {
        transform.position = location.position;
        DelayTime = delay;
        LifeTime = duration;
        isOn = false;
        ParticleSystem.SetActive(false);
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
        
        Purge
        
        
        
    }
}
