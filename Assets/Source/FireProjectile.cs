using UnityEngine;
using System.Collections;

public class FireProjectile : Projectile {

	public float dampen;
	public float distTraveled;
	public static float fireWidth = 0.1f;

    public ParticleSystem fireParticle;
    public float fireGrowthRate;

    public override void Initialize () {
        base.Initialize ();
        ParticleSystem.ShapeModule shape = fireParticle.shape;
        shape.radius = 0.01f;
    }

    void FixedUpdate () {
		if (distTraveled > range)
			velocity *= dampen;

		CastSphereRay ();
		transform.position += velocity * Time.fixedDeltaTime;
		distTraveled += velocity.magnitude * Time.fixedDeltaTime;

		if (velocity.magnitude < dampen) {
			ReturnToPool ();
			distTraveled = 0f;
		}

        ParticleSystem.ShapeModule shape = fireParticle.shape;
        shape.radius += fireGrowthRate * Time.fixedDeltaTime;
	}

	void CastSphereRay () {
		Ray ray = new Ray (transform.position, transform.right * velocity.magnitude * Time.fixedDeltaTime * 2f);
		RaycastHit hit;
		
		if (Physics.SphereCast (ray, fireWidth, out hit, velocity.magnitude * Time.fixedDeltaTime * 2f)) {
			if (ShouldHit (hit)) {
				OnHit (hit.collider, hit.point, transform.right);
                damage /= 5;
			}
		}
	}
}
