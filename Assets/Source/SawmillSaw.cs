using UnityEngine;
using System.Collections;

public class SawmillSaw : MonoBehaviour {

	public float attackWaitTime;
	public float speed;
	public float range;
	public float dampen;
	public int damage;
	public float width;

	public ParticleSystem particle;
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
			particle.emissionRate = Mathf.Lerp (0, maxEmmisionSpeed, charge / attackWaitTime);
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
			particle.emissionRate = Mathf.Lerp (0, maxEmmisionSpeed, remainingSpeed / speed);
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
		Debug.DrawLine (ray.origin, ray.origin + ray.direction * speed * Time.fixedDeltaTime, Color.white, 1f);
		for (int i = 0; i < hits.Length; i++) {
			hits[i].collider.SendMessage ("OnTakeDamage", new Projectile.Damage (damage, Colour.Yellow), SendMessageOptions.DontRequireReceiver);
		}
	}
}
