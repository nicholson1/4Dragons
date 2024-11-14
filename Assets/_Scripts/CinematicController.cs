using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicController : MonoBehaviour
{
    [SerializeField] private GameObject CameraTarget;
    [SerializeField] private Camera _camera;

    public static CinematicController _instance;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private bool canActivate = true; // Cooldown state
    private float cooldownTime = 1f; // Cooldown duration in seconds

    void Update()
    {
        // Check for Ctrl + Shift + R key press
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.H))
        {
            if (canActivate)
            {
                ActivateObject();
                StartCoroutine(Cooldown());
            }
        }
        if(Input.GetKey(KeyCode.C))
        {
            
        }
    }

    private void ActivateObject()
    {
        if (CameraTarget != null)
        {
            CameraTarget.SetActive(true);
            UIController._instance.CinemaUI.gameObject.SetActive(true);
            //Debug.Log("Object activated!");
        }
        
    }

    private IEnumerator Cooldown()
    {
        canActivate = false;
        yield return new WaitForSeconds(cooldownTime);
        canActivate = true;
    }
}
