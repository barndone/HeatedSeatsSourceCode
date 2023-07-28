using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [SerializeField] float boostForce = 30.0f;
    [SerializeField] float stoolBoostMultiplier = 1.5f;
    [SerializeField] float officeBoostMultiplier = 1.5f;
    [SerializeField] float gamingBoostMultiplier = 1.5f;
    [SerializeField] float loungeBoostMultiplier = 1.5f;
    [SerializeField] GameObject directionPointer;
    private Vector3 boostDirection;

    private void OnEnable()
    {
        //  calculate direction given terminal point (pointer) and starting point (transform) and then normalize it to get a final direction vector
        boostDirection = (directionPointer.transform.position - transform.position).normalized;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.CompareTag("Player"))
        {
            //  get a reference to the chair
            var chair = other.GetComponent<ChairController>();
            //  get a reference to the rigidbody
            var rb = chair.rb;

            //  if the rigidbody isn't null
            if (rb != null)
            {
                //  apply force based off of which type of chair it is
                if (chair.chairIndex == 0)
                {
                    //  time to BOOST
                    rb.AddForce(boostDirection * boostForce * stoolBoostMultiplier, ForceMode.Impulse);
                }
                if (chair.chairIndex == 1)
                {
                    //  time to BOOST
                    rb.AddForce(boostDirection * boostForce * officeBoostMultiplier, ForceMode.Impulse);
                }
                if (chair.chairIndex == 2)
                {
                    //  time to BOOST
                    rb.AddForce(boostDirection * boostForce * gamingBoostMultiplier, ForceMode.Impulse);
                }
                if (chair.chairIndex == 3)
                {
                    //  time to BOOST
                    rb.AddForce(boostDirection * boostForce * loungeBoostMultiplier, ForceMode.Impulse);
                }

                Debug.Log("Boosted " + other.name);
            }
        }
    }
}
