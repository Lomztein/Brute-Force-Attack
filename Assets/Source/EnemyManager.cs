using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

public class EnemyManager : MonoBehaviour {

	[Header ("References")]
	public static string WAVESET_FILE_EXTENSION = ".dat";
	
	public float spawnTime = 1f;

	public GameObject[] enemyTypes;

	public static bool waveStarted;
	public static bool wavePrebbing;

	public Image waveStartedIndicator;
	public Text waveCounterIndicator;
    public GameObject pathDemonstrator;
    public Image pathDemonstratorButton;
    public Sprite[] pathDemonstratorButtonSprites;
    public bool showingPaths;
    public Slider flushBattlefieldSlider;

    public Button[] disabledDuringWaves;

	[Header ("Wave Stuffs")]
	public List<Wave> waves = new List<Wave>();
	public Wave.Subwave currentSubwave;
    private List<EnemySpawnPoint> availableSpawns = new List<EnemySpawnPoint>();

    public static int ExternalWaveNumber { get { return cur.waveNumber + cur.waveMasteryIndex * cur.waves.Count; } }
    public static int WaveMastery { get { return Mathf.RoundToInt (Mathf.Pow (2, cur.waveMasteryIndex)); } }

    public int waveNumber;
	private int subwaveNumber;
	private int[] spawnIndex;
	private int endedIndex;
    public int waveMasteryIndex;

    public float amountModifier = 1f;

	public static float readyWaitTime = 2f;
	public static float gameProgress = 1f;

    [Range (0, 10)]
	public float gameProgressSpeed = 1f;

    // Paths are build first, enemies second, so EnemiesBuild also means PathsBuild.
    public enum ReadyStatus { None, PathsBuild, EnemiesBuild };
    public ReadyStatus readyStatus = ReadyStatus.None;
    private bool cancellingWave;

	[Header ("Enemies")]
	public int currentEnemies;
	public GameObject endBoss;
	private Dictionary<Wave.Enemy, List<GameObject>> pooledEnemies = new Dictionary<Wave.Enemy, List<GameObject>> ();
	//public Transform enemyPool;
    public List<Enemy> spawnedEnemies = new List<Enemy> ();
    public Dictionary<string, int> enemiesKilled = new Dictionary<string, int>();

	public static EnemyManager cur;
    public GameObject enemyPortalPrefab;
    public GameObject enemySpawnIndicator;
    public List<GameObject> currentPortals = new List<GameObject>();
    public List<GameObject> currentIndicators = new List<GameObject>();

    [Header ("Upcoming Wave")]
    public Text upcomingHeader;
	public RectTransform upcomingWindow;
	public GameObject upcomingEnemyPrefab;
	public GameObject upcomingSeperatorPrefab;

	private List<GameObject> upcomingContent = new List<GameObject>();
	private List<UpcomingElement> upcomingElements = new List<UpcomingElement>();

	public float buttonSize;
	public float seperatorSize;
	public float windowPosY;

    public GameObject listPartPrefab;
    public GameObject waveListParent;
    public RectTransform listContentParent;
    public List<GameObject> listContent = new List<GameObject>();
    public ScrollRect listScrollRect;

    public static int spawnedResearch = 0;
    public static int chanceToSpawnResearch;

    public static void Initialize () {
        cur = GameObject.Find ("EnemyManager").GetComponent<EnemyManager> ();
        cur.AddFinalBoss ();

        cur.EndWave (false);
        cur.SetSpawnIndicators ();
    }

