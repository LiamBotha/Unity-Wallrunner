using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            player.CurrentCheckpoint = this.transform;
        }
    }
}
