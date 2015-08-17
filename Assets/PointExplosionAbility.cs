using UnityEngine;
using System.Collections;

public class PointExplosionAbility : Ability {

	public int damage;
	public float range;
	public GameObject explosionParticle;
	public Transform crosshair;

	// Use this for initialization
	void Start () {
		crosshair.transform.localScale = Vector3.one * range;
	}
	
	// Update is called once per frame
	new void Update () {
		base.Update ();
		Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		transform.position = mousePos + Vector3.forward;
		RotateAnimateCrosshair (crosshair, range);
		if (Input.GetMouseButtonDown (0)) {
			Explode (new Vector3 (mousePos.x, mousePos.y, 0));
		}
	
	}

	void Explode (Vector3 pos) {
		Collider[] nearby = Physics.OverlapSphere (pos, range, Game.game.enemyLayer);
		for (int i = 0; i < nearby.Length; i++) {
			nearby[i].SendMessage ("OnTakeDamage", new Projectile.Damage (damage, Colour.Red), SendMessageOptions.DontRequireReceiver);
		}
		button.OnAbilityUsed ();
		Destroy ((GameObject)Instantiate (explosionParticle, pos, Quaternion.identity), 2f);
		Destroy (gameObject);
	}

	void OnDrawGizmos () {
		Gizmos.DrawWireSphere (transform.position, range);
	}
}
