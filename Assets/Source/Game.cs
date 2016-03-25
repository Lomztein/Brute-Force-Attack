using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

public enum Scene { Play, AssemblyBuilder, WaveBuilder };
public enum Gamemode { Standard, GlassEnemies, TitaniumEnemies, SciencePrevails, GallifreyStands, VDay, Length }

public class Game : MonoBehaviour {

    public enum State { NotStarted, Started, Ended }

    public static Scene currentScene;

    [Header("Battlefield")]
    public Transform background;
    public int battlefieldWidth;
    public int battlefieldHeight;
    public List<EnemySpawnPoint> enemySpawnPoints;

    [Header("Difficulty Settings")]
    public int assembliesAllowed = 10;
    public float healthMultiplier = 1;
    public float amountMultiplier = 1;

    [Header("Gamemodes")]
    public Gamemode gamemode;
    public string[] gamemodeDescriptions;

    [Header("References")]
    public Datastream datastream;
    public EnemyManager enemySpawn;
    public PurchaseMenu purchaseMenu;
    public Pathfinding pathfinder;
    public Map pathMap;
    public ResearchMenu researchMenu;
    public GameObject pauseMenu;
    public Slider researchSlider;
    public GameObject rangeIndicator;
    public GameObject worldCanvas;
    public GameObject enemyHealthSlider;
    public GameObject GUICanvas;
    public static List<Module> currentModules = new List<Module>();
    public GameObject darkOverlay;
    public GameObject errorMessage;
    public Transform[] postStartGUI;

    public GameObject gameOverIndicator;
    public GameObject masteryModeIndicator;

    public Text creditsText;
    public Text powerText;
    public Text researchText;

    public static Game game;

    public LayerMask enemyLayer;
    public LayerMask moduleLayer;

    [Header("Resources")]
    public int startingCredits;
    public int startingResearch;

    private static bool _creditsUpdatedThisFrame = false;
    private static int _credits;
    public static int credits {
        get { return _credits; }
        set { _credits = value;
            if (!_creditsUpdatedThisFrame) {
                if (PurchaseMenu.cur)
                    PurchaseMenu.UpdateButtons();
                AssemblyCircleMenu.UpdateButtons();
            }
        }
    }

    private static bool _researchUpdatedThisFrame = false;
    public static float researchProgress;
    private static int _research;
    public static int research {
        get { return _research; }
        set { _research = value;
            if (!_researchUpdatedThisFrame) {
                if (Game.game.researchMenu) Game.game.researchMenu.UpdateButtons();
            }
        }
    }

    public static float powerPercentage;

    public enum WallType { Unbuildable = -1, None = 0, Player = 1, Level = 2, WithTurret = 3 } // None is no wall, player is playermade walls, and Level is permanemt, non-removable walls.
    public static WallType[,] isWalled;
    public MeshFilter wallMeshFilter;

    private Vector3[] verts;
    private int[] tris;
    private Vector3[] norms;
    private Vector2[] uvs;

    public static bool isPaused;

    public static string MODULE_ASSEMBLY_SAVE_DIRECTORY;
    public static string WAVESET_SAVE_DIRECTORY;
    public static string BATTLEFIELD_SAVE_DIRECTORY;
    public static string RESEARCH_BACKUP_DATA_DIRECTORY;
    public static string SAVED_GAME_DIRECTORY;
    public string[] stockModuleNames;

    [Header("Settings")]
    public static float musicVolume;
    public static float soundVolume;
    public GameObject optionsMenu;
    public static bool enableMouseMovement = true;
    public Toggle toggleMouseMovementButton;
    public Toggle toggleOptionsMenuButton;
    public Slider musicSlider;
    public Slider soundSlider;
    public static bool fastGame;
    public static State state;

    [Header("Default Assembly Roster Generator")]
    public Module[] baseModules;
    public Module[] rotatorModules;

    public void GenerateDefaultAssembly ( Module weaponModule ) {
        Assembly assembly = new Assembly();

        assembly.assemblyName = weaponModule.moduleName + " Turret";
        assembly.assemblyDesc = weaponModule.moduleDesc;

        assembly.parts.Add(new Assembly.Part(true, baseModules[weaponModule.moduleClass].moduleName, 1, 0, 0f, 0f, 90f));
        assembly.parts.Add(new Assembly.Part(false, rotatorModules[weaponModule.moduleClass].moduleName, 2, 1, 0f, 0f, 90f));
        assembly.parts.Add(new Assembly.Part(false, weaponModule.moduleName, 3, 2, 0f, 0f, 90f));

        Assembly.SaveToFile(assembly.assemblyName, assembly);
    }

