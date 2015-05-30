using UnityEngine;
using System.Collections;

public class RailgunProjectile : Projectile {

	public float spherecastWidth;

	void FixedUpdate () {
		CastSphereRay ();
		transform.position += velocity * Time.fixedDeltaTime;
	}

	public void CastSphereRay () {
		Ray ray = new Ray (transform.position, transform.right * velocity.magnitude * Time.fixedDeltaTime);
		RaycastHit hit;
		
		if (Physics.SphereCast (ray, spherecastWidth, out hit, velocity.magnitude * Time.fixedDeltaTime)) {
			
			if (hit.collider.gameObject.layer != parent.layer) {
				
				hit.collider.SendMessage ("OnTakeDamage", new Projectile.Damage (damage, effectiveAgainst), SendMessageOptions.DontRequireReceiver);
				if (hitParticle) Destroy ((GameObject)Instantiate (hitParticle, hit.point, transform.rotation), 1f);
				ReturnToPool ();
				
			}
			
		}
	}

}
