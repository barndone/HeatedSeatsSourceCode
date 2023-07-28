using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisher : MonoBehaviour
{
    Rigidbody rb;

    public bool flying = false;
    [SerializeField] List<GameObject> firingPosition = new List<GameObject>();
    [SerializeField] bool shouldStop = true;

    [SerializeField] float airTime = 6.0f;
    [SerializeField] float forceMultiplier = 5f;

    [SerializeField] float pointDelay = 0.5f;
    float pointTimer = 0.0f;
    
    float timer = 0.0f;

    int pointIndex;

    [SerializeField] ParticleSystem particles;
    private AudioSource source;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.Sleep();

        var main = particles.main;
        main.loop = !shouldStop;
    }

    private void OnEnable()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("CharacterMesh") || collision.gameObject.CompareTag("Extinguisher"))
        {
            Debug.Log("bonk");

            rb.WakeUp();
            flying = true;
            pointIndex = Random.Range(0, firingPosition.Count);
            particles.Play();

            source.Play();
        }

        source.PlayOneShot(AudioManager.instance.PlayRandomExtinguisherHit());
    }

    private void FixedUpdate()
    {
        if (flying)
        {
            if (shouldStop)
            {
                if (timer <= airTime)
                {
                    timer += Time.deltaTime;

                    if (pointTimer <= pointDelay)
                    {
                        rb.AddForceAtPosition(-rb.transform.up * forceMultiplier, firingPosition[pointIndex].transform.position, ForceMode.Force);
                    }

                    else
                    {
                        pointTimer = 0.0f;
                        pointIndex = Random.Range(0, firingPosition.Count);
                    }

                }

                else
                {
                    flying = false;
                    timer = 0.0f;
                    particles.Stop();
                    source.Stop();
                }
            }
            else
            {
                if (pointTimer <= pointDelay)
                {
                    rb.AddForceAtPosition(-rb.transform.up * forceMultiplier, firingPosition[pointIndex].transform.position, ForceMode.Force);
                }

                else
                {
                    pointTimer = 0.0f;
                    pointIndex = Random.Range(0, firingPosition.Count);
                }
            }
        }
    }
}
