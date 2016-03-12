using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public Vector3 velocity;
	public int damage;
	public GameObject parent;
	public float range;
	public Transform target;
	public bool destroyOnTime = false;
	public bool penetrative = false;
	public Weapon parentWeapon;

	public bool hitOnlyTarget;
	public Colour effectiveAgainst;

	public ParticleSystem particle;
    public ParticleSystem hitParticle;
    public int particleCount;

	public float bulletSleepTime = 1f; // Should be synced with particle fadeout time.

	// Update is called once per frame
	public virtual void Initialize () {
		if (particle) particle.Play ();
	}
	
	void FixedUpdate () {

		CastRay ();
		transform.position += velocity * Time.fixedDeltaTime;

	}

	public void CastRay () {
		Ray ray = new Ray (transform.position, transform.right * velocity.magnitude * Time.fixedDeltaTime);
		RaycastHit hit;

		if (!penetrative) {
			if (Physics.Raycast (ray, out hit, velocity.magnitude * Time.fixedDeltaTime)) {
				if (ShouldHit (hit)) {

					if (hitOnlyTarget && hit.transform != target)
						return;

					OnHit (hit);
				}
			}
		}else{
			RaycastHit[] hits = Physics.RaycastAll (ray, velocity.magnitude * Time.fixedDeltaTime);
			for (int i = 0; i < hits.Length; i++) {
				if (ShouldHit (hits[i])) {
					OnHit (hits[i]);
				}
			}
		}
	}

	public bool ShouldHit (RaycastHit hit) {
		if (hit.collider.gameObject.layer != parent.layer && hit.collider.tag != "ProjectileIgnore") {
			return true;
		}
		return false;
	}

	public virtual void OnHit (RaycastHit hit) {
		hit.collider.SendMessage ("OnTakeDamage", new Projectile.Damage (damage, effectiveAgainst), SendMessageOptions.DontRequireReceiver);
        if (hitParticle) {
            hitParticle.Emit (particleCount);
            hitParticle.transform.position = hit.point;
            hitParticle.transform.rotation = transform.rotation;
        }
        if (!penetrative) ReturnToPool ();
	}

	public void ReturnToPool () {
		if (gameObject.activeSelf) {
			if (particle) {
				particle.transform.parent = null;
				particle.Stop ();
			}
            if (hitParticle) {
                hitParticle.transform.parent = null;
            }
			gameObject.SetActive (false);
			CancelInvoke ("ReturnToPool");
			Invoke ("ActuallyReturnToPoolGoddammit", bulletSleepTime + 0.1f);
		}
	}

	void ActuallyReturnToPoolGoddammit () {
		if (particle) {
			particle.transform.parent = transform;
			particle.transform.position = transform.position;
			particle.transform.rotation = transform.rotation;
		}

        if (parentWeapon) {
			parentWeapon.ReturnBulletToPool (gameObject);
		}else{
			Destroy (gameObject);
		}
	}

	public struct Damage {
		public int damage;
		public Colour effectiveAgainst;

		public Damage (int damage, Colour effectiveAgainst) {
			this.damage = damage;
			this.effectiveAgainst = effectiveAgainst;
		}
	}
}