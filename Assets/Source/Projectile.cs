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
    public bool isHitscan;
    public AnimationCurve damageFalloff;

    public ParticleSystem particle;
    public ParticleSystem hitParticle;
    public int particleCount;

	public float bulletSleepTime = 1f; // Should be synced with particle fadeout time.

	// Update is called once per frame
	public virtual void Initialize () {
		if (particle) particle.Play ();
        CastRay ();

        if (target && gameObject.activeSelf) {
            SphereCollider col = target.GetComponent<SphereCollider> ();
            if (Vector3.Distance (transform.position, target.position) < col.radius) {

                OnHit (col, transform.position, transform.forward);
            }
        }
	}
	
	void FixedUpdate () {

		CastRay ();
		transform.position += velocity * Time.fixedDeltaTime;

	}

	public void CastRay () {
		Ray ray = new Ray (transform.position, transform.right * velocity.magnitude * Time.fixedDeltaTime * 2f);
        Debug.DrawLine (ray.origin, ray.origin + ray.direction * velocity.magnitude * Time.fixedDeltaTime * 2f);
		RaycastHit hit;

		if (!penetrative) {
			if (Physics.Raycast (ray, out hit, velocity.magnitude * Time.fixedDeltaTime * 2f)) {
				if (ShouldHit (hit)) {

					if (hitOnlyTarget && hit.transform != target)
						return;

					OnHit (hit.collider, hit.point, transform.right);
				}
			}
		}else{
			RaycastHit[] hits = Physics.RaycastAll (ray, velocity.magnitude * Time.fixedDeltaTime * 2f);
			for (int i = 0; i < hits.Length; i++) {
				if (ShouldHit (hits[i])) {
					OnHit (hits[i].collider, hits[i].point, transform.right);
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

	public virtual void OnHit (Collider col, Vector3 point, Vector3 dir) {
        float distance = Vector3.Distance (parentWeapon.transform.position, point) / range;
		col.SendMessage ("OnTakeDamage", new Damage (Mathf.RoundToInt (damage * damageFalloff.Evaluate (distance)), effectiveAgainst, parentWeapon), SendMessageOptions.DontRequireReceiver);
        if (hitParticle) {
            hitParticle.Emit (particleCount);
            hitParticle.transform.position = point;
            hitParticle.transform.rotation = transform.rotation;
        }
        if (!penetrative) ReturnToPool ();
	}

	public void ReturnToPool () {
		if (gameObject.activeSelf || (hitParticle && hitParticle.gameObject.activeSelf)) {
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

        if (hitParticle) {
            hitParticle.transform.parent = parentWeapon.pool;
        }

        if (parentWeapon) {
			parentWeapon.ReturnBulletToPool (gameObject);
		}else{
            if (hitParticle) Destroy (hitParticle.gameObject);
			Destroy (gameObject);
        }
	}

	public struct Damage {

		public int damage;
		public Colour effectiveAgainst;
        public Weapon weapon;

		public Damage (int damage, Colour effectiveAgainst, Weapon weapon) {
			this.damage = damage;
			this.effectiveAgainst = effectiveAgainst;
            this.weapon = weapon;
		}
	}
}