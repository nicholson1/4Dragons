using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginController : MonoBehaviour
{
    [SerializeField] private GameObject login;
    [SerializeField] private GameObject register;

    public void LoadLogin()
    {
        login.SetActive(true);
        register.SetActive(false);

    }
    public void LoadRegister()
    {
        login.SetActive(false);
        register.SetActive(true);
    }
}