    public static void ToggleDarkOverlay () {
        ForceDarkOverlay(!game.darkOverlay.activeSelf);
    }

    public static void ForceDarkOverlay ( bool setting ) {
        Animator anim = game.darkOverlay.GetComponent<Animator>();
        switch (setting) {

            case false:
                anim.Play("OverlayFadeOut");
                break;

            case true:
                anim.Play("OverlayFadeIn");
                break;

        }
    }

    public static void ShowErrorMessage (string message, float time) {
        game.StartCoroutine(game.ActualShowErrorMessage(message, time));
    }

    IEnumerator ActualShowErrorMessage (string message, float time) {
        errorMessage.SetActive(true);
        errorMessage.GetComponentInChildren<Text>().text = message;
        yield return new WaitForSeconds(time);
        errorMessage.SetActive(false);
    }

    // Use this for initialization
    void Awake () {

		game = this;
        ResearchMenu.cur = researchMenu;

        purchaseMenu.Initialize ();
        InitializeSaveDictionaries ();
        ModuleMod.currentMenu = new GameObject[ModuleMod.MAX_DEPTH];

        for (int i = 0; i < purchaseMenu.all.Count; i++) {
            Module mod = purchaseMenu.all[i].GetComponent<Module> ();
            if (mod.moduleType == Module.Type.Weapon) {
                GenerateDefaultAssembly (mod);
            }
        }

        ModuleAssemblyLoader.ConvertLegacyAssemblyFiles ();
        HideGUI();
	}

    private void InitializeResources () {
        ModuleTreeButton.buttonTypes = Resources.LoadAll<GameObject> ("Module Tree GUI");
    }

	public void StartGame () {
        Debug.Log ("Initializing!");
        ShowGUI();

        InitializeResources ();
        researchMenu.gameObject.SetActive (true);
        researchMenu.Initialize ();

        EnemyManager.cur.Initialize();
        InitializeBattlefield ();
        pathMap.Initialize ();
        Datastream.cur.Initialize();

        researchMenu.SaveBackup ();
        state = State.Started;
		Debug.Log ("Done initializing!");
	}

    void HideGUI () {
        for (int i = 0; i < postStartGUI.Length; i++) {
            postStartGUI[i].gameObject.SetActive(false);
        }
    }

    void ShowGUI () {
        for (int i = 0; i < postStartGUI.Length; i++) {
            postStartGUI[i].gameObject.SetActive(true);
        }
    }

    public void LoadGame () {
        SavedGame loaded = SavedGame.Load ("GAME");

        credits = loaded.credits;
        research = loaded.research;
        researchProgress = loaded.researchProgress;

        for (int i = 0; i < loaded.turrets.Count; i++) {

        }

    }
		
	public void RestartMap () {
        SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		Time.timeScale = 1f;
		EnemyManager.gameProgress = 1f;
	}

    public void QuitToDesktop () {
        Application.Quit ();
    }

    public static void ToggleFastGameSpeed () {
        HoverContextElement ele = EnemyManager.cur.waveStartedIndicator.GetComponentInParent<HoverContextElement> ();
        if (fastGame) {
            Time.timeScale = 1f;
            EnemyManager.cur.waveStartedIndicator.color = Color.red;
            ele.text = "Speed up the game";
        } else {
            Time.timeScale = 2f;
            EnemyManager.cur.waveStartedIndicator.color = Color.magenta;
            ele.text = "Slow down the game";
        }
        HoverContextElement.activeElement = null;
        fastGame = !fastGame;
    }

    public void LooseGame () {

    }

	public void TogglePause () {
		if (isPaused) {
			isPaused = false;
			Time.timeScale = 1f;
			pauseMenu.SetActive (false);
			optionsMenu.SetActive (false);
		}else{
			isPaused = true;
			Time.timeScale = 0f;
			pauseMenu.SetActive (true);
		}
        ForceDarkOverlay (isPaused);
	}

