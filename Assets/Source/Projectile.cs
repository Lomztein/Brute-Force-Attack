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
	public GameObject hitParticle;
	public Weapon parentWeapon;

	public Colour effectiveAgainst;

	// Update is called once per frame
	public virtual void Initialize () {
	}
	
	void FixedUpdate () {

		CastRay ();
		transform.position += velocity * Time.fixedDeltaTime;

	}

	public void CastRay () {
		Ray ray = new Ray (transform.position, transform.right * velocity.magnitude * Time.fixedDeltaTime);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, velocity.magnitude * Time.fixedDeltaTime)) {
			if (hit.collider.gameObject.layer != parent.layer && hit.collider.tag != "BulletIgnore") {
				OnHit (hit);
			}
		}
	}

	public virtual void OnHit (RaycastHit hit) {
		hit.collider.SendMessage ("OnTakeDamage", new Projectile.Damage (damage, effectiveAgainst), SendMessageOptions.DontRequireReceiver);
		if (hitParticle) Destroy ((GameObject)Instantiate (hitParticle, hit.point, transform.rotation), 1f);
		if (!penetrative) ReturnToPool ();
	}

	public void ReturnToPool () {
		if (gameObject.activeSelf) {
			gameObject.SetActive (false);
			CancelInvoke ("ReturnToPool");
			Invoke ("ActuallyReturnToPoolGoddammit", Weapon.bulletSleepTime);
		}
	}

	void ActuallyReturnToPoolGoddammit () {
		if (parentWeapon) {
			parentWeapon.ReturnBulletToPool (gameObject);
		}else{
			Destroy (gameObject);
		}
	}

	public class Damage {
		public int damage;
		public Colour effectiveAgainst;

		public Damage (int damage, Colour effectiveAgainst) {
			this.damage = damage;
			this.effectiveAgainst = effectiveAgainst;
		}
	}
}