using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public enum Scene { Menu, Play, AssemblyBuilder, WaveBuilder, BattlefieldEditor };
public enum Gamemode { Standard, GlassEnemies, TitaniumEnemies, SciencePrevails, GallifreyStands, VDay, Length }

public class Game : MonoBehaviour {

    public enum State { NotStarted, Started, Ended }

    public static Scene currentScene = Scene.Play;

    [Header("Battlefield")]
    public Transform background;
    public int battlefieldWidth;
    public int battlefieldHeight;
    public string battlefieldName;
    public List<EnemySpawnPoint> enemySpawnPoints;

    [Header("Difficulty Settings")]
    public static DifficultySettings difficulty;

    [Header("Gamemodes")]
    public Gamemode gamemode;
    public string[] gamemodeDescriptions;

    [Header("References")]
    public Datastream datastream;
    public EnemyManager enemySpawn;
    public PurchaseMenu purchaseMenu;
    public EnemyInformationPanel enemyInformationPanel;
    public Pathfinding pathfinder;
    public Map pathMap;
    public ResearchMenu researchMenu;
    public GameObject pauseMenu;
    public GameObject saveGameMenu;
    public Text saveGameText;
    public Button[] disabledDuringWaves;
    public string[] disabledDuringWavesText;
    public Slider researchSlider;
    public GameObject rangeIndicator;
    public GameObject worldCanvas;
    public GameObject enemyHealthSlider;
    public GameObject GUICanvas;
    public static List<Module> currentModules = new List<Module>();
    public GameObject darkOverlay;
    public static int darkOverlaySiblingIndex;
    public static bool darkOverlayActive = true;
    public GameObject errorMessage;
    public Transform[] postStartGUI;
    public GameObject turretExplosionParticle;
    public Button revertGameButton;
    public Highscore highscoreMenu;
    public SettingsMenu settingsMenu;

    public GameObject gameOverIndicator;
    public GameObject masteryModeIndicator;

    public Text creditsText;
    public Text powerText;
    public Text researchText;
    public Slider datastreamHealthSlider;
    public Text datastreamHealthText;

    public static Game game;

    public LayerMask enemyLayer;
    public LayerMask moduleLayer;

    [Header ("Audio")]
    public AudioSource mainAudioSource;
    public AudioClip constructionMusic;
    public AudioClip combatMusic;

    [Header("Resources")]
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
    public static string GAME_DATA_DIRECTORY;
    public string[] stockModuleNames;

    [Header("Settings")]
    public GameObject optionsMenu;
    public Toggle toggleMouseMovementButton;
    public Toggle toggleOptionsMenuButton;
    public static bool fastGame;
    public static State state;

    public static string saveToLoad;

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

    public static void UpdateDarkOverlay () {

        GameObject overlay = null;

        if (game) {
            overlay = game.darkOverlay;
        } else {
            overlay = GameObject.FindGameObjectWithTag ("DarkOverlay");
        }

        // Just ignore the call if it's already set the same.
        overlay.GetComponent<DarkOverlay>().StartCoroutine (DelayedUpdateDarkOverlay ());
        //col.enabled = setting;
    }

    public static bool AnyActiveAboveOverlay () {
        bool setting = false;

        GameObject overlay = null;

        if (game) {
            overlay = game.darkOverlay;
            darkOverlaySiblingIndex = overlay.transform.GetSiblingIndex ();
        } else {
            overlay = GameObject.FindGameObjectWithTag ("DarkOverlay");
            darkOverlaySiblingIndex = overlay.transform.GetSiblingIndex ();
        }

        int children = overlay.transform.parent.childCount;
        for (int i = darkOverlaySiblingIndex + 1; i < children; i++) {
            Transform loc = overlay.transform.parent.GetChild (i);
            if (loc.gameObject.activeSelf
                && Vector3.Distance (loc.position, Vector3.zero) < 5000) {
                setting = true;
            }
        }

        return setting;
    }

