using UnityEngine;
using System.Collections;

public class FireProjectile : Projectile {

	public float dampen;
	public float distTraveled;
	public static float fireWidth = 0.1f;

	void FixedUpdate () {
		if (distTraveled > range)
			velocity *= dampen;

		CastSphereRay ();
		transform.position += velocity * Time.fixedDeltaTime;
		distTraveled += velocity.magnitude * Time.fixedDeltaTime;

		if (velocity.magnitude < dampen)
			Destroy (gameObject);
	}

	void CastSphereRay () {
		Ray ray = new Ray (transform.position, transform.right * velocity.magnitude * Time.fixedDeltaTime);
		RaycastHit hit;
		
		if (Physics.SphereCast (ray, fireWidth, out hit, velocity.magnitude * Time.fixedDeltaTime)) {
			
			if (hit.collider.gameObject.layer != parent.layer) {
				
				hit.collider.SendMessage ("OnTakeDamage", new Projectile.Damage (damage, effectiveAgainst), SendMessageOptions.DontRequireReceiver);
				Destroy (gameObject);
				
			}
			
		}
	}
}
