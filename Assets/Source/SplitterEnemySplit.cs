using UnityEngine;
using System.Collections;

public class SplitterEnemySplit : MonoBehaviour {

	public GameObject minion;
	public Vector3[] spawnPos;

	void OnDeath () {
		for (int i = 0; i < spawnPos.Length; i++) {
			GameObject m = (GameObject)Instantiate (minion, transform.position + spawnPos[i], Quaternion.identity);
			Enemy that = m.GetComponent<Enemy>();
			Enemy my = GetComponent<Enemy>();

			that.path = my.path;
			that.pathIndex = my.pathIndex;
		}
	}

	void OnDrawGizmos () {
		for (int i = 0; i < spawnPos.Length; i++) {
			Gizmos.DrawSphere (transform.position + spawnPos[i], 0.1f);
		}
	}
}
