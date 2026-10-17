using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public class II_InteractShowcase : MonoBehaviour
    {

        public GameObject interactPrefab;
        private GameObject currentObject;

        public Transform spawnPosition;


        void Start()
        {

        }

        void Update()
        {

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<II_LocalMultiplayerPlayerID>() != null)
            {
                if (currentObject != null)
                {
                    Destroy(currentObject);
                }

                int playerID = collision.GetComponent<II_LocalMultiplayerPlayerID>().playerID;

                currentObject = Instantiate(interactPrefab, spawnPosition);
                currentObject.GetComponent<II_LocalMultiplayerSpritePrompt>().spritePromptDatas[0].playerID = playerID;
                currentObject.GetComponent<II_LocalMultiplayerSpritePrompt>().UpdateDisplayedSprites();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (currentObject != null)
            { Destroy(currentObject); }
        }
    }
}
