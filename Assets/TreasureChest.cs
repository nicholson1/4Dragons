using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TreasureChest : MonoBehaviour
{
    public Transform Lid;
    private bool isOpen = false;
    public float targetRotation = -140f;
    public float rotationSpeed = 5f;
    private Quaternion initialRotation;
    private bool isRotating = false;

    public bool startingChest;
    public int force;
    public bool forceRelic = false;

    [SerializeField] private Button button;
    [SerializeField] private InputHandler inputHandler;
    
    [SerializeField] private AudioClip openChest;
    [SerializeField] private float openChestVol;
    [SerializeField] private AudioClip chestAmbiance;
    [SerializeField] private float chestAmbianceVol;

    public bool testRun = false;

    private void  OnMouseDown()
    {
        ClickOnTreaure();
    }
    
    public void ClickOnTreaure(bool force = false)
    {
        if(!force)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
                
        }        
        
        if(!testRun)
        {
            if (startingChest && !isOpen)
            {
                TutorialManager.Instance.CloseTip(TutorialNames.Start);
                SelectionManager._instance.CreateEquipmentListsStart();
            }

            if (!startingChest && !isOpen)
            {
                SelectionManager._instance.CreateChestReward(forceRelic);
            }

            //Debug.Log(!LootButtonManager._instance.HasItems());
            
            if (!LootButtonManager._instance.HasItems())
            {
                return;
            }
            
            //UIController._instance.ToggleLootUI(1);
            UIController._instance.ToggleInventoryUINew(true, InventoryState.Loot);
        }
        
        if (!isOpen)
        {
            isRotating = true;
            SoundManager.Instance.Play2DSFX(openChest, openChestVol);
        }
        
        
        isOpen = true;
        //StartCoroutine(WaitThenDisable());
    }
    
    private void HandleClickThroughInput()
    {
        if (!isActiveAndEnabled)
            return;

        Debug.Log($"Click through input");
        ClickOnTreaure(true);
    }

    private void HandleClickThroughClick()
    {
        Debug.Log($"Click through event system");
        ClickOnTreaure(true);
    }

    void Start()
    {
        initialRotation = Lid.transform.localRotation;
        GetComponent<Rigidbody>().AddForce(Vector3.down * force,ForceMode.Impulse);
        
        ambience = SoundManager.Instance.PlayAmbience(chestAmbiance, true);
        //add focre down to rigdid body

        inputHandler = EventSystem.current.GetComponent<InputHandler>();
        inputHandler.OnYes.AddListener(HandleClickThroughInput);

        button = GetComponentInChildren<Button>();
        button.onClick.AddListener(HandleClickThroughClick);
    }

    private AudioSource ambience;

    private void OnDisable()
    {
        if(SoundManager.Instance != null)
            SoundManager.Instance.StopAmbience(ambience, 2);
    }

    void Update()
    {
        if (isRotating)
        {
            // Interpolate between the current rotation and the target rotation
            Lid.transform.localRotation = Quaternion.Lerp(Lid.transform.localRotation, Quaternion.Euler(targetRotation, 0, 0), rotationSpeed * Time.deltaTime);
            if (Lid.transform.eulerAngles.x + 1 < targetRotation)
                isRotating = false;
        }

        //if (isActiveAndEnabled)
        //{
        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
        //        Debug.Log("hello?");
        //        ClickOnTreaure(true);
        //    }
        //}
    }

    public void Reset()
    {
        inputHandler.OnYes.RemoveListener(HandleClickThroughInput);
        Lid.transform.SetLocalPositionAndRotation(Lid.transform.localPosition , initialRotation);
        isOpen = false;
        isRotating = false;
        Start();
    }

    private IEnumerator WaitThenDisable()
    {
        yield return new WaitForSeconds(5);
        gameObject.SetActive(false);
    }
}
