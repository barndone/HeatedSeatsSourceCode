using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceHandler : MonoBehaviour
{
    /// <summary>
    /// The list of waypoints contained within this map, ensure they are ordered in the inspector according to their position along the race.
    /// The final checkpoint (starting line / lap marker) should be the LAST waypoint in the list.
    /// </summary>
    [Tooltip("The list of waypoints contained within this map, ensure they are ordered in the inspector according to their position along the race.\nThe final checkpoint (starting line / lap marker) should be the LAST waypoint in the list.")]
    public List<GameObject> waypoints = new List<GameObject>();

    /// <summary>
    /// The number of laps for this race
    /// </summary>
    public int laps = 3;
    public RenderTexture minimap;

    static public RaceHandler instance;

    private void Awake()
    {
        //  if there is currently no instance
        if (!instance)
        {
            //  assign the instance to this
            instance = this;
        }

        //  otherwise:
        else
        {
            //  we already have an instance
            Debug.LogWarning("ERROR: Second Race Handler Found in Scene");
            //  destroy the object this script is attached to
            Destroy(gameObject);
        }
    }

    //  this method will handle updating the fields of the chair after entering the checkpoint
    public void HandleCheckpoint(ChairController chair, GameObject waypoint)
    {
        //  if the active waypoint index is equal to the waypoint crossed by this chair
        if (waypoints.IndexOf(waypoint) == chair.activeWaypointIndex)
        {
            //  increase the active waypoint index
            chair.activeWaypointIndex++;
            Debug.Log("Active Checkpoint: " + chair.activeWaypointIndex);


            //  if the index is greater than the amount of waypoints in the waypoint list
            if (chair.activeWaypointIndex >= waypoints.Count)
            {
                Debug.Log("Active Checkpoint: " + chair.activeWaypointIndex);
                chair.activeWaypointIndex = 0;
                chair.lapCounter++;
                Debug.Log("Current Lap: " + chair.lapCounter);
                AudioManager.instance.PlayLapSound();
            }

            if (chair.lapCounter >= laps)
            {
                //float time;
                //GameManager.instance.completionTimes.TryGetValue(chair, out time);
                Debug.Log(chair.gameObject.name + " finished in " + GameManager.instance.raceTimer);     
            }
        }

        //  if the active waypoint index is different than the waypoint crossed by this chair
        //  indicate they are going the wrong way!
        else
        {
            if (chair.previousWaypoint <= waypoints.IndexOf(waypoint))
            {
                chair.wrongWayIndicator.SetActive(false);
                Debug.Log("Going the right way");
            }

            else
            {
                chair.wrongWayIndicator.SetActive(true);
                Debug.Log("Wrong Way");
            }
        }
       

        chair.previousWaypoint = waypoints.IndexOf(waypoint);
    }

    private void Update()
    {
        minimap.Release();
    }
}
