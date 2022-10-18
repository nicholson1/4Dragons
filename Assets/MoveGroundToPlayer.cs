using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGroundToPlayer : MonoBehaviour
{
    public GameObject player;

    private void Update()
    {
        if (Vector3.Distance(player.transform.position, this.transform.position) >20)
        {
            this.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        }
    }
}