    public static IEnumerator DelayedUpdateDarkOverlay () {
        yield return new WaitForEndOfFrame ();
        yield return new WaitForEndOfFrame ();

        Animator anim = null;
        Image image = null;
        GameObject overlay = null;

        bool setting = AnyActiveAboveOverlay ();

        if (game) {
            overlay = game.darkOverlay;
            anim = overlay.GetComponent<Animator> ();
            image = overlay.GetComponent<Image> ();
        } else {
            overlay = GameObject.FindGameObjectWithTag ("DarkOverlay");
            anim = overlay.GetComponent<Animator> ();
            image = overlay.GetComponent<Image> ();
        }

        if (setting && currentScene == Scene.Play)
            PlayerInput.cur.CancelAll ();

        if (darkOverlayActive == setting)
            yield break;

        switch (setting) {

            case false:
                anim.Play ("OverlayFadeOut");
                break;

            case true:
                anim.Play ("OverlayFadeIn");
                break;
        }

        image.raycastTarget = setting;
        darkOverlayActive = setting;
    }

    public static void PlaySFXAudio (AudioClip audio) {
        game.mainAudioSource.PlayOneShot (audio, SettingsMenu.soundVolume);
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

    void ResetStatics () {
        currentModules.Clear ();
    }

    public void ReturnToMainMenu () {
        SceneManager.LoadScene ("pv_menu");
    }

    // Use this for initialization
    void Awake () {

        game = this;
        ResetStatics ();
        ResearchMenu.InitializeAllStatics ();
        InitializeBasics ();

        ModuleMod.currentMenu = new GameObject[ModuleMod.MAX_DEPTH];

        if (currentScene == Scene.Play) {
            for (int i = 0; i < purchaseMenu.all.Count; i++) {
                Module mod = purchaseMenu.all[i].GetComponent<Module> ();
                if (mod.moduleType == Module.Type.Weapon) {
                    GenerateDefaultAssembly (mod);
                }
            }
        }

        ModuleAssemblyLoader.ConvertLegacyAssemblyFiles ();
        HideGUI();
	}

    void Start () {
        if (saveToLoad != null && saveToLoad.Length > 0) {
            Debug.Log ("Loading save: " + saveToLoad);
            StartCoroutine (LoadSaveOnSceneStart ());
        }else {
            DeleteAutosave ();
        }
    }

    IEnumerator LoadSaveOnSceneStart () {
        // Wait a single frame to allow all other Awake() and Start() functions to run.
        yield return null;

        LoadSavedGame (saveToLoad);
        saveToLoad = "";
    }

    private void InitializeResources () {
        ModuleTreeButton.buttonTypes = Resources.LoadAll<GameObject> ("Module Tree GUI");
    }

    void InitializeBasics () {
        Debug.Log ("Initializing!");
        InitializeDirectories ();
        InitializeResources ();
        state = State.Started;
        Datastream.healthAmount = Datastream.STARTING_HEALTH;

        if (currentScene != Scene.BattlefieldEditor) {

            ResearchMenu.cur = researchMenu;
            purchaseMenu.Initialize ();

        }

        if (currentScene == Scene.Play) {

            researchMenu.SaveBackup ();
            researchMenu.gameObject.SetActive (true);
            researchMenu.Initialize ();
            enemyInformationPanel.Initialize ();
        }

        if (currentScene == Scene.Play || currentScene == Scene.Menu) {
            SettingsMenu.cur = settingsMenu;
            SettingsMenu.LoadSettings ();
        }

        if (Application.platform == RuntimePlatform.Android)
            gamemode = Gamemode.TitaniumEnemies;

        Debug.Log ("Done initializing!");
    }

    public void StartGame () {
        InitializeNewGame ();
        PostInitialization ();
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
		
	public void RestartMap () {
        SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		Time.timeScale = 1f;
		EnemyManager.gameProgress = 1f;
	}

    public void DeleteAutosave () {
        string path = SAVED_GAME_DIRECTORY + "autosave.dat";
        if (File.Exists (path))
            File.Delete (path);
    }

    public void QuitToMenu () {
        SceneManager.LoadScene (0);
        if (isPaused)
            TogglePause ();
        SetGameSpeed (1f);
    }

    public void OpenHighscore () {
        gameOverIndicator.SetActive (false);
        highscoreMenu.InstanceDisplay ();
        DeleteAutosave ();
    }

    public static void ToggleFastGameSpeed () {
        HoverContextElement ele = EnemyManager.cur.waveStartedIndicator.GetComponentInParent<HoverContextElement> ();
        if (ele) {
            if (fastGame) {
                SetGameSpeed (1f);
                EnemyManager.cur.waveStartedIndicator.color = Color.red;
                ele.text = "Speed up the game";
            } else {
                SetGameSpeed (2f);
                EnemyManager.cur.waveStartedIndicator.color = Color.magenta;
                ele.text = "Slow down the game";
            }
            HoverContextElement.activeElement = null;
            fastGame = !fastGame;
        }
    }

    public static void SetGameSpeed (float speed) {
        Time.timeScale = speed;
    }

    public void LooseGame () {

    }

    public void ClearBattlefieldGameObjects () {
        for (int i = 0; i < currentModules.Count; i++) {
            currentModules[i].DestroyModule (false);
        }
        currentModules.Clear ();
        EnemyManager.cur.EndWave (false);
    }

	public void TogglePause () {
		if (isPaused) {
			isPaused = false;
			Time.timeScale = 1f;
			pauseMenu.SetActive (false);
			optionsMenu.SetActive (false);
			saveGameMenu.SetActive (false);

            PlayerPrefs.SetFloat ("fMusicVolume", SettingsMenu.musicVolume);
            PlayerPrefs.SetFloat ("fSoundVolume", SettingsMenu.soundVolume);
        } else{
			isPaused = true;
			Time.timeScale = 0f;
			pauseMenu.SetActive (true);
		}
        UpdateDarkOverlay ();
	}

    public void ToggleSaveGameMenu () {
        saveGameMenu.SetActive (!saveGameMenu.activeSelf);
        saveGameText.text = "Untitled Save";
    }

    public static void ChangeButtons (bool state) {
        int index = 0;
        foreach (Button butt in game.disabledDuringWaves) {
            butt.interactable = state;
            HoverContextElement ele = butt.GetComponent<HoverContextElement> ();
            if (ele) {
                if (state) {
                    ele.text = butt.gameObject.name;
                } else {
                    ele.text = game.disabledDuringWavesText[index];
                }
                HoverContextElement.activeElement = null;
            }
            index++;
        }
        if (game.saveGameMenu.activeSelf)
            game.ToggleSaveGameMenu ();
    }

	public void ToggleOptionsMenu () {
		optionsMenu.SetActive (toggleOptionsMenuButton.isOn);
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

        if (currentScene == Scene.BattlefieldEditor) {
            force = true;
            if (wallType == WallType.Player)
                wallType = WallType.Level;
        }

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

					if ((game.IsInsideField (xx, yy)
                        && isWalled[xx,yy] != WallType.Level
                        && isWalled[xx,yy] != WallType.Unbuildable
                        && isWalled[xx,yy] != WallType.WithTurret) ||
                        (force && game.IsInsideField (xx, yy))) {
						isWalled [xx, yy] = wallType;
					}
				}
			}

			//Pathfinding.ChangeArea (loc, !doWall);
			Game.game.GenerateWallMesh ();
			credits -= cost;
		}
	}

    public void ExplodeAllTurrets () {
        List<Module> modules = new List<Module> ();
        for (int i = 0; i < currentModules.Count; i++) {
            if (currentModules[i].isRoot)
                modules.Add (currentModules[i]);
        }
        
        StartCoroutine (ExplodeTurrets (RandomizeOrder (modules)));
    }

    public static List<Module> RandomizeOrder ( List<Module> list ) {
        List<Module> result = new List<Module> ();
        while (list.Count != 0) {
            int index = UnityEngine.Random.Range (0, list.Count);
            result.Add (list[index]);
            list.RemoveAt (index);
        }
        return result;
    }

    IEnumerator ExplodeTurrets (List<Module> turrets) {
        for (int i = 0; i < turrets.Count; i++) {
            Module turret = turrets[i];
            Destroy (Instantiate (turretExplosionParticle, turret.transform.position, turret.transform.rotation), 2);
            turret.DestroyModule ();

            yield return new WaitForSeconds (UnityEngine.Random.Range (0.5f, 3));
        }
    }

    public bool IsInsideField ( int x, int y ) {
        if (x < 0 || x > battlefieldWidth - 1)
            return false;
        if (y < 0 || y > battlefieldHeight - 1)
            return false;
        return true;
    }

    public static int GetWallingCost (int startX, int startY, int w, int h, WallType wallType) {
		int cost = 0;

        if (Game.currentScene == Scene.BattlefieldEditor)
            return 0;

		for (int y = startY; y < startY + h; y++) {
			for (int x = startX; x < startX + w; x++) {

				if (Game.game.IsInsideField (x + game.battlefieldWidth / 2,y + game.battlefieldHeight / 2)) {
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
		if (IsInsideField (x + 1, y)) {
			if (isWalled [x + 1, y] == type)
				mask += 1;
		}
		if (IsInsideField (x, y + 1)) {
			if (isWalled [x, y + 1] == type)
				mask += 2;
		}
		if (IsInsideField (x - 1, y)) {
			if (isWalled [x - 1, y] == type)
				mask += 4;
		}
		if (IsInsideField (x, y - 1)) {
			if (isWalled [x, y - 1] == type)
				mask += 8;
		}

		return mask;
	}

    public void EnterAssemblyEditorFromGame () {
        if (fastGame)
            ToggleFastGameSpeed ();
        Time.timeScale = 1f;

        IngameEditors.AssemblyEditorScene.openedFromIngame = true;
        SaveGame ("assemblysave");
        SceneManager.LoadScene ("pv_assemblybuilder");
    }

    public static void DeleteAssemblySave () {
        File.Delete (SAVED_GAME_DIRECTORY + "assemblysave.dat");
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
					if (pathfinder)
                        pathfinder.map.nodes[x,y].isWalkable = false;
				}else if (isWalled[x,y] == WallType.Level) {
					AddFace (x, y, x + battlefieldWidth * y, GetWallBitmask (x,y, WallType.Level), 0);
                    if (pathfinder)
                        pathfinder.map.nodes[x,y].isWalkable = false;
				}else if (pathfinder) {
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

        float sizeXmod = sizeX / 16f;
        float sizeYmod = sizeY / 16f;

        uvs[index * 4 + 0] = new Vector2 (id * sizeX + sizeX - sizeXmod,	horIndex * sizeY + sizeY - sizeYmod); 		//1,1
		uvs[index * 4 + 1] = new Vector2 (id * sizeX + sizeX - sizeXmod,	horIndex * sizeY + sizeYmod);   			//1,0
		uvs[index * 4 + 2] = new Vector2 (id * sizeX + sizeXmod,			horIndex * sizeY + sizeYmod); 				//0,0
		uvs[index * 4 + 3] = new Vector2 (id * sizeX + sizeXmod,			horIndex * sizeY + sizeY - sizeYmod); 		//0,1

		norms[index * 4 + 0] = Vector3.back;
		norms[index * 4 + 1] = Vector3.back;
		norms[index * 4 + 2] = Vector3.back;
		norms[index * 4 + 3] = Vector3.back;

	}

    void InitializeNewGame () {
        // Initialize resources
        credits = difficulty.startingCredits;
        research = difficulty.startingResearch;
        researchProgress = 0f;

        Datastream.healthAmount = Datastream.STARTING_HEALTH;

        if (isWalled == null)
            isWalled = new WallType[battlefieldWidth, battlefieldHeight];
    }

    public static void InitializeDirectories () {

        string dp = Application.dataPath;

        switch (Application.platform) {

            default:
                MODULE_ASSEMBLY_SAVE_DIRECTORY = dp + "/Player Data/Module Assemblies/";
                WAVESET_SAVE_DIRECTORY = dp + "/StreamingAssets/Wave Sets/";
                BATTLEFIELD_SAVE_DIRECTORY = dp + "/StreamingAssets/Battlefield Sets/";
                RESEARCH_BACKUP_DATA_DIRECTORY = dp + "/Research Backup Data/";
                SAVED_GAME_DIRECTORY = dp + "/Player Data/Saved Games/";
                GAME_DATA_DIRECTORY = dp + "/Player Data/Highscores/";
                break;

        }

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

        if (!Directory.Exists (GAME_DATA_DIRECTORY))
            Directory.CreateDirectory (GAME_DATA_DIRECTORY);
    }

    void PostInitialization () {
        ShowGUI ();

        // Initialize background graphic
        background.transform.localScale = new Vector3 (battlefieldWidth, battlefieldHeight, 1f);
		background.GetComponent<Renderer>().material.mainTextureScale = new Vector2 (battlefieldWidth / 2f, battlefieldHeight / 2f);

		// Initialize walls
		pathfinder.map.Initialize ();
        wallMeshFilter.GetComponent<MeshRenderer> ().enabled = true;
		GenerateWallMesh ();

        // Initialize datastream graphic
        datastream.transform.position = Vector3.down * (battlefieldHeight / 2 + 3);
        datastream.GetComponent<BoxCollider> ().center += Vector3.up * 6f / battlefieldHeight;
        datastream.start = new Vector3 (-battlefieldWidth / 2, -battlefieldHeight / 2 + 3f);
        datastream.flyDistance = battlefieldWidth;
        datastream.flySpeed = UnityEngine.Random.Range (5f, 10f);
        datastream.transform.position = Vector3.down * (battlefieldHeight / 2 + 3f);
        datastream.Initialize ();

        PurchaseMenu.cur.InitializeAssemblyButtons ();

        UpdateDarkOverlay ();
        AssembliesSelectionMenu.cur.gameObject.SetActive (false);
        EnemyManager.Initialize ();

        //researchMenu.InvalidateUselessButtons ();
        Game.DeleteAssemblySave ();
        SettingsMenu.cur.UpdateBloom ();

        if (fastGame)
            ToggleFastGameSpeed ();
    }

    void FixedUpdate () {
		if (currentScene == Scene.Play) {
            if (state == State.Started && Datastream.healthAmount <= 0 && gameOverIndicator.activeSelf == false) {
                Debug.Log ("Game has ended.");
                state = State.Ended;
                UpdateDarkOverlay ();
                HideGUI ();
                gameOverIndicator.SetActive(true);
                ExplodeAllTurrets ();

                revertGameButton.interactable = File.Exists (SAVED_GAME_DIRECTORY + "autosave.dat");
            }

            researchText.text = "Research: " + research.ToString();
            creditsText.text = "Credits: " + credits.ToString() + " LoC";
            datastreamHealthSlider.value = Datastream.healthAmount / (float)Datastream.STARTING_HEALTH;
            datastreamHealthText.text = "DATASTREAM CORRUPTION AT <b>" + ((1 - (Datastream.healthAmount / (float)Datastream.STARTING_HEALTH)) * 100).ToString () + " PERCENT</b>";
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

    public static void CrossfadeMusic (AudioClip newMusic, float fadeTime) {
        game.StartCoroutine (game.CFMUSIC (newMusic, fadeTime));
    }

    private IEnumerator CFMUSIC (AudioClip newMusic, float fadeTime) {
        if (mainAudioSource.clip == newMusic)
            yield break;

        int steps = Mathf.RoundToInt (fadeTime / Time.fixedDeltaTime) / 2;
        for (int i = 0; i < steps; i++) {
            mainAudioSource.volume = Mathf.Min (1f - (i / (float)steps) * SettingsMenu.musicVolume, SettingsMenu.cur.musicSlider.value);
            yield return new WaitForFixedUpdate ();
        }

        mainAudioSource.volume = 0f;

        mainAudioSource.Stop ();
        mainAudioSource.clip = newMusic;
        mainAudioSource.Play ();

        mainAudioSource.volume = SettingsMenu.musicVolume;

        for (int i = 0; i < steps; i++) {
            mainAudioSource.volume = Mathf.Min ((i / (float)steps) * SettingsMenu.musicVolume, SettingsMenu.cur.musicSlider.value);
        yield return new WaitForFixedUpdate ();
        }
    }

    void Update () {
        HoverContext.StaticUpdate();

        if (isPaused) {
            mainAudioSource.volume = SettingsMenu.musicVolume;
        }
    }

	public void SaveBattlefieldData (string fileName) {

        if (File.Exists (BATTLEFIELD_SAVE_DIRECTORY + fileName + ".dat")) {
            Debug.LogWarning("Tried to override file.");
            return;
        }

		BattlefieldData data = new BattlefieldData (fileName, "", battlefieldWidth, battlefieldHeight, isWalled, enemySpawnPoints);
        Utility.SaveObjectToFile (BATTLEFIELD_SAVE_DIRECTORY + fileName + ".dat", data);
	}

	public BattlefieldData LoadBattlefieldData (string fileName, bool isFullPath = false) {

		string fullFile = BATTLEFIELD_SAVE_DIRECTORY + fileName + ".dat";
        if (isFullPath)
            fullFile = fileName;

        BattlefieldData data = Utility.LoadObjectFromFile<BattlefieldData> (fullFile);

		if (data != null) {
            data.spawns = new List<EnemySpawnPoint> ();
			for (int i = 0; i < data.spawnsX.Length; i++) {

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
		public Game.WallType[,] walls;

		public float[] spawnsX;
		public float[] spawnsY;
		public float[] endsX;
		public float[] endsY;

        [System.NonSerialized]
        public List<EnemySpawnPoint> spawns;

		public BattlefieldData (string name, string desc, int _w, int _h, WallType[,] _walls, List<EnemySpawnPoint> _spawns) {

            this.name = name;
            this.desc = desc;

			width = _w;
			height = _h;
            walls = _walls;

            spawnsX = new float[_spawns.Count];
			spawnsY = new float[_spawns.Count];
            endsX   = new float[_spawns.Count];
            endsY   = new float[_spawns.Count];

            for (int i = 0; i < _spawns.Count; i++) {
                spawnsX[i] = _spawns[i].worldPosition.x;
				spawnsY[i] = _spawns[i].worldPosition.y;
				endsX[i] = _spawns[i].endPoint.worldPosition.x;
				endsY[i] = _spawns[i].endPoint.worldPosition.y;
			}
		}
	}

    public void LoadSavedGame (string fileName) {

        // First, clear battlefield of gameobjects, and load the data into a SavedGame object.
        // ClearBattlefieldGameObjects ();
        SavedGame sg = SavedGame.Load (fileName);

        // Load basic battlefield size and walls.
        battlefieldWidth = sg.battlefieldData.width;
        battlefieldHeight = sg.battlefieldData.height;
        isWalled = sg.battlefieldData.walls;
        difficulty = sg.difficulty;
        battlefieldName = sg.battlefieldData.name;

        // Set purchaseables and spawnpoints.
        if (IngameEditors.AssemblyEditorScene.newAssemblies != null) {
            sg.selectedTurrets.AddRange (IngameEditors.AssemblyEditorScene.newAssemblies);
            IngameEditors.AssemblyEditorScene.newAssemblies.Clear ();
        }

        purchaseMenu.SetAssemblies (sg.selectedTurrets);
        enemySpawnPoints = new List<EnemySpawnPoint> ();

        for (int i = 0; i < sg.battlefieldData.spawnsX.Length; i++) {
            EnemySpawnPoint sp = ScriptableObject.CreateInstance<EnemySpawnPoint> ();
            sp.worldPosition = new Vector3 (sg.battlefieldData.spawnsX[i], sg.battlefieldData.spawnsY[i]);
            sp.endPoint = ScriptableObject.CreateInstance<EnemyEndPoint> ();
            sp.endPoint.worldPosition = new Vector3 (sg.battlefieldData.endsX[i], sg.battlefieldData.endsY[i]);
            enemySpawnPoints.Add (sp);
        }

        // Load in-world turrets.
        for (int i = 0; i < sg.turrets.Count; i++) {
            SavedGame.SavedAssembly ass = sg.turrets[i];
            ModuleAssemblyLoader loader = ((GameObject)Instantiate (purchaseMenu.assemblyLoader)).GetComponent<ModuleAssemblyLoader> ();
            GameObject root = loader.LoadAssembly (ass.assembly, true);
            root.transform.position = new Vector3 (ass.posX, ass.posY);
            root.transform.rotation = Quaternion.Euler (0, 0, ass.rot);
            root.transform.parent = null;

            Module rootModule = root.GetComponent<Module> ();
            rootModule.score = sg.turrets[i].score;
            for (int j = 0; j < rootModule.modules.Count; j++) {

                rootModule.modules[j].SetStartingUpgradeCost ();

                for (int a = 0; a < ass.levels.Count; a++) {

                    if (rootModule.modules[j].moduleType == (Module.Type)a) {
                        for (int b = 0; b < ass.levels[a]; b++) {
                            rootModule.modules[j].UpgradeModule ();

                            // So many bloody loops, though this should now save assembly upgrades in the SavedData data.
                            // Haha, ass.
                        }
                    }
                }
            }
        }

        // Load research. Likely a very unefficient method, but it should do without issues.
        for (int i = 0; i < sg.researchedResearch.Count; i++) {
            Research r = ResearchMenu.cur.research[sg.researchedResearch[i]];
            r.Purchase (true);
        }

        // Set resources.
        credits = sg.credits;
        research = sg.research;
        researchProgress = sg.researchProgress;
        PlayerInput.flushTimer = sg.flushTimer;

        // Finalize loading.
        Datastream.healthAmount = sg.health;
        PostInitialization ();

        EnemyManager.cur.waveMastery = sg.masteryNumber;
        EnemyManager.cur.waveNumber = sg.waveNumber;
        EnemyManager.externalWaveNumber = sg.totalWaveNumber;
        EnemyManager.gameProgress = sg.gameProgress;
        EnemyManager.cur.enemiesKilled = sg.enemiesKilled;

        EnemyManager.cur.EndWave (false);
    }

    public void RetryAutosave () {
        saveToLoad = SAVED_GAME_DIRECTORY + "autosave.dat";
        RestartMap ();
    }

    public void SavePauseGame () {
        SaveGame (saveGameText.text);
        ToggleSaveGameMenu ();
    }

    public void SaveGame (string path) {
        SavedGame saved = new SavedGame ();

        saved.turrets = new List<SavedGame.SavedAssembly> ();
        saved.difficulty = difficulty;
        saved.selectedTurrets = purchaseMenu.GetAssemblies (); 

        for (int i = 0; i < currentModules.Count; i++) {
            if (currentModules[i].isRoot) {
                saved.turrets.Add (new SavedGame.SavedAssembly (currentModules[i]));
            }
        }

        saved.battlefieldData = new BattlefieldData (battlefieldName, "", game.battlefieldWidth, game.battlefieldHeight, Game.isWalled, game.enemySpawnPoints);
        saved.waveSetPath = WAVESET_SAVE_DIRECTORY + "DEFAULT" + EnemyManager.WAVESET_FILE_EXTENSION;

        saved.researchedResearch = new List<int> ();

        for (int i = 0; i < researchMenu.research.Count; i++) {
            if (ResearchMenu.cur.research[i].isBought)
                saved.researchedResearch.Add (ResearchMenu.cur.research[i].index);
        }

        saved.credits = credits;
        saved.research = research;
        saved.researchProgress = researchProgress;
        saved.flushTimer = PlayerInput.flushTimer;

        saved.waveNumber = EnemyManager.cur.waveNumber;
        saved.totalWaveNumber = EnemyManager.externalWaveNumber;
        saved.masteryNumber = EnemyManager.cur.waveMastery;
        saved.gameProgress = EnemyManager.gameProgress;
        saved.enemiesKilled = EnemyManager.cur.enemiesKilled;

        saved.health = Datastream.healthAmount;

        saved.Save (path);
    }

    [System.Serializable]
    public class SavedGame {

        public BattlefieldData battlefieldData;

        public List<Assembly> selectedTurrets;
        public List<SavedAssembly> turrets;

        public string waveSetPath;
        public List<int> researchedResearch;

        public int credits;
        public int research;
        public float researchProgress;
        public int flushTimer;

        public int waveNumber;
        public int masteryNumber;
        public int totalWaveNumber;
        public float gameProgress;
        public Dictionary<string, int> enemiesKilled;

        public DifficultySettings difficulty;

        public int health;

        public void Save (string fileName) {
            Utility.SaveObjectToFile (SAVED_GAME_DIRECTORY + fileName + ".dat", this);
        }

        public static SavedGame Load (string fileName) {
            string fullFile = fileName;
            SavedGame data = Utility.LoadObjectFromFile<SavedGame> (fullFile);

            return data;
        }

        [Serializable]
        public class SavedAssembly {

            public Assembly assembly;

            public float posX;
            public float posY;
            public float rot;
            public int score;

            public List<int> levels = new List<int>();

            public SavedAssembly (Module rootModule) {
                assembly = rootModule.assembly;
                posX = rootModule.transform.localPosition.x;
                posY = rootModule.transform.localPosition.y;
                rot = rootModule.transform.localEulerAngles.z;
                score = rootModule.score;
                for (int i = 0; i < 3; i++) {
                    levels.Add (rootModule.GetAssemblyUpgradeLevel ((Module.Type)i));
                }
            }
        }
    }

    [Serializable]
    public class DifficultySettings {

        public string name;
        [TextArea]
        public string desc;

        public int amountMultiplier;
        public float healthMultiplier;
        public int startingCredits;
        public int startingResearch;
        public int researchPerRound = 1;
        public float weaponDamageToDifferentColor = 2f;

    }
}
