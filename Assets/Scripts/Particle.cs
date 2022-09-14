using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public ParticleSystem part;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
        
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log(other.tag);
        if (other.tag == "Enemy")
        {
            other.GetComponent<Enemy>().Eaten();
            Debug.Log("enemy burned");

        }
        
    }
}
