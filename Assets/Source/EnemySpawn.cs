using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemySpawn : MonoBehaviour {
	
	public Rect enemySpawnRect;
	public float spawnTime = 1f;

	public GameObject[] enemyTypes;

	public static bool waveStarted;
	public Image waveStartedIndicator;
	public Text waveCounterIndicator;
	public GameObject gameOverIndicator;

	public List<Wave> waves = new List<Wave>();
	public Wave.Subwave currentSubwave;

	public int waveNumber;
	private int subwaveNumber;
	private int[] spawnIndex;
	
	public static float gameProgress = 1f;
	public float gameProgressSpeed = 1f;

	void Start () {
		EndWave ();
	}

	public void ReadyWave () {
		waveStartedIndicator.color = Color.yellow;
		waveCounterIndicator.text = "Wave: Initialzing..";
		Dijkstra.BakePaths ();
	}

	public void StartWave () {
		waveNumber++;
		if (waveNumber <= waves.Count) {
			waveStarted = true;
			waveStartedIndicator.color = Color.red;
			waveCounterIndicator.text = "Wave: " + waveNumber.ToString ();
			gameProgress *= gameProgressSpeed;
			ContinueWave (true);
		}else{
			gameOverIndicator.SetActive (true);
		}
	}

	void ContinueWave (bool first) {
		if (!first)
			subwaveNumber++;

		if (subwaveNumber >= waves[waveNumber - 1].subwaves.Count) {
			EndWave ();
			return;
		}

		currentSubwave = waves[waveNumber - 1].subwaves[subwaveNumber];
		spawnIndex = new int[currentSubwave.enemies.Count];

		for (int i = 0; i < currentSubwave.enemies.Count; i++) {
			Invoke ("Spawn" + i.ToString (), 0f);
		}
	}

	public void EndWave () {
		waveStarted = false;
		currentSubwave = null;
		subwaveNumber = 0;
		waveStartedIndicator.color = Color.green;
	}

	Vector3 GetSpawnPosition () {
		return new Vector3 (Random.Range (enemySpawnRect.x, enemySpawnRect.width/2), Random.Range (enemySpawnRect.y, enemySpawnRect.y - enemySpawnRect.height/2));
	}

	void CreateEnemy (GameObject enemy, int index) {
		Instantiate (enemy, GetSpawnPosition (), Quaternion.identity);
		spawnIndex[index]++;

		if (spawnIndex[index] < currentSubwave.enemies[index].spawnAmount) {
			Invoke ("Spawn" + index.ToString (), currentSubwave.spawnTime / (float)currentSubwave.enemies[index].spawnAmount);
		}else{
			ContinueWave (false);
		}
	}

	void OnDrawGizmos () {
		Gizmos.DrawWireCube (enemySpawnRect.center, new Vector3 (enemySpawnRect.width, enemySpawnRect.height));
	}

	void Spawn0 () {
		int index = 0;
		CreateEnemy (currentSubwave.enemies[index].enemy, index);
	}
	void Spawn1 () {
		int index = 1;
		CreateEnemy (currentSubwave.enemies[index].enemy, index);
	}
	void Spawn2 () {
		int index = 2;
		CreateEnemy (currentSubwave.enemies[index].enemy, index);
	}
	void Spawn3 () {
		int index = 3;
		CreateEnemy (currentSubwave.enemies[index].enemy, index);
	}
	void Spawn4 () {
		int index = 4;
		CreateEnemy (currentSubwave.enemies[index].enemy, index);
	}
	void Spawn5 () {
		int index = 5;
		CreateEnemy (currentSubwave.enemies[index].enemy, index);
	}
	void Spawn6 () {
		int index = 6;
		CreateEnemy (currentSubwave.enemies[index].enemy, index);
	}
	void Spawn7 () {
		int index = 7;
		CreateEnemy (currentSubwave.enemies[index].enemy, index);
	}
}