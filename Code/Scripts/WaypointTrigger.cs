using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ChairController chair;
        other.TryGetComponent<ChairController>(out chair);

        if (other.gameObject.CompareTag("Player") && !other.isTrigger && !chair.fallen)
        {
            RaceHandler.instance.HandleCheckpoint(chair, gameObject);
        }
    }
}
