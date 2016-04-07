using UnityEngine;
using System.Collections;

public class HomingProjectile : Projectile {

	public float turnSpeed;
	public float speed;
	public static bool autoFindTarget;
    public bool slowWhenTurning;

	public override void Initialize () {
		speed = velocity.magnitude;
	}

	public virtual void FixedUpdate () {

		float startAngle = Angle.CalculateAngle (Vector3.zero, velocity);
		transform.rotation = Quaternion.Euler (0, 0, startAngle);

        if (target && !target.gameObject.activeSelf)
            target = null;

        float rotateMul = 1f;
        if (target && slowWhenTurning)
            rotateMul = (1f + Mathf.Max (1-Vector3.Dot (velocity.normalized, (target.position - transform.position).normalized), 0f));

        if (target)
			transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (0,0, Angle.CalculateAngle (transform, target)), turnSpeed * Time.fixedDeltaTime * rotateMul);

        velocity = transform.right * speed;

        CastRay ();
		transform.position += velocity * Time.fixedDeltaTime;

        if (autoFindTarget && parentWeapon && parentWeapon.target)
            target = parentWeapon.target;
	}

}