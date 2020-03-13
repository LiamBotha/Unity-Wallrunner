using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag.Equals("Player"))
        {
            var player =other.GetComponent<PlayerController>();
            player.Death();
        }
    }
}
