using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    public class II_ChangePlayerInput : MonoBehaviour
    {

        public PlayerInput playerInput;

        public GameObject playerObj1;
        public GameObject playerObj2;

        private int currentID = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void ChangeInput()
        {

            if (currentID == 0)
            {
                currentID = 1;

                playerObj1.SetActive(false);
                playerObj2.SetActive(true);

                playerObj1.GetComponent<II_Player>().EnableHelicopterControls();
            }
            else
            {
                currentID = 0;

                playerObj1.SetActive(true);
                playerObj2.SetActive(false);

                playerObj2.GetComponent<II_Player>().EnablePlatformerControls();
            }

        }
    }
}
