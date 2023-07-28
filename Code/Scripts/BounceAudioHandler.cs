using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceAudioHandler : MonoBehaviour
{

    public bool active = false;

    private AudioSource source;

    private void OnEnable()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (active)
        {
            source.PlayOneShot(AudioManager.instance.PlayRandomJugBounce());
        }
    }
}
