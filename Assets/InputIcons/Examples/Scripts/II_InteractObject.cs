using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public class II_InteractObject : MonoBehaviour
    {

        private Animator anim => GetComponent<Animator>();
        private bool wasInteracted = false;

        public ParticleSystem particles1;
        public ParticleSystem particles2;

        void Start()
        {

        }

        void Update()
        {

        }

        public void Interact()
        {
            if (wasInteracted)
                return;

            anim.Play("Interact");
            wasInteracted = true;
        }

        public void SpawnParticles()
        {
            particles1.Play();
        }

        public void SpawnParticles2()
        {
            particles2.Play();
        }
    }
}