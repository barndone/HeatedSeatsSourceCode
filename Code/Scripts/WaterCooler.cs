using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCooler : MonoBehaviour
{
    [SerializeField] Rigidbody jugRB;

    [SerializeField] float impactForce = 25.0f;

    void Awake()
    {
        jugRB.Sleep();    
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Extinguisher"))
        {
            Debug.Log("This water cooler got smACKED");

            //  cache the chair controller
            ChairController controller = collision.gameObject.GetComponent<ChairController>();

            this.transform.DetachChildren();

            jugRB.WakeUp();
            jugRB.AddForce(controller.rb.velocity * -impactForce);

            jugRB.gameObject.GetComponent<BounceAudioHandler>().active = true;

            Destroy(this);
        }
    }
}
