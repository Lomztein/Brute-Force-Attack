using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public Vector3 velocity;
	public int damage;
	public GameObject parent;
	public float range;
	public Transform target;

	// Update is called once per frame
	void FixedUpdate () {

		CastRay ();
		transform.position += velocity * Time.fixedDeltaTime;

	}

	public void CastRay () {
		Ray ray = new Ray (transform.position, transform.right * velocity.magnitude * Time.fixedDeltaTime);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, velocity.magnitude * Time.fixedDeltaTime)) {

			if (hit.collider.gameObject.layer != parent.layer) {

				hit.collider.SendMessage ("OnTakeDamage", damage, SendMessageOptions.DontRequireReceiver);
				Destroy (gameObject);

			}

		}
	}
}