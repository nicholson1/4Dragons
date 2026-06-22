using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InputIcons
{
    public class II_Menu : MonoBehaviour
    {

        public static II_Menu mainMenu;
        public static II_Menu currentMenu;
        public bool isMainMenu = false;

        private II_Menu lastMenu;
        public GameObject menuObject;
        public GameObject firstSelected;
        private GameObject lastSelected { get; set; }

        public delegate void OnMainMenuOpened();
        public delegate void OnMainMenuClosed();

        public static OnMainMenuOpened onMainMenuOpened;
        public static OnMainMenuClosed onMainMenuClosed;


        // Start is called before the first frame update
        void Start()
        {
            if (isMainMenu)
                mainMenu = this;

            CloseMenu();
        }

        public static void HandleMenuButtonPressed()
        {
            if (currentMenu == null) //no menu open
            {
                if (mainMenu)
                {
                    mainMenu.OpenMenu();
                    onMainMenuOpened?.Invoke();
                }
                else
                    Debug.LogWarning("No main menu available");
            }
            else
            {
                currentMenu.CloseMenu();
            }


        }

        public void OpenMenu()
        {
            if (currentMenu == this)
                return;

            if (currentMenu)
            {
                lastMenu = currentMenu;
                currentMenu.lastSelected = EventSystem.current.currentSelectedGameObject;
                currentMenu = this;
                lastMenu.CloseMenu();
            }

            menuObject.SetActive(true);
            currentMenu = this;
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        public void CloseMenu()
        {
            menuObject.SetActive(false);

            if (isMainMenu && currentMenu == this)
            {
                onMainMenuClosed?.Invoke();
                currentMenu = null;
            }
            else if (lastMenu && currentMenu == this)
            {
                currentMenu = null;
                lastMenu.OpenMenu();
                if (lastMenu.lastSelected)
                {
                    EventSystem.current.SetSelectedGameObject(lastMenu.lastSelected);
                }
            }

        }

    }
}