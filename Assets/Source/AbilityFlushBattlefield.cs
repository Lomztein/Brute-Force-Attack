using UnityEngine;
using System.Collections;

public class AbilityFlushBattlefield : Ability {

	public float explosionDelay;
	public GameObject explosion;
	public Transform crosshair;
	public int damage;

	// Use this for initialization
	void Start () {
		StartCoroutine (ExplodeEverything ());
	}

	/*void Update () {
		if (crosshair) {
			Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			crosshair.transform.position = mousePos + Vector3.forward;
			RotateAnimateCrosshair (crosshair, 2f);
		}
	}*/

	IEnumerator ExplodeEverything () {
		//Destroy (crosshair.gameObject);
		//button.OnAbilityUsed ();

		for (int y = -Game.game.battlefieldHeight / 2; y < Game.game.battlefieldHeight / 2; y += 2) {
			for (int x = -Game.game.battlefieldWidth / 2; x < Game.game.battlefieldWidth / 2; x++) {
				Vector3 offset = new Vector3 (Random.Range (-1f, 1f), Random.Range (-1f, 1f)) + Vector3.one * 0.5f;
				Collider[] nearby = Physics.OverlapSphere (new Vector3 (x,y) + offset, 2f, Game.game.enemyLayer);
                for (int i = 0; i < nearby.Length; i++) {
                    Enemy ene = nearby[i].GetComponent<Enemy> ();
                    if (ene) {
                        ene.SendMessage ("OnTakeDamage", new Projectile.Damage (int.MaxValue, Colour.None, null));
                    }
                }
				Destroy ((GameObject)Instantiate (explosion, new Vector3 (x, y) + offset, Quaternion.identity), 1f);
				// yield return new WaitForFixedUpdate ();
			}
			yield return new WaitForSeconds (explosionDelay);
		}
		Destroy (gameObject);
	}
}
