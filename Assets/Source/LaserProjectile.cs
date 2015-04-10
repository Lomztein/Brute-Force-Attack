using UnityEngine;
using System.Collections;

public class LaserProjectile : Projectile {

	public LineRenderer line;

	void Start () {

		Ray ray = new Ray (transform.position, velocity.normalized * range);
		RaycastHit hit;

		line.SetPosition (0, transform.position);
		if (Physics.Raycast (ray, out hit, range, Game.game.enemyLayer)) {
			line.SetPosition (1, hit.point);
			hit.collider.SendMessage ("OnTakeDamage", damage);
		}else{
			line.SetPosition (1, ray.GetPoint (range));
		}

		Destroy (gameObject, 0.5f);
	}

	void FixedUpdate () {
		line.material.color = new Color (1,0,0,line.material.color.a - 2f * Time.fixedDeltaTime);
	}
	
}