    public void DemonstratePaths () {
        if (showingPaths) {
            showingPaths = false;
            PathDemonstrator.cur.Destroy ();
            pathDemonstratorButton.sprite = pathDemonstratorButtonSprites[0];
            pathDemonstratorButton.GetComponentInParent<HoverContextElement> ().text = "Display enemy paths";
        } else {

            SetAvailableSpawnpoints ();
            if (availableSpawns.Count == 0) {
                Game.ShowErrorMessage ("Cannot show paths: Paths unclear", 5f);
                return;
            }

            showingPaths = true;
            StartCoroutine (DPATHS ());
            pathDemonstratorButton.sprite = pathDemonstratorButtonSprites[1];
            pathDemonstratorButton.GetComponentInParent<HoverContextElement> ().text = "Stop displaying enemy paths";
        }
        HoverContextElement.activeElement = null;
    }

    public void SetSpawnIndicators () {
        foreach (GameObject i in currentIndicators) {
            Destroy (i);
        }

        currentIndicators.Clear ();
        SetAvailableSpawnpoints ();
        Pathfinding.BakePaths ();

        for (int i = 0; i < Game.game.enemySpawnPoints.Count; i++) {
            EnemySpawnPoint point = Game.game.enemySpawnPoints[i];

            Vector3 next = Game.game.GetVoidDirection (point.worldPosition);
            Quaternion rotation = Quaternion.Euler (0f, 0f, Angle.CalculateAngle (point.worldPosition, next) + 180f);
            GameObject newIndicator = (GameObject)Instantiate (enemySpawnIndicator, point.worldPosition, rotation);

            currentIndicators.Add (newIndicator);
            SpriteRenderer sprite = newIndicator.GetComponentInChildren<SpriteRenderer> ();

            if (point.blocked) {
                sprite.color = Color.red;
            }else {
                sprite.color = Color.green;
            }
        }
    }

    public void CancelDemonstration ( string message ) {
        if (pathDemonstratorButton.sprite == pathDemonstratorButtonSprites[1]) {
            Game.ShowErrorMessage (message, 2f);
            DemonstratePaths ();
        }
    }

    private IEnumerator DPATHS () {

        for (int i = 0; i < availableSpawns.Count; i++) {
            while (availableSpawns[i].path == null) {
                yield return new WaitForEndOfFrame ();
            }

            Vector2[] path = availableSpawns[i].path;
            GameObject d = (GameObject)Instantiate (pathDemonstrator, path[0], Quaternion.identity);
            d.GetComponent<PathDemonstrator> ().path = path;
            d.GetComponent<PathDemonstrator> ().StartPath ();
            for (int j = 0; j < 50; j++) {
                if (!showingPaths)
                    yield break;
                yield return new WaitForFixedUpdate ();
            }
        }
    }

    public static void AddEnemy (Enemy enemy) {
        cur.spawnedEnemies.Add (enemy);
    }

    void Update () {
        if (Input.GetButtonDown("StartWave") && !Game.isPaused) {
            StartReadyWave ();
        }
    }

    void FixedUpdate () {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
        mousePos.z = 0;

        //Enemy movement code:
        for (int i = 0; i < spawnedEnemies.Count; i++) {
            if (spawnedEnemies[i] && spawnedEnemies[i].active) {
                Enemy enemy = spawnedEnemies[i];

                enemy.UpdateHealthbar ();
                enemy.DoMovement ();    
                enemy.UpdateFreeze ();
            }
        }
    }

    void UpdateAmountModifier () {
        amountModifier = WaveMastery * Game.difficulty.amountMultiplier;
        if (Game.game.gamemode == Gamemode.GlassEnemies) {
            amountModifier *= 10;
        } else if (Game.game.gamemode == Gamemode.TitaniumEnemies) {
            amountModifier = amountModifier * 0.1f;
        }
    }

    private IEnumerator CleanEnemyArray () {
        int destroyPerTick = Mathf.CeilToInt ((float)spawnedEnemies.Count / readyWaitTime * Time.fixedDeltaTime);

        for (int i = 0; i < spawnedEnemies.Count; i++) {
            Destroy (spawnedEnemies[i].gameObject);
            Destroy (spawnedEnemies[i].deathParticle.gameObject);
            if (i % destroyPerTick == 0)
                yield return new WaitForFixedUpdate ();
        }

        spawnedEnemies.Clear ();
    }

