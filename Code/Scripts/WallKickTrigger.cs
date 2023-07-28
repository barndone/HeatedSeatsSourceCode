using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallKickTrigger : MonoBehaviour
{
    [Tooltip("The delay between checking wall collisions.Default to 1.0")]
    [SerializeField] float TickDelay = 1.0f;
    private float tickTime;

    private void OnTriggerEnter(Collider other)
    {

        //  check if the object that entered this collision contains a chair controller component
        if (other.gameObject.GetComponent<ChairController>() != null)
        {
            //Debug.Log(other.gameObject.name + "can wall kick");

            ChairController chair = other.gameObject.GetComponent<ChairController>();

            //  chair.wallKick = true;
            Vector3 approach = chair.rb.velocity;

            //  raycast check for finding the normal of the wall this chair is close to
            RaycastHit hit;

            List<RaycastHit> wallHits = new List<RaycastHit>();

            //  iterate through the step count of the chair
            for (int i = 0; i < chair.stepCount; i++)
            {
                //  get the raycast direction based off of the angle step (360 / step count) * the index
                Vector3 hitDir = Quaternion.Euler(0, chair.angleStep * i, 0) * approach;

                //  if we have a hit:
                if (Physics.Raycast(other.gameObject.transform.position, hitDir * 1f, out hit))
                {
                    //  check if this object is the one being hit
                    if (hit.transform.gameObject == this.gameObject)
                    {
                        //  add this hit to the list
                        wallHits.Add(hit);
                    }
                }
            }

            //  if the wallHits list count is greater than or equal to 1
            if (wallHits.Count >= 1)
            {
                //  sort the list based off of shortest distance (closest wall)
                var sortedHits = wallHits.OrderBy(x => x.distance);

                //  sorted list -> [0] should be the closest wall
                wallHits = sortedHits.ToList<RaycastHit>();

                //  if the distance isn't 0
                if (chair.wallDist != 0.0f)
                {
                    //  check if the distance of this hit is less than or equal to the distance already cached
                    if (wallHits[0].distance <= chair.wallDist)
                    {
                        //  replace the distance and approach normal
                        chair.wallDist = wallHits[0].distance;
                        chair.approachNormal = wallHits[0].normal;
                        //  if so, draw a ray!
                        //Debug.DrawRay(chair.transform.position, wallHits[0].point, Color.green, 1.0f);
                    }
                }

                //  otherwise, it's 0-> assign values
                else
                {
                    chair.wallDist = wallHits[0].distance;
                    chair.approachNormal = wallHits[0].normal;
                    //  if so, draw a ray!
                    //Debug.DrawRay(chair.transform.position, wallHits[0].point, Color.green, 1.0f);
                }

                Vector3 point = wallHits[0].point;
                Vector3 pivot = chair.transform.position;
                Quaternion rot = Quaternion.Euler(0, 180, 0);

                Vector3 pointRotatedAroundPivot = rot * (point - pivot) + pivot;
                // chair.mesh.transform.LookAt(pointRotatedAroundPivot);
                
                chair.lookAtPos = pointRotatedAroundPivot;
                chair.facingWall = true;

                //chair.meshCounterRotation.transform.Rotate(new Vector3(0, 180, 0));
            }
        }
    }

    //  commented out to test performance -Brandon

    //  private void OnTriggerStay(Collider other)
    //  {
    //      if (other.gameObject.GetComponent<ChairController>() != null)
    //      {
    //  
    //          ChairController chair = other.gameObject.GetComponent<ChairController>();
    //          Vector3 approach = chair.rb.velocity;
    //  
    //          //  raycast check for finding the normal of the wall this chair is close to
    //          RaycastHit hit;
    //  
    //          List<RaycastHit> wallHits = new List<RaycastHit>();
    //  
    //          //  iterate through the step count of the chair
    //          for (int i = 0; i < chair.stepCount; i++)
    //          {
    //              //  get the raycast direction based off of the angle step (360 / step count) * the index
    //              Vector3 hitDir = Quaternion.Euler(0, chair.angleStep * i, 0) * approach;
    //  
    //              //  if we have a hit:
    //              if (Physics.Raycast(other.gameObject.transform.position, hitDir * 1f, out hit))
    //              {
    //                  //  check if this object is the one being hit
    //                  if (hit.transform.gameObject == this.gameObject)
    //                  {
    //                      //  add this hit to the list
    //                      wallHits.Add(hit);
    //                  }
    //              }
    //          }
    //  
    //          //  if the wallHits list count is greater than or equal to 1
    //          if (wallHits.Count >= 1)
    //          {
    //              //  sort the list based off of shortest distance (closest wall)
    //              var sortedHits = wallHits.OrderBy(x => x.distance);
    //  
    //              //  sorted list -> [0] should be the closest wall
    //              wallHits = sortedHits.ToList<RaycastHit>();
    //  
    //              //  if the distance isn't 0
    //              if (chair.wallDist != 0.0f)
    //              {
    //                  //  check if the distance of this hit is less than or equal to the distance already cached
    //                  if (wallHits[0].distance <= chair.wallDist)
    //                  {
    //                      //  replace the distance and approach normal
    //                      chair.wallDist = wallHits[0].distance;
    //                      chair.approachNormal = wallHits[0].normal;
    //                      //  if so, draw a ray!
    //                      //Debug.DrawRay(chair.transform.position, wallHits[0].point, Color.green, 1.0f);
    //                  }
    //              }
    //  
    //              //  otherwise, it's 0-> assign values
    //              else
    //              {
    //                  chair.wallDist = wallHits[0].distance;
    //                  chair.approachNormal = wallHits[0].normal;
    //                  //  if so, draw a ray!
    //                  //Debug.DrawRay(chair.transform.position, wallHits[0].point, Color.green, 1.0f);
    //              }
    //  
    //              //  //  rob mentioned this may introduce errors, trying different approach
    //              //  var angleToWall = Quaternion.LookRotation(wallHits[0].point, chair.transform.position);
    //              //  Vector3 rot = angleToWall.eulerAngles;
    //              //  rot = new Vector3(rot.x, rot.y + 180, rot.z);
    //              //  angleToWall = Quaternion.Euler(rot);
    //  
    //  
    //              //  rotate the gameobject containing the meshes to look at the world space position of the direction of the approach normal
    //              //chair.mesh.transform.LookAt(wallHits[0].point);
    //              //chair.mesh.transform.LookAt(chair.meshPointer.transform.position);
    //  
    //              Vector3 point = wallHits[0].point;
    //              Vector3 pivot = chair.transform.position;
    //              Quaternion rot = Quaternion.Euler(0, 180, 0);
    //  
    //              Vector3 pointRotatedAroundPivot = rot * (point - pivot) + pivot;
    //              /// chair.mesh.transform.LookAt(pointRotatedAroundPivot);
    //  
    //              chair.lookAtPos = pointRotatedAroundPivot;
    //              chair.facingWall = true;
    //  
    //              //chair.meshCounterRotation.transform.Rotate(new Vector3(0, 180, 0));
    //          }
    //      }
    //  }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<ChairController>() != null)
        {
           // Debug.Log(other.gameObject.name + "can't wall kick");

            ChairController colChair = other.gameObject.GetComponent<ChairController>();

            //  colChair.wallKick = false;
            //  colChair.wallKicked= false;
            colChair.wallDist = 0.0f;

            colChair.facingWall = false;
            colChair.lookAtPos = Vector3.zero;
            //colChair.mesh.transform.LookAt(colChair.forward);

            colChair.approachNormal = Vector3.zero;
            //colChair.meshCounterRotation.transform.Rotate(new Vector3(0, 180, 0));
        }
    }
}
