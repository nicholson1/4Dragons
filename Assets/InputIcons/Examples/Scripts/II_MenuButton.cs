using UnityEngine;
using UnityEngine.SceneManagement;

namespace InputIcons
{
    public class II_MenuButton : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        public void QuitApplication()
        {
            Application.Quit();
        }

        public void LoadScene(string name)
        {
            SceneManager.LoadScene(name);
        }

        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void CloseMenu()
        {
            GetComponentInParent<II_Menu>().CloseMenu();
        }
    }
}