	public void ToggleOptionsMenu () {
		optionsMenu.SetActive (toggleOptionsMenuButton.isOn);
	}

	public void ToggleMouseMovement () {
		enableMouseMovement = toggleMouseMovementButton.isOn;
		if (enableMouseMovement) {
			toggleMouseMovementButton.transform.GetChild (0).GetComponent<Text>().text = "Mouse Movement";
		}else{
			toggleMouseMovementButton.transform.GetChild (0).GetComponent<Text>().text = "WASD Movement";
		}
	}

	public static Vector2 WorldToWallPos (Vector3 pos) {
		return new Vector2 (pos.x + game.battlefieldWidth / 2, pos.y + game.battlefieldHeight / 2);
	}

	/// <summary>
	/// Positivizes a rectangle which, contrary to how a rect is normally set up, should be with a start and end, instead of start and size.
	/// </summary>
	/// <returns>The positivized rect, with start and size.</returns>
	/// <param name="rect">Rect.</param>
	public static Rect PositivizeRect (Rect rect) {

		float startX = Mathf.Min (rect.x, rect.width);
		float startY = Mathf.Min (rect.y, rect.height);
		
		float endX = Mathf.Max (rect.x, rect.width);
		float endY = Mathf.Max (rect.y, rect.height);
		
		float w = endX - startX;
		float h = endY - startY;

		return new Rect (startX, startY, w, h);

	}

	public static void ChangeWalls (Rect rect, WallType wallType, bool force = false) {

		Rect loc = PositivizeRect (rect);

		int startX = Mathf.RoundToInt (loc.x);
		int startY = Mathf.RoundToInt (loc.y);

		int w = Mathf.RoundToInt (loc.width);
		int h = Mathf.RoundToInt (loc.height);

		int cost = GetWallingCost (startX, startY, w, h, wallType);

		if (credits > cost) {
			for (int y = startY; y < startY + h; y++) {
				for (int x = startX; x < startX + w; x++) {

                    int xx = x + game.battlefieldWidth / 2;
                    int yy = y + game.battlefieldHeight / 2;

					if ((Pathfinding.finder.IsInsideField (x + game.battlefieldWidth / 2, y + game.battlefieldHeight / 2)
                        && isWalled[xx,yy] != WallType.Level
                        && isWalled[xx,yy] != WallType.Unbuildable
                        && isWalled[xx,yy] != WallType.WithTurret) || force) {
						isWalled [xx, yy] = wallType;
					}
				}
			}

			//Pathfinding.ChangeArea (loc, !doWall);
			Game.game.GenerateWallMesh ();
			credits -= cost;
		}
	}

	public static int GetWallingCost (int startX, int startY, int w, int h, WallType wallType) {
		int cost = 0;

		for (int y = startY; y < startY + h; y++) {
			for (int x = startX; x < startX + w; x++) {

				if (Pathfinding.finder.IsInsideField (x + game.battlefieldWidth / 2,y + game.battlefieldHeight / 2)) {
                    if (wallType == WallType.Player) {
                        if (isWalled[x + game.battlefieldWidth / 2, y + game.battlefieldHeight / 2] == WallType.None)
                            cost += 4;
                    }
                    if (wallType == WallType.None) {
                        if (isWalled[x + game.battlefieldWidth / 2,y + game.battlefieldHeight / 2] == WallType.Player)
						    cost -= 2;
					}
				}
			}
		}

		return cost;
	}

	/* public static void CalculatePowerLevel () {
		//powerPercentage = PowerModule.CalculateTotalPowerGenerationSpeed () / Module.CalculateTotalPowerRequirements ();
		powerPercentage = 1f;

		string pName = "";
		if (powerPercentage > 2)
			pName = "Plentiful";

		if (powerPercentage >= 1.2f && powerPercentage < 2f)
			pName = "Formidable";

		if (powerPercentage >= 1f && powerPercentage < 1.2f)
			pName = "Good";

		if (powerPercentage >= 0.8f && powerPercentage < 1f)
			pName = "Low";

		if (powerPercentage >= 0.5f && powerPercentage < 0.8f)
			pName = "Critical";

		if (powerPercentage < 0.5f)
			pName = "Outage";

		Game.game.powerText.text = "Power: " + pName;
	}*/

