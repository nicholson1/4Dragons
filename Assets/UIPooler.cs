using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPooler : MonoBehaviour
{
    public static UIPooler _instance;

    public List<GameObject> StatusTextsPool = new List<GameObject>(); 
    public List<GameObject> IntentPool = new List<GameObject>(); 
    public List<GameObject> buffDebuffPool = new List<GameObject>();

    public List<GameObject> NotificationMessages = new List<GameObject>();





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
}
