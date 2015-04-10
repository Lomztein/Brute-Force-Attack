using UnityEngine;
using System.Collections;

public class HomingProjectile : Projectile {

	public float turnSpeed;
	public float speed;

	void Start () {
		speed = velocity.magnitude;
	}

	void FixedUpdate () {

		if (target)
			transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (0,0, Angle.CalculateAngle (transform, target)), turnSpeed * Time.fixedDeltaTime);

		velocity = transform.right * speed;
		CastRay ();
		transform.position += velocity * Time.fixedDeltaTime;

	}
}
