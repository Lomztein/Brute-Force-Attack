using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClusterHomingMissle : HomingProjectile {

	public float searchRange;
	public GameObject clusterMissle;
	public int maxCluster = 16;

	// Update is called once per frame
	public override void FixedUpdate () {
		base.FixedUpdate ();

		if (target) {
			if (Vector3.Distance (transform.position, target.position) < searchRange) {
				List<Transform> toAttack = new List<Transform> ();
				Collider[] nearby = Physics.OverlapSphere (target.position, searchRange, Game.game.enemyLayer);
				for (int i = 0; i < nearby.Length; i++) {
					if (toAttack.Count < maxCluster) {
						if (nearby [i].GetComponent<Enemy> ().type == target.GetComponent<Enemy>().type) {
							toAttack.Add (nearby[i].transform);
						}
					}
				}

				for (int i = 0; i < toAttack.Count; i++) {
					GameObject cluster = (GameObject)Instantiate (clusterMissle, transform.position, transform.rotation);
					HomingProjectile missle = cluster.GetComponent<HomingProjectile>();
					missle.target = toAttack[i];
					missle.damage = Mathf.RoundToInt ((float)damage / (float)toAttack.Count);
					missle.velocity = transform.right * speed * 1.5f;
					missle.parent = parent;
					missle.Initialize ();
				}

				Destroy ((GameObject)Instantiate (hitParticle, transform.position, Quaternion.identity), 1f);
				ReturnToPool ();
			}
		}	
	}
}