    public void ForceInstantCleanEnemyArray () {
        for (int i = 0; i < spawnedEnemies.Count; i++) {
            Destroy (spawnedEnemies[i].gameObject);
            Destroy (spawnedEnemies[i].deathParticle.gameObject);
            Enemy ene = spawnedEnemies[i];
            if (ene.healthSlider && ene.healthSlider.transform.parent != ene.transform) {
                Destroy (ene.healthSlider.gameObject);
            }
        }
        spawnedEnemies.Clear ();
    }

	void AddFinalBoss () {
		// What the fuck is this shit?
		Wave.Enemy e = new Wave.Enemy ();
		e.enemy = endBoss;
		e.spawnAmount = 1;
		Wave.Subwave s = new Wave.Subwave ();
		s.enemies.Add (e);
		s.spawnTime = 1f;
		Wave w = new Wave ();
		w.subwaves.Add (s);
		waves.Add (w);
	}

	void Poop () {
		// Hø hø hø hø..
		waveNumber++;
		UpdateUpcomingWaveScreen (waves [waveNumber], ExternalWaveNumber, upcomingWindow);
		Invoke ("Poop", 1f);
	}

	// TODO: Replace wavePrebbing and waveStarted with enums

	public IEnumerator PoolBaddies () {
        yield return new WaitForSeconds (1f);

        if (availableSpawns.Count == 0) {
            CancelWave();
            readyStatus = ReadyStatus.EnemiesBuild;
            Game.ShowErrorMessage("Unable to start wave: Paths unclear", 3f);
        } else {
            Wave cur = waves[waveNumber - 1];
            Queue<Wave.Enemy> spawnQueue = new Queue<Wave.Enemy>();

            currentEnemies = 0;
            int index = -1;
            foreach (Wave.Subwave sub in cur.subwaves) {
                foreach (Wave.Enemy ene in sub.enemies) {
                    index++;
                    ene.index = index;

                    spawnQueue.Enqueue(ene);

                    SplitterEnemySplit split = ene.enemy.GetComponent<SplitterEnemySplit>();
                    if (split)
                        currentEnemies += Mathf.RoundToInt (ene.spawnAmount * (float)split.spawnPos.Length * amountModifier);
                    currentEnemies += Mathf.RoundToInt (ene.spawnAmount * amountModifier);
                }
            }

            int spawnPerTick = 256;
            List<Enemy> toArray = new List<Enemy>();

            index = 0;
            while (spawnQueue.Count > 0) {
                for (int i = 0; i < Mathf.RoundToInt (spawnQueue.Peek().spawnAmount * amountModifier); i++) {
                    GameObject newEne = (GameObject)Instantiate(spawnQueue.Peek().enemy, Vector3.right * 5000f, Quaternion.identity);
                    toArray.Add(newEne.GetComponent<Enemy>());

                    newEne.SetActive(false);
                    //newEne.transform.parent = enemyPool;

                    Enemy e = newEne.GetComponent<Enemy>();
                    e.upcomingElement = upcomingElements[spawnQueue.Peek().index];
                    e.Initialize ();

                    if (!pooledEnemies.ContainsKey(spawnQueue.Peek())) {
                        pooledEnemies.Add(spawnQueue.Peek(), new List<GameObject>());
                    }
                    pooledEnemies[spawnQueue.Peek()].Add(newEne);
                    index++;


                    if (index >= spawnPerTick) {
                        yield return new WaitForFixedUpdate();
                        index = 0;
                    }
                }
                spawnQueue.Dequeue();
            }

            spawnedEnemies = toArray;
            chanceToSpawnResearch = currentEnemies;
            readyStatus = ReadyStatus.EnemiesBuild;
            yield return null;
        }
	}

    public void StartReadyWave () {
        StartCoroutine(ReadyWave());
    }

