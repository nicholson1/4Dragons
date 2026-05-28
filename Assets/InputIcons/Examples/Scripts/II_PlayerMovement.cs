using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public class II_PlayerMovement : MonoBehaviour
    {

        private II_Player player => GetComponent<II_Player>();

        [Header("Movement Settings")]
        public float movementSpeed = 3f;


        private Vector3 inputDirection;
        private Vector3 smoothInputMovement;
        public float movementSmoothingTime = 1f;

        public bool allowVerticalMovement = false;

        public float jumpForce = 4f;
        public float jumpCooldown = 2f;
        private float currentJumpCooldown = 1f;
        private Rigidbody2D body => GetComponent<Rigidbody2D>();

        // Start is called before the first frame update
        void Start()
        {
            player.MovementEvent += UpdateMovement;
            player.JumpEvent += Jump;
        }

        private void OnDestroy()
        {
            player.MovementEvent -= UpdateMovement;
            player.JumpEvent -= Jump;
        }

        void Update()
        {
            CalculateMovementInputSmoothing();
            Move();

            if (currentJumpCooldown > 0)
                currentJumpCooldown -= Time.deltaTime;
        }

        public void Jump()
        {
            if (currentJumpCooldown > 0)
                return;

            body.velocity = Vector2.zero;
            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currentJumpCooldown = jumpCooldown;
        }

        public void UpdateMovement()
        {
            inputDirection = player.GetInputDirection();

        }

        private void CalculateMovementInputSmoothing()
        {
            smoothInputMovement = Vector3.Lerp(smoothInputMovement, inputDirection, Time.deltaTime / movementSmoothingTime);
        }


        private void Move()
        {
            Vector3 moveVec = new Vector3(smoothInputMovement.x, 0,0);
            if (allowVerticalMovement)
                moveVec.y = smoothInputMovement.y;

            Vector3 movement = moveVec * movementSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }
}
