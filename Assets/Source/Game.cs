using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class Game : MonoBehaviour {

	[Header ("Battlefield")]
	public Transform background;
	public int battlefieldWidth;
	public int battlefieldHeight;

	[Header ("References")]
	public Datastream datastream;
	public EnemySpawn enemySpawn;
	public PurchaseMenu purchaseMenu;
	public Pathfinding pathfinder;
	public Map pathMap;
	public ResearchMenu researchMenu;
	public GameObject pauseMenu;
	public Slider researchSlider;
	public GameObject rangeIndicator;
	public GameObject worldCanvas;
	public GameObject enemyHealthSlider;
	
	public Text creditsText;
	public Text powerText;
	public Text researchText;

	public static Game game;
	
	public LayerMask enemyLayer;
	public LayerMask moduleLayer;

	[Header ("Resources")]
	public int startingCredits;
	public int startingResearch;

	private static int _credits;
	public static int credits {
		get { return _credits; }
		set { _credits = value;
			if (PurchaseMenu.cur)
				PurchaseMenu.UpdateButtons ();
		}
	}

	public static float researchProgress;
	private static int _research;
	public static int research {
		get { return _research; }
		set { _research = value;
			Game.game.researchMenu.UpdateButtons ();
		}
	}

	public static float powerPercentage;

	public static bool[,] isWalled;
	public MeshFilter wallMeshFilter;

	private Vector3[] verts;
	private int[] tris;
	private Vector3[] norms;
	private Vector2[] uvs;

	public static bool isPaused;

	public static string MODULE_ASSEMBLY_SAVE_DIRECTORY;
	public static string WAVESET_SAVE_DIRECTORY;
	public static string BATTLEFIELD_SAVE_DIRECTORY;
	public string[] stockModuleNames;

	[Header ("Settings")]
	public static float musicVolume;
	public static float soundVolume;
	public GameObject optionsMenu;
	public static bool enableMouseMovement = true;
	public Toggle toggleMouseMovementButton;
	public Toggle toggleOptionsMenuButton;
	public Slider musicSlider;
	public Slider soundSlider;

	// Use this for initialization
	void Start () {
		Debug.Log ("Initializing!");
		game = this;
		InitializeBattlefield ();
		pathMap.Initialize ();
		researchMenu.Initialize ();
		Debug.Log ("Done initializing!");
	}

	public void RestartMap () {
		Application.LoadLevel (Application.loadedLevel);
		Time.timeScale = 1f;
		EnemySpawn.gameProgress = 1f;
	}

	public void QuitToDesktop () {
		Application.Quit ();
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

	public static void ChangeWalls (Rect rect, bool doWall) {

		Rect loc = PositivizeRect (rect);

		int startX = (int)loc.x;
		int startY = (int)loc.y;

		int w = (int)loc.width;
		int h = (int)loc.height;

		int cost = GetWallingCost (startX, startY, w, h, doWall);

		if (credits > cost) {
			for (int y = startY; y < startY + h; y++) {
				for (int x = startX; x < startX + w; x++) {
					if (Pathfinding.finder.IsInsideField (x + game.battlefieldWidth / 2, y + game.battlefieldHeight / 2)) {
						isWalled [x + game.battlefieldWidth / 2, y + game.battlefieldHeight / 2] = doWall;	
					}
				}
			}

			Pathfinding.ChangeArea (loc, !doWall);
			Game.game.GenerateWallMesh ();
			credits -= cost;
		}
	}

	public static int GetWallingCost (int startX, int startY, int w, int h, bool doWall) {
		int cost = 0;

		for (int y = startY; y < startY + h; y++) {
			for (int x = startX; x < startX + w; x++) {

				if (Pathfinding.finder.IsInsideField (x + game.battlefieldWidth / 2,y + game.battlefieldHeight / 2)) {
					if (doWall) {
						if (!isWalled[x + game.battlefieldWidth / 2,y + game.battlefieldHeight / 2])
							cost += 4;
					}else{
						if (isWalled[x + game.battlefieldWidth / 2,y + game.battlefieldHeight / 2])
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

	void GenerateWallMesh () {

		wallMeshFilter.transform.position = new Vector3 (-battlefieldWidth/2f, -battlefieldHeight/2f, background.transform.position.z - 1);
		wallMeshFilter.transform.localScale = new Vector3 (1f/background.localScale.x, 1f/background.localScale.y);
		
		verts = new Vector3[4 * battlefieldHeight * battlefieldWidth];
		tris  = new     int[6 * battlefieldHeight * battlefieldWidth];
		norms = new Vector3[verts.Length];
		uvs   = new Vector2[verts.Length];

		for (int y = 0; y < battlefieldHeight; y++) {
			for (int x = 0; x < battlefieldWidth; x++) {

				if (isWalled[x,y]) {
					AddFace (x, y, x + battlefieldWidth * y);
				}

			}
		}

		Mesh mesh = new Mesh ();
		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.normals = norms;
		mesh.uv = uvs;
	
		wallMeshFilter.mesh = mesh;
	}

	void AddFace (int x, int y, int index) {

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

		uvs[index * 4 + 0] = new Vector2 (1f,1f);
		uvs[index * 4 + 1] = new Vector2 (1f,0f);
		uvs[index * 4 + 2] = new Vector2 (0f,0f);
		uvs[index * 4 + 3] = new Vector2 (0f,1f);

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
	}

	void InitializeBattlefield () {

		// Initialize files
		InitializeSaveDictionaries ();

		if (!Directory.Exists (MODULE_ASSEMBLY_SAVE_DIRECTORY))
			Directory.CreateDirectory (MODULE_ASSEMBLY_SAVE_DIRECTORY);
	
		if (!Directory.Exists (WAVESET_SAVE_DIRECTORY))
			Directory.CreateDirectory (WAVESET_SAVE_DIRECTORY);

		if (!Directory.Exists (BATTLEFIELD_SAVE_DIRECTORY))
			Directory.CreateDirectory (BATTLEFIELD_SAVE_DIRECTORY);

		// Initialize resources
		credits = startingCredits;
		research = startingResearch;
		researchProgress = 0f;

		// Initialize background graphic
		background.transform.localScale = new Vector3 (battlefieldWidth, battlefieldHeight, 1f);
		background.GetComponent<Renderer>().material.mainTextureScale = new Vector2 (battlefieldWidth / 2f, battlefieldHeight / 2f);

		// Initialize datastream graphic
		datastream.start = new Vector3 (-battlefieldWidth/2, -battlefieldHeight/2 + 3f);
		datastream.flyDistance = battlefieldWidth;
		datastream.transform.position = Vector3.down * (battlefieldHeight / 2 + 3f);

		// Initialize enemy spawn
		enemySpawn.enemySpawnRect = new Rect (-battlefieldWidth/2, battlefieldHeight/2, battlefieldWidth, 3);

		// Initialize walls
		isWalled = new bool[battlefieldWidth,battlefieldHeight];
		GenerateWallMesh ();

		// Initialize purchase menu
		purchaseMenu.Initialize ();
	}

	void FixedUpdate () {

		musicVolume = musicSlider.value;
		soundVolume = soundSlider.value;

		if (datastream.pooledNumbers.Count <= 0) {
			RestartMap ();
		}

		researchText.text = "Research: " + research.ToString ();
		creditsText.text = "Credits: " + credits.ToString () + " LoC";
		researchSlider.value = researchProgress;

		if (researchProgress > 1) {
			float excess = researchProgress - 1;
			researchProgress = excess;
			research++;
		}
	}
}