	public IEnumerator ReadyWave () {
        if (Game.state == Game.State.Started && waveStartedIndicator.GetComponentInParent<Button>().interactable == true) { // Hacked? Not even remotely lol pffth what are you on about please donn't hurt me :(
            if (!waveStarted && !wavePrebbing) {
                Game.ChangeButtons (false);
                PlayerInput.cur.CancelAll ();
                waveStartedIndicator.GetComponentInParent<HoverContextElement> ().text = "Initializing...";
                HoverContextElement.activeElement = null;
                Game.CrossfadeMusic (Game.game.combatMusic, 2f);

                waveNumber++;

                cancellingWave = false;
                wavePrebbing = true;
                waveStartedIndicator.color = Color.yellow;
                waveCounterIndicator.text = "Wave: Initializing..";
                spawnedResearch = 0;
                SetAvailableSpawnpoints ();
                SetPortals (true);
                while (readyStatus != ReadyStatus.PathsBuild) {
                    yield return new WaitForEndOfFrame ();
                }
                StartCoroutine (PoolBaddies ());
                while (readyStatus != ReadyStatus.EnemiesBuild) {
                    yield return new WaitForEndOfFrame ();
                }
                if (cancellingWave) {
                    yield break;
                }
                StartWave ();
            } else if (waveStarted) {
                Game.ToggleFastGameSpeed ();
            }
        }

	}

    void SetPortals (bool state) {
        if (state) {
            for (int i = 0; i < availableSpawns.Count; i++) {
                GameObject newPortal = (GameObject)Instantiate (enemyPortalPrefab, availableSpawns[i].worldPosition - Vector3.forward, Quaternion.identity);
                currentPortals.Add (newPortal);
            }
        }else {
            for (int i = 0; i < currentPortals.Count; i++) {
                Destroy (currentPortals[i]);
            }
        }
    }

    void SetAvailableSpawnpoints () {
        Pathfinding.BakePaths ();
        availableSpawns = new List<EnemySpawnPoint>();

        for (int i = 0; i < Game.game.enemySpawnPoints.Count; i++) {

            EnemySpawnPoint sp = Game.game.enemySpawnPoints[i];
            Vector2 wp = Pathfinding.finder.WorldToNode(sp.worldPosition) + new Vector2 (0.5f, 0.5f);
            int x = Mathf.RoundToInt(wp.x) - 1;
            int y = Mathf.RoundToInt(wp.y) - 1;

            if (Game.isWalled[x,y] == Game.WallType.None && sp.path != null) {
                availableSpawns.Add(sp);
                sp.blocked = false;
            } else {
                sp.blocked = true;
            }
        }

        // So many steps just to convert a single coordinate to rounded numbers..
    }

    void CancelWave () {

        waveNumber--;

        cancellingWave = true;
        wavePrebbing = false;
        EndWave(false);
    }

	public void OnEnemyDeath () {
		currentEnemies--;
        chanceToSpawnResearch = currentEnemies;
		if (currentEnemies < 1 && Game.state == Game.State.Started) {
			EndWave (true);

			if (waveNumber >= waves.Count) {
				if (WaveMastery == 1) {
                    Game.game.masteryModeIndicator.SetActive(true);
				}else{
					ContinueMastery ();
				}
			}
		}
	}

    public static void AddKill (string enemyName) {
        if (cur.enemiesKilled == null)
            cur.enemiesKilled = new Dictionary<string, int> ();

        if (!cur.enemiesKilled.ContainsKey (enemyName)) {
            cur.enemiesKilled.Add (enemyName, 1);
        }else {
            cur.enemiesKilled[enemyName]++;
        }
    }

    public static int GetKills (string enemyName) {
        if (cur.enemiesKilled.ContainsKey (enemyName)) {
            return cur.enemiesKilled[enemyName];
        }else {
            return 0;
        }
    }

