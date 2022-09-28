using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class CharacterMovement : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float jumpSpeed;

    private CharacterController characterController;
    private float ySpeed;
    private float originalStepOffset;

    public GameObject Fire;
    public bool feared = false;
    private SoundObserver SO;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;
        SO = FindObjectOfType<SoundObserver>();

    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        float magnitude = Mathf.Clamp01(movementDirection.magnitude) * speed;
        movementDirection.Normalize();

        ySpeed += Physics.gravity.y *5  * Time.deltaTime;

        
        if (characterController.isGrounded)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ySpeed = jumpSpeed;
                SO.playJump();
            }
        }
        else
        {
            characterController.stepOffset = 0;
        }

       
        

        Vector3 velocity = movementDirection * magnitude;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

       
    }
}
    // [SerializeField] private CharacterController characterController;
    //
    // public float speed;
    //
    // [SerializeField] private float jumpHeight = 3.5f;
    // private bool jump;
    // private bool isGrounded;
    //
    // [SerializeField] private float gravity = -30f;
    // Vector3 verticalVelocity = Vector3.zero;
    //
    // public float raycastDist;
    // //[SerializeField] private LayerMask groundMask;
    //
    // void FixedUpdate()
    // {
    //     
    //    
    //     
    // }
    //
    // void Update()
    // {
    //     
    //     isGrounded = Physics.CheckSphere(transform.position, raycastDist, 3);
    //     if (isGrounded)
    //     {
    //         verticalVelocity.y = 0;
    //     }
    //     
    //     
    //     if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
    //     {
    //         jump = true;
    //         Debug.Log("jump = true");
    //
    //     }
    //     
    //     if (jump)
    //     {
    //         if (isGrounded)
    //         {
    //             verticalVelocity.y = Mathf.Sqrt(-2f * jumpHeight * gravity);
    //             isGrounded = false;
    //             Debug.Log("jump");
    //         }
    //
    //         jump = false;
    //     }
    //     if (!isGrounded)
    //     {
    //         verticalVelocity.y += gravity * Time.fixedDeltaTime;
    //         Debug.Log(verticalVelocity.y);
    //     }
    //
    //     
    //     
    //     Vector3 move = new Vector3(Input.GetAxis("Horizontal"), verticalVelocity.y, Input.GetAxis("Vertical"));
    //     characterController.Move(move * Time.deltaTime * speed);
    //     
    //     
    // }

