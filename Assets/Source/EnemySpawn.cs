using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class EnemySpawn : MonoBehaviour {

	[Header ("References")]
	public static string WAVESET_FILE_EXTENSION = ".wvs";
	
	public Rect enemySpawnRect;
	public float spawnTime = 1f;

	public GameObject[] enemyTypes;

	public static bool waveStarted;
	public static bool wavePrebbing;

	public Image waveStartedIndicator;
	public Text waveCounterIndicator;
	public GameObject gameOverIndicator;

	[Header ("Wave Stuffs")]
	public List<Wave> waves = new List<Wave>();
	public Wave.Subwave currentSubwave;

	public int waveNumber;
	private int subwaveNumber;
	private int[] spawnIndex;
	private int endedIndex;
	public int waveMastery = 1;
	
	public static float gameProgress = 1f;
	public float gameProgressSpeed = 1f;

	[Header ("Enemies")]
	public int currentEnemies;
	public GameObject endBoss;

	public static EnemySpawn cur;

	[Header ("Upcoming Wave")]
	public RectTransform upcomingCanvas;
	public RectTransform upcomingWindow;
	public GameObject upcomingEnemyPrefab;
	public GameObject upcomingSeperatorPrefab;
	private List<GameObject> upcomingContent = new List<GameObject>();

	public float buttonSize;
	public float seperatorSize;
	public float windowPosY;

	void Start () {
		cur = this;
		EndWave ();
	}

	void Poop () {
		// Hø hø hø hø..
		waveNumber++;
		UpdateUpcomingWaveScreen (waves [waveNumber]);
		Invoke ("Poop", 1f);
	}

	// TODO: Replace wavePrebbing and waveStarted with enums

	public void ReadyWave () {
		if (!waveStarted && !wavePrebbing) {
			wavePrebbing = true;
			waveStartedIndicator.color = Color.yellow;
			waveCounterIndicator.text = "Wave: Initialzing..";
			Pathfinding.BakePaths (Game.game.battlefieldWidth, Game.game.battlefieldHeight);
			Invoke ("StartWave", 2f);
		}
	}

	public void OnEnemyDeath () {
		currentEnemies--;
		if (currentEnemies < 1) {
			EndWave ();

			if (waveNumber > waves.Count) {
				if (waveMastery == 1) {
					gameOverIndicator.SetActive (true);
				}else{
					ContinueMastery ();
				}
			}
		}
	}

	public void ContinueMastery () {
		waveNumber = 0;
		waveMastery *= 2;
		gameOverIndicator.SetActive (false);
		UpdateUpcomingWaveScreen (waves[waveNumber]);
	}

	void UpdateUpcomingWaveScreen (Wave upcoming) {

		for (int i = 0; i < upcomingContent.Count; i++) {
			Destroy (upcomingContent [i]);
		}

		int sIndex = 0;
		int eIndex = 0;

		foreach (Wave.Subwave sub in upcoming.subwaves) {

			Vector3 sepPos = Vector3.down * (4 + eIndex * buttonSize) + Vector3.down * sIndex * seperatorSize;
			GameObject newSep = (GameObject)Instantiate (upcomingSeperatorPrefab, sepPos, Quaternion.identity);
			newSep.transform.SetParent (upcomingCanvas, false);
			upcomingContent.Add (newSep);
			sIndex++;
			foreach (Wave.Enemy ene in sub.enemies) {

				RectTransform rt = upcomingEnemyPrefab.GetComponent<RectTransform>();
				Vector3 enePos = new Vector3 (-rt.sizeDelta.x ,-rt.sizeDelta.y, 0) / 2 + Vector3.down * sIndex * seperatorSize + Vector3.down * eIndex * buttonSize + Vector3.right * 45;
				GameObject newEne = (GameObject)Instantiate (upcomingEnemyPrefab, enePos, Quaternion.identity);
				newEne.transform.SetParent (upcomingCanvas, false);
				upcomingContent.Add (newEne);

				newEne.transform.FindChild ("Image").GetComponent<Image>().sprite = ene.enemy.transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
				Text text = newEne.transform.FindChild ("Amount").GetComponent<Text>();
				text.text = "x " + (ene.spawnAmount * waveMastery).ToString ();

				eIndex++;
			}
		}

		upcomingWindow.sizeDelta = new Vector2 (upcomingWindow.sizeDelta.x, sIndex * seperatorSize + eIndex * buttonSize + buttonSize);
		upcomingWindow.position = new Vector3 (upcomingWindow.position.x, Screen.height - windowPosY - upcomingWindow.sizeDelta.y / 2);
	}

	public void StartWave () {
		waveNumber++;
		if (waveNumber <= waves.Count) {

			wavePrebbing = false;
			waveStarted = true;
			waveStartedIndicator.color = Color.red;
			waveCounterIndicator.text = "Wave: " + waveNumber.ToString ();
			gameProgress *= gameProgressSpeed;
			ContinueWave (true);

			currentEnemies = 0;
			Wave cur = waves[waveNumber - 1];
			foreach (Wave.Subwave sub in cur.subwaves) {
				foreach (Wave.Enemy ene in sub.enemies) {
					SplitterEnemySplit split = ene.enemy.GetComponent<SplitterEnemySplit>();
					if (split) currentEnemies += split.spawnPos.Length * waveMastery;
					currentEnemies += ene.spawnAmount * waveMastery;
				}
			}
		}else{
			Instantiate (endBoss, GetSpawnPosition (), Quaternion.identity);
			waveCounterIndicator.text = "Wave: HOLY SHIT WHAT THE FUCK IS THAT?!";
			
			wavePrebbing = false;
			waveStarted = true;
		}
	}

	void ContinueWave (bool first) {

		endedIndex = 0;
		if (!first)
			subwaveNumber++;

		if (waves [waveNumber - 1].subwaves.Count > subwaveNumber) {
			currentSubwave = waves [waveNumber - 1].subwaves [subwaveNumber];
			spawnIndex = new int[currentSubwave.enemies.Count];

			for (int i = 0; i < currentSubwave.enemies.Count; i++) {
				Invoke ("Spawn" + i.ToString (), 0f);
			}
			//Invoke ("ContinueFalseWave", currentSubwave.spawnTime + 2f);
		}
	}

	void ContinueFalseWave () {
		ContinueWave (false);
	}

	public void EndWave () {
		waveStarted = false;
		currentSubwave = null;
		subwaveNumber = 0;
		waveStartedIndicator.color = Color.green;
		Game.credits += 25 * waveNumber;
		if (waves.Count >= waveNumber + 1) {
			UpdateUpcomingWaveScreen (waves [waveNumber]);
		}
	}

	Vector3 GetSpawnPosition () {
		return new Vector3 (Random.Range (enemySpawnRect.x, enemySpawnRect.width/2), Random.Range (enemySpawnRect.y, enemySpawnRect.y - enemySpawnRect.height/2));
	}

	void CreateEnemy (GameObject enemy, int index) {
		if (enemy) {
			Instantiate (enemy, GetSpawnPosition (), Quaternion.identity);

			spawnIndex[index]++;

			if (spawnIndex[index] < currentSubwave.enemies[index].spawnAmount * waveMastery) {
				Invoke ("Spawn" + index.ToString (), currentSubwave.spawnTime / ((float)currentSubwave.enemies[index].spawnAmount * waveMastery));
			}else{
				endedIndex++;
				if (endedIndex == spawnIndex.Length) {
					ContinueWave (false);
				}
			}
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