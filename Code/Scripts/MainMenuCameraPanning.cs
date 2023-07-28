using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainMenuCameraPanning : MonoBehaviour
{
    public GameObject defaultTarget;
    public List<GameObject> cameraTargets = new List<GameObject>();
    public List<CinemachineSmoothPath> paths = new List<CinemachineSmoothPath>();

    //  field representing how fast the camera will move along the path
    public float cameraSpeed = 2.0f;

    //  keeps track of which target / path the camera should be on
    private int pathIndex;

    private CinemachineVirtualCamera vCam;
    private bool pathing =false;
    private bool pathingStart = false;
    private bool returnPath = false;

    private GameObject target;

    private CinemachineTrackedDolly dolly;

    private void OnEnable()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        dolly = vCam.GetCinemachineComponent<CinemachineTrackedDolly>();
        target = defaultTarget;
        vCam.LookAt = target.transform;
    }

    private void Update()
    {
        if (pathing)
        {
            if (returnPath)
            {
                returnPath = false;
            }
            if (pathingStart)
            {
                target = cameraTargets[pathIndex];
                dolly.m_Path = paths[pathIndex];
                vCam.LookAt = target.transform;

                pathingStart = false;
            }

            dolly.m_PathPosition += Time.deltaTime;

            if (dolly.m_PathPosition >= 1.0f)
            {
                pathing = false;
            }
        }

        if (returnPath)
        {
            if (pathing)
            {
                pathing = false;
            }

            if (pathingStart)
            {
                target = defaultTarget;
                vCam.LookAt = target.transform;
                pathingStart = false;
            }

            dolly.m_PathPosition -= Time.deltaTime;

            if (dolly.m_PathPosition <= 0.0f)
            {
                returnPath = false;
            }
        }
    }

    public void TrackSwap(int i)
    {
        pathIndex = i;
        pathing = true;
        pathingStart = true;
    }

    public void ReturnTrack()
    {
        returnPath = true;
        pathingStart = true;
    }
}