	public void ContinueMastery () {
		waveNumber = 0;
        waveMasteryIndex++;
        UpdateAmountModifier ();
        Game.game.masteryModeIndicator.SetActive (false);
        UpdateUpcomingWaveScreen (waves[waveNumber], ExternalWaveNumber, upcomingWindow);
        UpdateAmountModifier ();
	}

	void UpdateUpcomingWaveScreen (Wave upcoming, int waveIndex, RectTransform window) {

        if (window == upcomingWindow) {
            upcomingHeader.text = "Upcoming";
            for (int i = 0; i < upcomingContent.Count; i++) {
                Destroy (upcomingContent[i]);
            }

            for (int i = 0; i < upcomingElements.Count; i++) {
                Destroy (upcomingElements[i]);
            }

            upcomingElements.Clear ();
        }

		int sIndex = 0;
		int eIndex = 0;

        window.name = (waveIndex + 1).ToString ();
        RectTransform upcomingCanvas = window.Find ("Content").gameObject.GetComponent<RectTransform> ();

		foreach (Wave.Subwave sub in upcoming.subwaves) {

			Vector3 sepPos = Vector3.down * (4 + eIndex * buttonSize) + Vector3.down * sIndex * seperatorSize;
			GameObject newSep = (GameObject)Instantiate (upcomingSeperatorPrefab, sepPos, Quaternion.identity);
			newSep.transform.SetParent (upcomingCanvas, false);

            if (window == upcomingWindow)
                upcomingContent.Add (newSep);

			sIndex++;
			foreach (Wave.Enemy ene in sub.enemies) {

				RectTransform rt = upcomingEnemyPrefab.GetComponent<RectTransform>();
				Vector3 enePos = new Vector3 (-rt.sizeDelta.x ,-rt.sizeDelta.y, 0) / 2 + Vector3.down * sIndex * seperatorSize + Vector3.down * eIndex * buttonSize + Vector3.right * 45;
				GameObject newEne = (GameObject)Instantiate (upcomingEnemyPrefab, enePos, Quaternion.identity);
				newEne.transform.SetParent (upcomingCanvas, false);

                if (window == upcomingWindow)
                    upcomingContent.Add (newEne);

				newEne.transform.Find ("Image").GetComponent<Image>().sprite = ene.enemy.transform.Find ("Sprite").GetComponent<SpriteRenderer>().sprite;
                Button button = newEne.transform.Find ("Image").GetComponent<Button> ();
                Text text = newEne.transform.Find ("Amount").GetComponent<Text>();

                AddEnemyButtonListener (button, ene.enemy.GetComponent<Enemy>(), 1);

                if (window == upcomingWindow) {
                    upcomingElements.Add (UpcomingElement.CreateInstance<UpcomingElement> ());
                    upcomingElements[upcomingElements.Count - 1].upcomingText = text;
                    upcomingElements[upcomingElements.Count - 1].remaining = Mathf.RoundToInt (ene.spawnAmount * amountModifier + 1);
                    upcomingElements[upcomingElements.Count - 1].Decrease ();
                }else {
                    text.text = "x " + (ene.spawnAmount * amountModifier).ToString ();
                }

                eIndex++;
			}
		}

        float sy = sIndex * seperatorSize + eIndex * buttonSize + buttonSize;
        window.sizeDelta = new Vector2 (window.sizeDelta.x, sy);
        window.position = new Vector3 (window.position.x, Screen.height - windowPosY - window.sizeDelta.y / 2);

        LayoutElement ele = window.GetComponent<LayoutElement> ();
        // If it contains a layout element, it's from the list.
        if (ele) {
            ele.preferredHeight = sy;
        }
	}

    void AddEnemyButtonListener ( Button button, Enemy enemy, int wave ) {
        button.onClick.AddListener (() => {
            EnemyInformationPanel.Open (enemy, int.Parse (button.transform.parent.parent.parent.name));
        });
    }

