using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    

    public float Speed = 5f;
    public float JumpHeight = 2f;
    public float GroundDistance = 0.2f;
    public float DashDistance = 5f;
    public LayerMask Ground;
    public float rotationSpeed;
    

    private Rigidbody _body;
    private Vector3 _inputsMove = Vector3.zero;
    private Vector3 _inputsRot = Vector3.zero;

    
    public bool _isGrounded = true;
    private Transform _groundChecker;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _groundChecker = transform.GetChild(0);
    }

    void Update()
    {
        _isGrounded = Physics.CheckSphere(_groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore);


        _inputsMove = Vector3.zero;
        _inputsRot = Vector3.zero;

        _inputsRot.x = Input.GetAxis("Horizontal");
        _inputsMove.z = Input.GetAxis("Vertical");
        
       // if (_inputsMove != Vector3.zero)
        //    transform.forward = _inputsMove;

        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _body.AddForce(Vector3.up * Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
        }
        if (Input.GetButtonDown("Dash"))
        {
            Debug.Log("Dash");
            //Vector3 dashVelocity = Vector3.Scale(transform.forward, DashDistance * new Vector3((Mathf.Log(1f / (Time.deltaTime * _body.drag + 1)) / -Time.deltaTime), 0, (Mathf.Log(1f / (Time.deltaTime * _body.drag + 1)) / -Time.deltaTime)));
            _body.AddForce(transform.forward * DashDistance, ForceMode.Impulse);
        }

    }

    private float gravity = 0;

    void FixedUpdate()
    {
        
        transform.Rotate(0, _inputsRot.x*rotationSpeed*Time.deltaTime, 0);
        _body.MovePosition(transform.position + transform.forward * _inputsMove.z * Time.deltaTime * Speed);
        
        // gravity
        if (!_isGrounded)
        {
            gravity += Physics.gravity.y * Time.deltaTime;
            _body.AddForce(transform.up * gravity , ForceMode.Impulse); 

        }
        else
        {
            gravity = 0;
        }

    }
    public void Hit(float force)
    {
        this.GetComponent<Rigidbody>().AddForce(Vector3.back * force, ForceMode.Impulse);
        Debug.Log("hit");
    }
    

}
