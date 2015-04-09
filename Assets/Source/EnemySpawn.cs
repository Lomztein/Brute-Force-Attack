using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemySpawn : MonoBehaviour {

	public Rect enemySpawnRect;
	public float spawnTime = 1f;

	public GameObject enemy;

	public static bool waveStarted;
	public Image waveStartedIndicator;

	void Start () {
		EndWave ();
	}

	public void StartWave () {
		Invoke ("SpawnEnemy", spawnTime);
		waveStarted = true;
		waveStartedIndicator.color = Color.red;
	}

	public void EndWave () {
		waveStarted = false;
		waveStartedIndicator.color = Color.green;
	}

	void SpawnEnemy () {
		if (waveStarted) {
			Invoke ("SpawnEnemy", spawnTime);
			spawnTime -= 0.01f;

			Vector3 pos = new Vector3 (Random.Range (enemySpawnRect.x, enemySpawnRect.width/2), Random.Range (enemySpawnRect.y, enemySpawnRect.y - enemySpawnRect.height/2));
			Instantiate (enemy, pos, Quaternion.identity);
		}
	}

	void OnDrawGizmos () {
		Gizmos.DrawWireCube (enemySpawnRect.center, new Vector3 (enemySpawnRect.width, enemySpawnRect.height));
	}
}