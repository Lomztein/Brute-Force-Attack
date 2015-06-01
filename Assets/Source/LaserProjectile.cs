using UnityEngine;
using System.Collections;

public class LaserProjectile : Projectile {

	public LineRenderer line;

	public override void Initialize () {

		line.material.color = new Color (1,1,1,1);
		Ray ray = new Ray (transform.position, velocity.normalized * range);
		RaycastHit hit;

		line.SetPosition (0, transform.position);
		if (Physics.Raycast (ray, out hit, range, Game.game.enemyLayer)) {
			line.SetPosition (1, hit.point);
			OnHit (hit);
		}else{
			line.SetPosition (1, ray.GetPoint (range));
		}

		Invoke ("ReturnToPool", 0.5f);
	}

	void FixedUpdate () {
		line.material.color = new Color (1,1,1,line.material.color.a - 2f * Time.fixedDeltaTime);
	}
	
}
