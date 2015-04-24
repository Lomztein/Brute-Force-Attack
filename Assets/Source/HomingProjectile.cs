using UnityEngine;
using System.Collections;

public class HomingProjectile : Projectile {

	public float turnSpeed;
	public float speed;
	public bool autoFindTarget;

	void Start () {
		speed = velocity.magnitude;
	}

	void FixedUpdate () {

		float startAngle = Angle.CalculateAngle (Vector3.zero, velocity);
		transform.rotation = Quaternion.Euler (0, 0, startAngle);

		if (target)
			transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (0,0, Angle.CalculateAngle (transform, target)), turnSpeed * Time.fixedDeltaTime);

		velocity = transform.right * speed;
		CastRay ();
		transform.position += velocity * Time.fixedDeltaTime;
	}
}
