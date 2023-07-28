using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    [SerializeField] ChairController chaseTarget;
    [SerializeField] Vector3 followOffset;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = chaseTarget.transform.position + followOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if (chaseTarget == null)
        {
            return;
        }
        else
        {
            if (GameManager.instance.RaceOngoing && !chaseTarget.fallen)
            {
                followOffset = Quaternion.AngleAxis(chaseTarget.aimAction.ReadValue<Vector2>().x * chaseTarget.rotationSpeed * Time.deltaTime, Vector3.up) * followOffset;
                transform.position = chaseTarget.transform.position + followOffset;
                transform.LookAt(chaseTarget.transform.position);
            }
        }
    }
}
