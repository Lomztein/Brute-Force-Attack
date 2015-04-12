using UnityEngine;
using System.Collections;

public class FireProjectile : Projectile {

	public float dampen;

	void FixedUpdate () {
		velocity *= dampen;
		CastRay ();
		transform.position += velocity * Time.fixedDeltaTime;
	}
}
