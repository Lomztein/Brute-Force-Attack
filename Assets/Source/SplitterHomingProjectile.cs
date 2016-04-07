using UnityEngine;
using System.Collections;

public class SplitterHomingProjectile : HomingProjectile {

	public GameObject splitterPrefab;
	public static int amount;

	public override void OnHit (Collider col, Vector3 point, Vector3 dir) {
		base.OnHit (col, point, dir);
		for (int i = 0; i < amount; i++) {
			GameObject split = (GameObject)Instantiate (splitterPrefab, transform.position, Quaternion.Euler (0, 0, Random.Range (0, 360)));
			Projectile pro = split.GetComponent<Projectile>();
			pro.damage = Mathf.RoundToInt (damage / 2f);
			Vector2 ran = Random.insideUnitCircle.normalized;
			pro.velocity = new Vector3 (ran.x, ran.y, 0) * speed;
			pro.parent = parent;
			pro.Initialize ();
			Destroy (split, pro.range / speed);
		}
	}
}