    public void ShowWaveList () {
        Game.UpdateDarkOverlay ();
        waveListParent.SetActive (true);

        foreach (GameObject obj in listContent) {
            Destroy (obj);
        }
        listContent = new List<GameObject> ();

        for (int i = 0; i < waves.Count; i++) {
            int trueIndex = i + waveMasteryIndex * waves.Count;

            GameObject newListPart = Instantiate (listPartPrefab);
            listContent.Add (newListPart);

            RectTransform newListTransform = newListPart.GetComponent<RectTransform> ();
            newListTransform.SetParent (listContentParent, false);

            newListTransform.Find ("Header").gameObject.GetComponent<Text> ().text = "Wave " + (trueIndex+1);
            UpdateUpcomingWaveScreen (waves[i], trueIndex, newListTransform);
        }

        listScrollRect.horizontalNormalizedPosition = waveNumber / (float)waves.Count;
    }

    public void CloseWaveList () {
        Game.UpdateDarkOverlay ();

        foreach (GameObject obj in listContent) {
            Destroy (obj);
        }
        listContent = new List<GameObject> ();

        waveListParent.SetActive (false);
    }

    public void StartWave () {
		if (waveNumber <= waves.Count) {

            waveStartedIndicator.GetComponentInParent<HoverContextElement> ().text = "Speed up the game";
            HoverContextElement.activeElement = null;

            upcomingHeader.text = "Remaining";

            wavePrebbing = false;
			waveStarted = true;
			waveStartedIndicator.color = Color.red;
			waveCounterIndicator.text = "Wave: " + ExternalWaveNumber.ToString ();
            gameProgress = GetProgressForWave (ExternalWaveNumber);
			ContinueWave (true);

            PlayerInput.cur.UpdateFlushBattlefieldHoverContextElement ();
        }
    }

    public static float GetProgressForWave (int wave) {
        return Mathf.Pow (cur.gameProgressSpeed, wave);
    }

    public float GetProgressForWaveFromInstance (int wave) {
        return Mathf.Pow (gameProgressSpeed, wave);
    }

    void ContinueWave (bool first) {

		endedIndex = 0;
		if (!first)
			subwaveNumber++;

		if (waves [waveNumber - 1].subwaves.Count > subwaveNumber) {
			currentSubwave = waves [waveNumber - 1].subwaves [subwaveNumber];
			spawnIndex = new int[currentSubwave.enemies.Count];

			for (int i = 0; i < currentSubwave.enemies.Count; i++) {
				SendMessage ("Spawn" + i.ToString ());
			}
			//Invoke ("ContinueFalseWave", currentSubwave.spawnTime + 2f);
		}
	}

	void ContinueFalseWave () {
		ContinueWave (false);
	}

    public void EndWave(bool finished) {
        PlayerInput.cur.flushBattlefieldAnimator.SetBool ("Flashing", false);
        ForceInstantCleanEnemyArray ();

        Game.CrossfadeMusic (Game.game.constructionMusic, 2f);
        SetPortals (false);

        Game.ChangeButtons (true);
		waveStarted = false;
		currentSubwave = null;
		subwaveNumber = 0;
        if (Game.fastGame)
            Game.ToggleFastGameSpeed ();
        waveStartedIndicator.color = Color.green;
        UpdateAmountModifier ();

        if (finished && Datastream.healthAmount > 0) {
            Game.credits += 25 * ExternalWaveNumber;
            PlayerInput.ChangeFlushTimer (-1);
            Game.game.SaveGame ("autosave");
            Datastream.healthAmount = Mathf.Min (Datastream.healthAmount + Datastream.healPerWave, Datastream.STARTING_HEALTH);
            Datastream.cur.UpdateNumberMaterials ();
        }

        if (waves.Count >= waveNumber + 1) {
			UpdateUpcomingWaveScreen (waves [waveNumber], ExternalWaveNumber, upcomingWindow);
		}

        waveCounterIndicator.text = "Wave: " + ExternalWaveNumber.ToString();
        if (Game.state == Game.State.Started)
            waveStartedIndicator.GetComponentInParent<HoverContextElement>().text = "Start wave " + (waveNumber + 1).ToString ();
        HoverContextElement.activeElement = null;
        PlayerInput.ChangeFlushTimer (0);
	}

