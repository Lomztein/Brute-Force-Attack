using UnityEngine;
using System.Collections;

public class PointExplosionAbility : Ability {

	public int damage;
	public float range;
	public GameObject explosionParticle;
	public Transform crosshair;
	public bool doExplodeDamage = true;

	// Use this for initialization
	void Start () {
		crosshair.transform.localScale = Vector3.one * range;
	}
	
	// Update is called once per frame
	new void Update () {
		base.Update ();
		Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		transform.position = mousePos + Vector3.forward;
		RotateAnimateCrosshair (crosshair, 2f);
		if (Input.GetMouseButtonDown (0)) {
			Explode (new Vector3 (mousePos.x, mousePos.y, 0));
		}
	
	}

	void Explode (Vector3 pos) {
		if (doExplodeDamage) {
			Collider[] nearby = Physics.OverlapSphere (pos, range, Game.game.enemyLayer);
			for (int i = 0; i < nearby.Length; i++) {
				nearby [i].SendMessage ("OnTakeDamage", new Projectile.Damage (damage, Colour.Red, null), SendMessageOptions.DontRequireReceiver);
			}
		}
		button.OnAbilityUsed ();
		GameObject obj = (GameObject)Instantiate (explosionParticle, pos, Quaternion.identity);
		if (doExplodeDamage) Destroy (obj, 2f);
		Destroy (gameObject);
	}

	void OnDrawGizmos () {
		Gizmos.DrawWireSphere (transform.position, range);
	}
}
