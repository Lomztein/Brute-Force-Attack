using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class EnemySpawn : MonoBehaviour {

	public static string WAVESET_FILE_EXTENSION = ".wvs";
	
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
		Game.credits += 25 * waveNumber;
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

	public void SaveWaveset (Wave[] waves, string name) {
		string path = Game.WAVESET_SAVE_DIRECTORY + name + WAVESET_FILE_EXTENSION;
		StreamWriter write = File.CreateText (path);

		write.WriteLine ("PROJECT VIRUS WAVE SET FILE, EDIT WITH CAUTION");
		write.WriteLine (name);

		foreach (Wave wave in waves) {
			write.WriteLine ("\twave:");
			foreach (Wave.Subwave subwave in wave.subwaves) {
				write.WriteLine ("\t\tsptm:" + subwave.spawnTime.ToString ());
				write.WriteLine ("\t\tenms:");
				foreach (Wave.Enemy enemy in subwave.enemies) {
					write.WriteLine ("\t\t\tenmy:" + enemy.enemy.name);
					write.WriteLine ("\t\t\tamnt:" + enemy.spawnAmount.ToString ());
				}
			}
		}

		write.WriteLine ("END OF FILE");
		write.Close ();
	}

	public List<Wave> LoadWaveset (string name) {
		string path = Game.WAVESET_SAVE_DIRECTORY + name + WAVESET_FILE_EXTENSION;
		string[] content = ModuleAssemblyLoader.GetContents (path);

		List<Wave> locWaves = new List<Wave> ();

		Wave cw = null;
		Wave.Subwave cs = null;
		Wave.Enemy ce = null;
		Debug.Log (content.Length);

		for (int i = 0; i < content.Length; i++) {
			string c = content [i];

			// Find wave
			if (c.Length > 4) {
				if (c.Substring (0,5) == "\twave") {
					cw = new Wave ();
					locWaves.Add (cw);
				}
			}

			// Find and read subwave
			if (c.Length > 5) {
				if (c.Substring (0,6) == "\t\tsptm") {
					cs = new Wave.Subwave ();
					cs.spawnTime = float.Parse (c.Substring (7));
					cw.subwaves.Add (cs);
				}
			}

			// Find and read enemy
			if (c.Length > 6) {
				if (c.Substring (0,7) == "\t\t\tenmy") {
					ce =  new Wave.Enemy ();
					ce.enemy = GetEnemyFromName (c.Substring (8));
				}

				if (c.Substring (0,7) == "\t\t\tamnt") {
					ce.spawnAmount = int.Parse (c.Substring (8));
					cs.enemies.Add (ce);
				}
			}
		}

		return locWaves;
	}

	GameObject GetEnemyFromName (string n) {
		foreach (GameObject obj in enemyTypes) {
			if (obj.name == n) {
				return obj;
			}
		}

		return null;
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