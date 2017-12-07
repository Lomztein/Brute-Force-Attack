using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingEnemy : Enemy {

    public float rotationSpeed = 0.2f;

    public override Vector3 DoMovement() {
        Vector3 loc = base.DoMovement ();

        Quaternion rot = Quaternion.Euler (0, 0, Angle.CalculateAngle (thisTransform.position, loc));
        thisRigidody.MoveRotation (Quaternion.Lerp (thisTransform.rotation, rot, rotationSpeed * Time.fixedDeltaTime));
        return loc;
    }
}
