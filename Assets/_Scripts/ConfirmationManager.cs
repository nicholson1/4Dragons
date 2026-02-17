using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationManager : MonoBehaviour
{
    public static ConfirmationManager _instance;

    public GameObject RestartText;
    public GameObject QuitText;
    public GameObject Background;
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

    public void OpenConfirmation(ConfirmationType type)
    {
        switch (type)
        {
            case ConfirmationType.Restart:
                RestartText.SetActive(true);
                break;
            case ConfirmationType.Quit:
                QuitText.SetActive(true);
                break;
            case ConfirmationType.None:
                Debug.LogWarning("No confirmation type given");
                return;
        }  
        Background.SetActive(true);
    }

    public void CloseConfirmation()
    {
        QuitText.SetActive(false);
        RestartText.SetActive(false);
        Background.SetActive(false);
    }
}

public enum ConfirmationType
{
    Restart,
    Quit,
    None,
}
