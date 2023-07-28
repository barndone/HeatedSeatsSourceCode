using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IgnoreMeshCollider : MonoBehaviour
{
    [SerializeField] Collider rbCollider;
    [SerializeField] GameObject meshRef;

    private void Start()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("SampleScene"))
        {
            meshRef = gameObject;

            if (meshRef.CompareTag("CharacterMesh"))
            {
                rbCollider = meshRef.transform.root.GetComponentInChildren<ChairController>().rb.gameObject.GetComponent<Collider>();
            }

            Collider[] colliders = meshRef.GetComponentsInChildren<Collider>();

            foreach (Collider meshCollider in colliders)
            {
                Physics.IgnoreCollision(rbCollider, meshCollider, true);
            }
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("SampleScene"))
        {
            if (!gameObject.CompareTag("CharacterMesh"))
            {
                meshRef.transform.position = rbCollider.transform.position;

                meshRef.transform.rotation = Quaternion.Euler(0, rbCollider.gameObject.transform.localRotation.eulerAngles.y, 0);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("SampleScene"))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var rb = rbCollider.gameObject.GetComponent<Rigidbody>();

                rb.AddForceAtPosition(-collision.impulse,
                    new Vector3(collision.transform.localPosition.x, rb.transform.position.y, rb.transform.position.z),
                    ForceMode.Impulse);
            }
        }
    }
}
