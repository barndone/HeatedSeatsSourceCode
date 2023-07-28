using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachineTurret : MonoBehaviour
{
    [Tooltip("The gameobject used to represent the position that the cans will be fired from.")]
    [SerializeField] GameObject firingPoint;
    [Tooltip("The radius around the firing points position (random range)")]
    [SerializeField] float firingRadius;
    [Tooltip("The force multiplier added to the cans on instantiation.")]
    [SerializeField] float launchMultiplier;
    [Tooltip("The minimum number of cans the vending machine will fire after getting hit DO NOT MAKE MORE THAN MAX CANS AT ONCE OR I WILL FIND YOU.")]
    [SerializeField] [Range(1, 99)] int minCansAtOnce = 1;
    [Tooltip("The maximum number of cans the vending machine will fire after getting hit.")]
    [SerializeField] [Range(1, 100)] int maxCansAtOnce = 3;
    [Tooltip("The delay between cans being fired.")]
    [SerializeField] float delay = 1.0f;

    //  the timer keeping track of delay progress
    float timer;
    [Tooltip("The prefab representing the can.")]
    [SerializeField] GameObject canPrefab;

    //  internal tracker for number of cans fired
    int numberLaunched = 0;
    //  number the vending machine will fire
    int numberToLaunch;
    //  tracks wether this is firing or not
    bool firing = false;

    [Tooltip("The mass of the cans fired by the vending machine-- increase to allow for the cans to affect the chair more!")]
    [SerializeField] float canMass = 0.005f;

    private AudioSource source;

    private void OnEnable()
    {
        source = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Extinguisher"))
        {
            Debug.Log("vending machine smackaronied");

            if (!firing)
            {
                numberToLaunch = GenerateNumberToFire();
                firing = true;
            }
        }
    }

    private void Update()
    {
        if (firing)
        {
            if (numberLaunched < numberToLaunch)
            {
                if (timer >= delay)
                {
                    FireCan();

                    timer = 0.0f;
                }

                else
                {
                    timer += Time.deltaTime;
                }
            }
        }
    }

    private int GenerateNumberToFire()
    {
        return Random.Range(minCansAtOnce, maxCansAtOnce);
    }

    //  Fire a can-
    //  if the object pool limit has not been reached, instantiate a prefab, then apply a force to that prefab. Add it to the object pool
    //  if the limit has been reached, get the first value in the object pool, remove it from the queue, recycle it, and then re-add it to the queue!
    private void FireCan()
    {
        Rigidbody canRB;

        if (!GameManager.instance.stopSpawning)
        {
            var can = Instantiate(canPrefab, new Vector3((firingPoint.transform.position.x + Random.Range(-firingRadius, firingRadius)), firingPoint.transform.position.y, firingPoint.transform.position.z), transform.parent.rotation);
            GameManager.instance.spawnedCans.Enqueue(can);
            canRB = can.GetComponentInChildren<Rigidbody>();

            if (GameManager.instance.spawnedCans.Count == GameManager.instance.MAX_SPAWNED_CANS)
            {
                GameManager.instance.stopSpawning = true;
                Debug.Log("Can limit reached");
            }
        }
        else
        {
            var can = GameManager.instance.spawnedCans.Dequeue();
            canRB = can.GetComponentInChildren<Rigidbody>();
            canRB.Sleep();
            can.transform.SetPositionAndRotation(new Vector3(firingPoint.transform.position.x + Random.Range(-firingRadius, firingRadius), firingPoint.transform.position.y, firingPoint.transform.position.z), transform.parent.rotation);
            canRB.transform.SetPositionAndRotation(new Vector3(firingPoint.transform.position.x + Random.Range(-firingRadius, firingRadius), firingPoint.transform.position.y, firingPoint.transform.position.z), transform.parent.rotation);
            GameManager.instance.spawnedCans.Enqueue(can);
        }

        source.PlayOneShot(AudioManager.instance.canLaunched);
        canRB.mass = canMass;
        canRB.WakeUp();
        canRB.AddForce(transform.forward * launchMultiplier);
        numberLaunched++;

        if (numberLaunched == numberToLaunch)
        {
            firing = false;

            numberLaunched = 0;
        }
    }
}
