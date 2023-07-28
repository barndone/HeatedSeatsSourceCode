using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BalanceUtils
{
    //  calculates the relative velocity of another Rigidbody relative to this Rigidbody 
    public static Vector3 CalcRelativeVelocity(Rigidbody thisRB, Rigidbody otherRB)
    {
        return otherRB.velocity - thisRB.velocity;
    }

    //  returns -1 or 1 depending on which side of the chair the collision is happening on
    public static int DetermineSideOfCollision(ChairController thisChair, Vector3 collisionPos)
    {
        //  first- get the direction to the collision: other.pos - this.pos
        Vector3 dirToCollision = collisionPos - thisChair.gameObject.transform.position;

        //  get the dot product of the forward direction of this chair to the direction to the collision:
        float dotResult = Vector3.Dot(thisChair.forward, dirToCollision);
        
        //  check if the vectors are perpendicular to each other (0)
        if (dotResult == 0.0f)
        {
            //  if so, let's get the right of the fwd and get compare the dot product of the right to the direction to the collision
            Vector3 rightOfFwd = Vector3.Cross(thisChair.forward, thisChair.rotator.transform.up);

            //  re-calculate the dot product result
            dotResult = Vector3.Dot(rightOfFwd, dirToCollision);
        }

        //  if our dot product result is less than 0, it is on the left side
        if (dotResult < 0)
        {
            return 1;
        }

        //  otherwise it is positive, so on the right side
        else
        {
            return -1;
        }
    }
}
