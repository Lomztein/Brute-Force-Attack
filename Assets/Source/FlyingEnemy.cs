using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy {

    public float rotationSpeed;

    public override void Start() {
        base.Start ();
        offset *= 5f;
    }

    public override Vector3 DoMovement() {
        if (pathIndex == path.Length - 1) {
            pathIndex--;
        }

        Vector3 loc = new Vector3 (path [ pathIndex ].x, path [ pathIndex ].y) + offset;
        float dist = Vector3.Distance (thisTransform.position, loc);

        float rotMul = 360f / rotationSpeed;
        if (dist < rotMul * 3f) {
            pathIndex++;
        }

        thisRigidody.MovePosition (Vector3.MoveTowards (thisTransform.position, thisTransform.position + thisTransform.right, speed * Time.fixedDeltaTime * freezeMultiplier));

        Quaternion rot = Quaternion.Euler (0, 0, Angle.CalculateAngle (thisTransform.position, loc));
        thisRigidody.MoveRotation (Quaternion.RotateTowards (thisTransform.rotation, rot, rotationSpeed * Time.fixedDeltaTime));

        return loc;
    }
}