	public static bool IsInsideBattlefield (Vector3 pos) {
		if (pos.x > game.battlefieldWidth / 2)
			return false;
		if (pos.x < -game.battlefieldWidth / 2)
			return false;
		if (pos.y > game.battlefieldHeight / 2)
			return false;
		if (pos.y < -game.battlefieldHeight / 2)
			return false;
		
		return true;
	}

	byte GetWallBitmask (int x, int y, WallType type) {
		byte mask = 0;
		if (pathfinder.IsInsideField (x + 1, y)) {
			if (isWalled [x + 1, y] == type)
				mask += 1;
		}
		if (pathfinder.IsInsideField (x, y + 1)) {
			if (isWalled [x, y + 1] == type)
				mask += 2;
		}
		if (pathfinder.IsInsideField (x - 1, y)) {
			if (isWalled [x - 1, y] == type)
				mask += 4;
		}
		if (pathfinder.IsInsideField (x, y - 1)) {
			if (isWalled [x, y - 1] == type)
				mask += 8;
		}

		return mask;
	}

	void GenerateWallMesh () {

		wallMeshFilter.transform.position = new Vector3 (-battlefieldWidth/2f, -battlefieldHeight/2f, background.transform.position.z - 1);
		wallMeshFilter.transform.localScale = new Vector3 (1f/background.localScale.x, 1f/background.localScale.y);
		
		verts = new Vector3[4 * battlefieldHeight * battlefieldWidth];
		tris  = new     int[6 * battlefieldHeight * battlefieldWidth];
		norms = new Vector3[verts.Length];
		uvs   = new Vector2[verts.Length];

		for (int y = 0; y < battlefieldHeight; y++) {
			for (int x = 0; x < battlefieldWidth; x++) {

				if (isWalled[x,y] == WallType.Player || isWalled[x,y] == WallType.WithTurret) {
					AddFace (x, y, x + battlefieldWidth * y, GetWallBitmask (x,y, WallType.Player), 1);
					pathfinder.map.nodes[x,y].isWalkable = false;
				}else if (isWalled[x,y] == WallType.Level) {
					AddFace (x, y, x + battlefieldWidth * y, GetWallBitmask (x,y, WallType.Level), 0);
					pathfinder.map.nodes[x,y].isWalkable = false;
				}else{
					pathfinder.map.nodes[x,y].isWalkable = true;
				}
			}
		}

		for (int i = 0; i < currentModules.Count; i++) {
			currentModules[i].BlockArea ();
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.normals = norms;
		mesh.uv = uvs;
	
		wallMeshFilter.mesh = mesh;
	}

	void AddFace (int x, int y, int index, int id, int horIndex) {

		verts[index * 4 + 0] = new Vector3 (x + 1 ,y + 1);
		verts[index * 4 + 1] = new Vector3 (x + 1 ,y);
		verts[index * 4 + 2] = new Vector3 (x ,y);
		verts[index * 4 + 3] = new Vector3 (x ,y + 1);

		tris[index * 6 + 0] = index * 4 + 0;
		tris[index * 6 + 1] = index * 4 + 1;
		tris[index * 6 + 2] = index * 4 + 2;

		tris[index * 6 + 3] = index * 4 + 3;
		tris[index * 6 + 4] = index * 4 + 0;
		tris[index * 6 + 5] = index * 4 + 2;

		float sizeX = 1f/16f;
		float sizeY = 0.5f;
		
		uvs[index * 4 + 0] = new Vector2 (id * sizeX + sizeX,	horIndex * sizeY + sizeY); 		//1,1
		uvs[index * 4 + 1] = new Vector2 (id * sizeX + sizeX,	horIndex * sizeY);   			//1,0
		uvs[index * 4 + 2] = new Vector2 (id * sizeX,			horIndex * sizeY); 				//0,0
		uvs[index * 4 + 3] = new Vector2 (id * sizeX,			horIndex * sizeY + sizeY); 		//0,1

		norms[index * 4 + 0] = Vector3.back;
		norms[index * 4 + 1] = Vector3.back;
		norms[index * 4 + 2] = Vector3.back;
		norms[index * 4 + 3] = Vector3.back;

	}

	public static void InitializeSaveDictionaries () {
		string dp = Application.dataPath;
		
		MODULE_ASSEMBLY_SAVE_DIRECTORY = dp + "/StreamingAssets/Module Assemblies/";
		WAVESET_SAVE_DIRECTORY = dp + "/StreamingAssets/Wave Sets/";
		BATTLEFIELD_SAVE_DIRECTORY = dp + "/StreamingAssets/Battlefield Sets/";
		RESEARCH_BACKUP_DATA_DIRECTORY = dp + "/Research Backup Data/";
		SAVED_GAME_DIRECTORY = dp + "/StreamingAssets/Saved Games/";
    }

	void InitializeBattlefield () {

		// Initialize files

		if (!Directory.Exists (MODULE_ASSEMBLY_SAVE_DIRECTORY))
			Directory.CreateDirectory (MODULE_ASSEMBLY_SAVE_DIRECTORY);
	
		if (!Directory.Exists (WAVESET_SAVE_DIRECTORY))
			Directory.CreateDirectory (WAVESET_SAVE_DIRECTORY);

		if (!Directory.Exists (BATTLEFIELD_SAVE_DIRECTORY))
			Directory.CreateDirectory (BATTLEFIELD_SAVE_DIRECTORY);

        if (!Directory.Exists (RESEARCH_BACKUP_DATA_DIRECTORY))
            Directory.CreateDirectory (RESEARCH_BACKUP_DATA_DIRECTORY);

        if (!Directory.Exists (SAVED_GAME_DIRECTORY))
            Directory.CreateDirectory (SAVED_GAME_DIRECTORY);

        // Load battlefield data
        isWalled = new WallType[battlefieldWidth,battlefieldHeight];
		currentModules = new List<Module>();

		// Initialize resources
		credits = startingCredits;
		research = startingResearch;
		researchProgress = 0f;

		// Initialize background graphic
		background.transform.localScale = new Vector3 (battlefieldWidth, battlefieldHeight, 1f);
		background.GetComponent<Renderer>().material.mainTextureScale = new Vector2 (battlefieldWidth / 2f, battlefieldHeight / 2f);

		// Initialize walls
		pathfinder.map.Initialize ();
		GenerateWallMesh ();
		
		// Initialize datastream graphic
		datastream.start = new Vector3 (-battlefieldWidth/2, -battlefieldHeight/2 + 3f);
		datastream.flyDistance = battlefieldWidth;
		datastream.transform.position = Vector3.down * (battlefieldHeight / 2 + 3f);

		// Initialize enemy spawn.
		GenerateDefaultSpawnpoints ();
        SaveBattlefieldData ("DEFAULT");

		// Initialize purchase menu
	}

	void FixedUpdate () {
		if (currentScene == Scene.Play) {
            musicVolume = musicSlider.value;
            soundVolume = soundSlider.value;

            if (state == State.Started && datastream.pooledNumbers.Count <= 0 && gameOverIndicator.activeSelf == false) {
                state = State.Ended;
                gameOverIndicator.SetActive(true);
            }

            researchText.text = "Research: " + research.ToString();
            creditsText.text = "Credits: " + credits.ToString() + " LoC";
            researchSlider.value = researchProgress;

            if (researchProgress > 1) {
                float excess = researchProgress - 1;
                researchProgress = excess;
                research++;
            }

            _creditsUpdatedThisFrame = false;
            _researchUpdatedThisFrame = false;
        }
	}

    void Update () {
        HoverContext.StaticUpdate();
    }

	public void SaveBattlefieldData (string fileName) {

		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (BATTLEFIELD_SAVE_DIRECTORY + fileName + ".dat");

		BattlefieldData data = new BattlefieldData (battlefieldWidth, battlefieldHeight, isWalled, enemySpawnPoints);
		bf.Serialize (file, data);
		file.Close ();

	}

	public BattlefieldData LoadBattlefieldData (string fileName, bool isFullPath = false) {

		string fullFile = BATTLEFIELD_SAVE_DIRECTORY + fileName + ".dat";
        if (isFullPath)
            fullFile = fileName;

		if (File.Exists (fullFile)) {

			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (fullFile, FileMode.Open);

			BattlefieldData data = (BattlefieldData)bf.Deserialize (file);
            data.spawns = new List<EnemySpawnPoint> ();
			file.Close ();

            Debug.Log (fileName);
			for (int i = 0; i < data.spawnsX.Count; i++) {

				Vector3 start = new Vector3 (data.spawnsX[i], data.spawnsY[i]);
				Vector3 end = new Vector3 (data.endsX[i], data.endsY[i]);

				EnemySpawnPoint spawn  = ScriptableObject.CreateInstance<EnemySpawnPoint>();
				EnemyEndPoint endPoint = ScriptableObject.CreateInstance<EnemyEndPoint>();

				spawn.worldPosition = start;
				endPoint.worldPosition = end;

				spawn.endPoint = endPoint;
                data.spawns.Add (spawn);
			}
		    return data;
        }
        return null;
    }

	void GenerateDefaultSpawnpoints () {
		for (int x = 1; x < battlefieldWidth + 1; x++) {
			EnemySpawnPoint spawn = ScriptableObject.CreateInstance<EnemySpawnPoint>();
			EnemyEndPoint end = ScriptableObject.CreateInstance<EnemyEndPoint>();

			spawn.worldPosition = new Vector3 (x - battlefieldWidth / 2f - 0.5f, battlefieldHeight / 2f - 0.5f);
			end.worldPosition = new Vector3 (x - battlefieldWidth / 2f - 0.5f, -battlefieldHeight / 2f - 0.5f);

			spawn.endPoint = end;
			enemySpawnPoints.Add (spawn);
		}
	}

	[Serializable]
	public class BattlefieldData {

        public string name;
        public string desc;

		public int width;
		public int height;
		public WallType[,] walls;

		public List<int> spawnsX;
		public List<int> spawnsY;
		public List<int> endsX;
		public List<int> endsY;

        [System.NonSerialized]
        public List<EnemySpawnPoint> spawns;

		public BattlefieldData (int _w, int _h, WallType[,] _walls, List<EnemySpawnPoint> _spawns) {

			width = _w;
			height = _h;
			walls = _walls;

			spawnsX = new List<int>();
			spawnsY = new List<int>();
			endsX   = new List<int>();
			endsY   = new List<int>();

			for (int i = 0; i < _spawns.Count; i++) {
				spawnsX.Add ((int)_spawns[i].worldPosition.x);
				spawnsY.Add ((int)_spawns[i].worldPosition.y);
				endsX.Add   ((int)_spawns[i].endPoint.worldPosition.x);
				endsY.Add   ((int)_spawns[i].endPoint.worldPosition.y);
			}
		}
	}

    public void SaveGame (string path) {
        SavedGame saved = new SavedGame ();

        for (int i = 0; i < currentModules.Count; i++) {
            if (currentModules[i].isRoot) {
                saved.turrets.Add (currentModules[i].assembly);
            }
        }

        saved.battlefieldData = new BattlefieldData (game.battlefieldWidth, game.battlefieldHeight, Game.isWalled, game.enemySpawnPoints);
        saved.waveSetPath = WAVESET_SAVE_DIRECTORY + "DEFAULT" + EnemyManager.WAVESET_FILE_EXTENSION;

        List<ResearchMenu.SimpleResearch> res = new List<ResearchMenu.SimpleResearch> ();
        for (int i = 0; i < researchMenu.research.Count; i++) {
            res.Add (new ResearchMenu.SimpleResearch (researchMenu.research[i]));
        }

        saved.wave = EnemyManager.cur.waveNumber;
        saved.credits = credits;
        saved.research = research;
        saved.researchProgress = researchProgress;

        saved.Save (path);
    }

    [System.Serializable]
    public class SavedGame {

        public BattlefieldData battlefieldData;
        public List<Assembly> turrets;
        public string waveSetPath;
        public List<ResearchMenu.SimpleResearch> researchSet;

        public int wave;
        public int credits;
        public int research;
        public float researchProgress;

        public void Save (string fileName) {
            BinaryFormatter bf = new BinaryFormatter ();
            FileStream file = File.Create (SAVED_GAME_DIRECTORY + fileName + ".dat");

            bf.Serialize (file, this);
            file.Close ();
        }

        public static SavedGame Load (string fileName) {
            if (File.Exists (fileName)) {

                BinaryFormatter bf = new BinaryFormatter ();
                FileStream file = File.Open (fileName, FileMode.Open);

                SavedGame data = (SavedGame)bf.Deserialize (file);
                file.Close ();

                return data;
            }

            return null;
        }
    }
}
