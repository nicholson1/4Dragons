using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovement1 : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float jumpSpeed;
    public float GravityScale = 5;
    public float dashSpeed;
    public float dashTime;
    public float dashCooldown;
    private float dashTimer;

    public Vector3 Drag;

    private CharacterController characterController;
    private float ySpeed;
    private float originalStepOffset;
    private bool _isDashing = false;
    private bool _airActionTaken = false;

    [SerializeField] Animator _animator;
    
    
    

    //private SoundObserver SO;
    public static event Action playerJumpEvent;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;

    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = Vector3.zero;
        if (verticalInput > 0)
        {
           movementDirection = transform.forward;
        }
        else if(verticalInput < 0)
        {
            movementDirection = -transform.forward;

        }

       
        float magnitude = Mathf.Clamp01(movementDirection.magnitude) * speed;
        movementDirection.Normalize();

        ySpeed += Physics.gravity.y * GravityScale  * Time.deltaTime;

        
        if (characterController.isGrounded)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ySpeed = jumpSpeed;
                _airActionTaken = false;
                _animator.SetTrigger("Jump");
                //if(playerJumpEvent!= null) playerJumpEvent();


            }
        }

        if (!characterController.isGrounded && _airActionTaken == false)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ySpeed = jumpSpeed;
                //if(playerJumpEvent!= null) playerJumpEvent();
                _animator.SetTrigger("DoubleJump");


                if (!characterController.isGrounded)
                {
                    _airActionTaken = true;
                }
            }
        }

        // else
        // {
        //     characterController.stepOffset = 0;
        // }
        
        
        
        Vector3 velocity = movementDirection * magnitude;
        velocity.y = ySpeed;

        
        //dash
        if (dashTimer <= 0 && !_isDashing)
        {
            if (characterController.isGrounded )
            {
                if ( Input.GetButtonDown("Dash"))
                {
                    Debug.Log("dash");

                    StartCoroutine(Dash());
                }
            }
            else if (_airActionTaken == false)
            {
                if ( Input.GetButtonDown("Dash"))
                {
                    
                    _airActionTaken = true;
                    
                    StartCoroutine(Dash());
                }
            }
                
        }
        
        
        //DRAG
        //velocity.x /= 1 + Drag.x * Time.deltaTime;
        //velocity.y /= 1 + Drag.y * Time.deltaTime;
        //velocity.z /= 1 + Drag.z * Time.deltaTime;


        if (!_isDashing)
        {
            transform.Rotate(0, Input.GetAxis("Horizontal")*rotationSpeed*Time.deltaTime, 0);

        }

       
        

        

        characterController.Move(velocity * Time.deltaTime);

        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
        }

       
    }

    private IEnumerator Dash()
    {
        dashTimer += dashCooldown;
        _isDashing = true;
        float startTime = Time.time; // need to remember this to know how long to dash
        while(Time.time < startTime + dashTime)
        {
            characterController.Move(transform.forward * dashSpeed * Time.deltaTime);
            // or controller.Move(...), dunno about that script
            yield return null; // this will make Unity stop here and continue next frame
        }

        _isDashing = false;
    }
}
    

