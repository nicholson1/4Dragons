using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public class II_PlayerRotation : MonoBehaviour
    {
        private II_Player player => GetComponent<II_Player>();

        private Vector3 inputDirection;
        public float rotationSpeed = 3;

        public Transform rotatingTransform;

        void Start()
        {
            player.RotationEvent += UpdateRotation;
        }

        private void OnDestroy()
        {
            player.RotationEvent -= UpdateRotation;
        }


        void Update()
        {
            Rotate();
        }

        private void Rotate()
        {
            rotatingTransform.Rotate(inputDirection * rotationSpeed * Time.deltaTime);
        }

        public void UpdateRotation()
        {
            inputDirection = player.GetRotationDirection();
        }
    }
}