	EnemySpawnPoint GetSpawnPosition (Enemy enemy) {
        return availableSpawns[Random.Range(0, availableSpawns.Count)];
	}

	void CreateEnemy (int index) {
        if (waveStarted) {
            Wave.Enemy enemy = currentSubwave.enemies[index];
            GameObject e = pooledEnemies[enemy][0];

            if (e) {
                e.SetActive (true);

                Enemy ene = e.GetComponent<Enemy> ();
                ene.active = true;
                e.tag = ene.type.ToString ();

                ene.spawnPoint = GetSpawnPosition (ene);
                ene.transform.position = ene.spawnPoint.worldPosition;

                ene.path = ene.spawnPoint.path;
            }

            pooledEnemies[enemy].RemoveAt (0);
            spawnIndex[index]++;

            if (spawnIndex[index] < currentSubwave.enemies[index].spawnAmount * amountModifier) {
                Invoke ("Spawn" + index.ToString (), currentSubwave.spawnTime / ((float)currentSubwave.enemies[index].spawnAmount * amountModifier));
            } else {
                endedIndex++;
                if (endedIndex == spawnIndex.Length) {
                    ContinueWave (false);
                }
            }
        } else {
            EndWave (false);
        }
	}

	public void SaveWaveset (Wave[] waves, string name) {
		string path = Game.WAVESET_SAVE_DIRECTORY + name + WAVESET_FILE_EXTENSION;
        StreamWriter write = new StreamWriter (path, false);

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

	public static List<Wave> LoadWaveset (string name) {
		string path = Game.WAVESET_SAVE_DIRECTORY + name + WAVESET_FILE_EXTENSION;
		string[] content = ModuleAssemblyLoader.GetContents (path);

		List<Wave> locWaves = new List<Wave> ();

		Wave cw = null;
		Wave.Subwave cs = null;
		Wave.Enemy ce = null;

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
					ce.enemy = EnemyManager.cur.GetEnemyFromName (c.Substring (8));
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

    void OnDrawGizmos () {
        if (waveStarted) {
            for (int i = 0; i < availableSpawns.Count; i++) {
                EnemySpawnPoint point = availableSpawns[i];
                for (int j = 0; j < point.path.Length - 1; j++) {
                    UnityEngine.Debug.DrawLine (point.path[j], point.path[j + 1], Color.red);
                }
            }
        }
        if (Game.game && Game.game.enemySpawnPoints != null)
            for (int i = 0; i < Game.game.enemySpawnPoints.Count; i++) {
                Gizmos.DrawSphere(Game.game.enemySpawnPoints[i].worldPosition, 0.5f);
                Gizmos.DrawSphere(Game.game.enemySpawnPoints[i].endPoint.worldPosition, 0.25f);
                Gizmos.DrawLine(Game.game.enemySpawnPoints[i].worldPosition, Game.game.enemySpawnPoints[i].endPoint.worldPosition);
            }
    }

	void Spawn0 () {
		int index = 0;
		CreateEnemy (index);
	}
	void Spawn1 () {
		int index = 1;
		CreateEnemy (index);
	}
	void Spawn2 () {
		int index = 2;
		CreateEnemy (index);
	}
	void Spawn3 () {
		int index = 3;
		CreateEnemy (index);
	}
	void Spawn4 () {
		int index = 4;
		CreateEnemy (index);
	}
	void Spawn5 () {
		int index = 5;
		CreateEnemy (index);
	}
	void Spawn6 () {
		int index = 6;
		CreateEnemy (index);
	}
	void Spawn7 () {
		int index = 7;
		CreateEnemy (index);
	}
}