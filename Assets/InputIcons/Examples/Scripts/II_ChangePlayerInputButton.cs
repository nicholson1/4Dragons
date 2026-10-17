using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public class II_ChangePlayerInputButton : MonoBehaviour
    {
        public void ChangeInput()
        {
#if UNITY_6000_0_OR_NEWER
            FindFirstObjectByType<II_ChangePlayerInput>()?.ChangeInput();
#else
            FindObjectOfType<II_ChangePlayerInput>()?.ChangeInput();
#endif
        }
    }
}

