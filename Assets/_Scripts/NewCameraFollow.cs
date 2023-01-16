using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NewCameraFollow : MonoBehaviour
{
    private Transform target;
    public Vector3 target_Offset;
    //[SerializeField] private GameObject camera;
    

    //private bool rotate = true;
    public float rotateSpeed;
    
    private void Awake()
    {
    }

    private void OnDestroy()
    {
    }
   
    private void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        target_Offset = transform.position - target.position;
        //rotateTarget = transform.eulerAngles;
    }
    void Update()
    {


        transform.rotation = target.transform.rotation;
        
        
        if (target)
        {
            transform.position = Vector3.Lerp(transform.position, target.position+target_Offset, 0.05f);
        }
        
        
    }

    
}
