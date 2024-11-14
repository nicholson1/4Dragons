using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CinematicController : MonoBehaviour
{
    [SerializeField] private GameObject CameraTarget;
    [SerializeField] private CinemaCamera _camera;
    



    public static CinematicController _instance;

    private bool activated = false;
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
    private float cooldownTime = .25f; // Cooldown duration in seconds

    private void Start()
    {
        _camera.Onzoomspeed(2);
        _camera.Onrotatespeed(100);
    }

    void Update()
    {
        // Check for Ctrl + Shift + R key press
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.H))
        {
            if (canActivate)
            {
                ActivateObject();
                _camera.gameObject.SetActive(activated);
                _camera.Start();
                StartCoroutine(Cooldown());

            }
        }

        // if (Input.GetKey(KeyCode.C))
        // {
        //     if (canActivate && activated)
        //     {
        //         
        //         StartCoroutine(Cooldown());
        //
        //     }
        // }

        if (Input.GetKey(KeyCode.T))
        {
            if (canActivate && activated)
            {
                UIController._instance.CinemaUI.gameObject.SetActive(!UIController._instance.CinemaUI.activeSelf);
                CameraTarget.GetComponent<MeshRenderer>().enabled = UIController._instance.CinemaUI.activeSelf;
                StartCoroutine(Cooldown());
            }
        }
    }

    private void ActivateObject()
    {
        if (CameraTarget != null)
        {
            CameraTarget.SetActive(!activated);
            UIController._instance.CinemaUI.gameObject.SetActive(!activated);
            activated = !activated;
            
            UIController._instance.ToggleUI();
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
