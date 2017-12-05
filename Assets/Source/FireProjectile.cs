using UnityEngine;
using System.Collections;

public class FireProjectile : Projectile {

	public float dampen;
	public float distTraveled;

	public static float fireWidth = 0.5f;

    public float timeTraveled;

    public ParticleSystem fireParticle;
    public float fireGrowthRate;

    public override void Initialize () {
        base.Initialize ();
        ParticleSystem.ShapeModule shape = fireParticle.shape;
        timeTraveled = 0f;
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
        timeTraveled += Time.fixedDeltaTime;
        shape.radius = GetGrowth ();
	}

    private float GetGrowth() {
        return timeTraveled * fireGrowthRate;
    }

	void CastSphereRay () {
		Ray ray = new Ray (transform.position, transform.right * velocity.magnitude * Time.fixedDeltaTime * 2f);

        RaycastHit [ ] hits = Physics.SphereCastAll (ray, fireWidth * GetGrowth (), velocity.magnitude * Time.fixedDeltaTime * 2f);
        for (int i = 0; i < hits.Length; i++) {
			if (ShouldHit (hits[i])) {
				OnHit (hits [ i ].collider, hits [ i ].point, transform.right);
			}
		}
	}
}
