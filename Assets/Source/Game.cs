using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Game : MonoBehaviour {

	public Transform background;
	public int battlefieldWidth;
	public int battlefieldHeight;

	public Datastream datastream;
	public EnemySpawn enemySpawn;
	public PurchaseMenu purchaseMenu;
	public Dijkstra pathfinding;
	public ResearchMenu researchMenu;

	public Text creditsText;
	public Text powerText;
	public Text researchText;

	public static Game game;

	public LayerMask enemyLayer;
	public LayerMask moduleLayer;

	public int startingCredits;
	public static int credits;
	public static int research;
	public static float powerPercentage;

	public static bool[,] isWalled;
	public MeshFilter wallMeshFilter;

	private Vector3[] verts;
	private int[] tris;
	private Vector3[] norms;
	private Vector2[] uvs;

	public static bool isPaused;
	public GameObject pauseMenu;

	// Use this for initialization
	void Start () {
		Debug.Log ("Initializing!");
		game = this;
		InitializeBattlefield ();
		purchaseMenu.InitializePurchaseMenu ();
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
			pauseMenu.SetActive (false);
			Time.timeScale = 1f;
		}else{
			isPaused = true;
			pauseMenu.SetActive (true);
			Time.timeScale = 0f;
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
						cost += 10;
				}else{
					if (isWalled[x,y])
						cost -= 5;
				}
			}
		}

		return cost;
	}

	public static void CalculatePowerLevel () {
		powerPercentage = PowerModule.CalculateTotalPowerGenerationSpeed () / Module.CalculateTotalPowerRequirements () * 100f;
		Game.game.powerText.text = "Power: " + powerPercentage.ToString () + "%";
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

		// Initialize resources
		credits = startingCredits;
		research = 0;

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
	}

	void Update () {

		if (datastream.pooledNumbers.Count <= 0) {
			RestartMap ();
		}

		researchText.text = "Research: " + research.ToString ();
		creditsText.text = "Credits: " + credits.ToString () + " LoC";
	}
}
