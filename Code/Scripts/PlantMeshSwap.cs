using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantMeshSwap : MonoBehaviour
{
    [SerializeField] GameObject BrokenPlanter;
    [SerializeField] GameObject Planter;

    [SerializeField] ParticleSystem ps;

    bool hit = false;

    private void OnEnable()
    {
        BrokenPlanter.SetActive(false);
       // ps = gameObject.GetComponent<ParticleSystem>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!hit)
            {
                hit = true;

                BrokenPlanter.SetActive(true);
                ps.Play();
                var chair = collision.gameObject.GetComponent<ChairController>();
                chair.wallsInRange.Remove(Planter);
                Planter.SetActive(false);
            }
        }
    }
}
