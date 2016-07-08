using UnityEngine;
using System.Collections;

public class SawmillSaw : Projectile {

	public float attackWaitTime;
	public float speed;
	public float dampen;
	public float width;

	private float maxEmmisionSpeed = 500;
	public float direction;
	public TrailRenderer trailRenderer;

	// Use this for initialization
	void Start () {
		StartCoroutine (Attack ());
	}

	IEnumerator Attack () {
		float charge = 0f;
		while (charge < attackWaitTime) {
            Utility.ChangeParticleEmmisionRate (particle, Mathf.Lerp (0, maxEmmisionSpeed, charge / attackWaitTime));
			charge += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate ();
		}

		float travel = 0f;
		while (travel < range) {
			transform.position += transform.rotation * Vector3.right * speed * Time.fixedDeltaTime;
			CastRay (speed);
			travel += Time.fixedDeltaTime * speed;
			yield return new WaitForFixedUpdate ();
		}

		float remainingSpeed = speed;
		while (remainingSpeed > 0.2f) {
			CastRay (remainingSpeed);

            ParticleSystem.EmissionModule em = particle.emission;
            float rate = Mathf.Lerp (0, maxEmmisionSpeed, remainingSpeed / speed);
            em.rate = new ParticleSystem.MinMaxCurve (rate);
			trailRenderer.startWidth = Mathf.Lerp (0, 1, remainingSpeed / speed);
			transform.position += transform.rotation * Vector3.right * remainingSpeed * Time.fixedDeltaTime;
			remainingSpeed *= dampen;
			yield return new WaitForFixedUpdate ();
		}

		Destroy (gameObject, 0.5f);
	}

	void CastRay (float speed) {
		Ray ray = new Ray (transform.position, transform.rotation * Vector3.right);
		RaycastHit[] hits = Physics.SphereCastAll (ray, width, speed * Time.fixedDeltaTime);
		Debug.Log (hits.Length);
		Debug.DrawLine (ray.origin, ray.origin + ray.direction * speed * Time.fixedDeltaTime, Color.white, 1f);
		for (int i = 0; i < hits.Length; i++) {
			hits[i].collider.SendMessage ("OnTakeDamage", new Projectile.Damage (damage, Colour.Yellow, null), SendMessageOptions.DontRequireReceiver);
		}
	}
}
