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
	public Dijkstra pathfinding;
	public ResearchMenu researchMenu;
	public GameObject pauseMenu;
	
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

	// Use this for initialization
	void Start () {
		Debug.Log ("Initializing!");
		game = this;
		InitializeBattlefield ();
		pathfinding.InitializeDijkstraField (battlefieldWidth, battlefieldHeight);
		researchMenu.Initialize ();
		CalculatePowerLevel ();
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
		}else{
			isPaused = true;
			Time.timeScale = 0f;
			pauseMenu.SetActive (true);
		}
	}

	public static void ChangeWalls (Rect rect, bool doWall) {

		int startX = Mathf.RoundToInt (Game.game.pathfinding.WorldToNode (new Vector3 (rect.x,rect.y)).x);
		int startY = Mathf.RoundToInt (Game.game.pathfinding.WorldToNode (new Vector3 (rect.x,rect.y)).y);
		int w = Mathf.RoundToInt (rect.width);
		int h = Mathf.RoundToInt (rect.height);

		int cost = GetWallingCost (startX, startY, w, h, doWall);

		if (credits > cost) {
			for (int y = startY; y < startY + h; y++) {
				for (int x = startX; x < startX + w; x++) {
					isWalled[x,y] = doWall;
				}
			}

			Dijkstra.ChangeArea (rect, !doWall);
			Game.game.GenerateWallMesh ();
			credits -= cost;
		}
	}

	public static int GetWallingCost (int startX, int startY, int w, int h, bool doWall) {
		int cost = 0;

		for (int y = startY; y < startY + h; y++) {
			for (int x = startX; x < startX + w; x++) {
				if (doWall) {
					if (!isWalled[x,y])
						cost += 4;
				}else{
					if (isWalled[x,y])
						cost -= 2;
				}
			}
		}

		return cost;
	}

	public static void CalculatePowerLevel () {
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

	void InitializeBattlefield () {

		// Initialize files
		MODULE_ASSEMBLY_SAVE_DIRECTORY = Application.persistentDataPath + "/Module Assemblies/";
		WAVESET_SAVE_DIRECTORY = Application.persistentDataPath + "/Wave Sets/";
		BATTLEFIELD_SAVE_DIRECTORY = Application.persistentDataPath + "/Battlefield Sets/";

		if (!Directory.Exists (MODULE_ASSEMBLY_SAVE_DIRECTORY))
			Directory.CreateDirectory (MODULE_ASSEMBLY_SAVE_DIRECTORY);
	
		if (!Directory.Exists (WAVESET_SAVE_DIRECTORY))
			Directory.CreateDirectory (WAVESET_SAVE_DIRECTORY);

		if (!Directory.Exists (BATTLEFIELD_SAVE_DIRECTORY))
			Directory.CreateDirectory (BATTLEFIELD_SAVE_DIRECTORY);

		// Initialize resources
		credits = startingCredits;
		research = startingResearch;

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
		purchaseMenu.stockModules = new System.Collections.Generic.Dictionary<GameObject, int>();
		purchaseMenu.InitializePurchaseMenu (purchaseMenu.standard.ToArray ());
		purchaseMenu.InitialzeAssemblyButtons ();
		purchaseMenu.CloseAssemblyButtons ();
	}

	void Update () {

		if (datastream.pooledNumbers.Count <= 0) {
			RestartMap ();
		}

		researchText.text = "Research: " + research.ToString ();
		creditsText.text = "Credits: " + credits.ToString () + " LoC";
	}
}
