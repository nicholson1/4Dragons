using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputIcons
{
    public class II_SwitchRebindProfile : MonoBehaviour
    {

        public int profileIdToSwitchTo = 0;

        public void SwitchToProfile()
        {
            InputIconsManagerSO.SwitchToPlayerProfile(profileIdToSwitchTo);
        }
    }
}
