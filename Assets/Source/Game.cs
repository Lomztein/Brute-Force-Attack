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

	public static Game game;

	public LayerMask enemyLayer;
	public LayerMask moduleLayer;

	// Use this for initialization
	void Start () {
		Debug.Log ("Initializing!");
		game = this;
		InitializeBattlefield ();
		purchaseMenu.InitializePurchaseMenu ();
		pathfinding.InitializeDijkstraField (battlefieldWidth, battlefieldHeight);
		Debug.Log ("Done initializing!");
	}

	void InitializeBattlefield () {

		// Initialize background graphic
		background.transform.localScale = new Vector3 (battlefieldWidth, battlefieldHeight, 1f);
		background.GetComponent<Renderer>().material.mainTextureScale = new Vector2 (battlefieldWidth / 2f, battlefieldHeight / 2f);

		// Initialize datastream graphic
		datastream.start = new Vector3 (-battlefieldWidth/2, -battlefieldHeight/2 + 3f);
		datastream.flyDistance = battlefieldWidth;
		datastream.transform.position = Vector3.down * (battlefieldHeight / 2 + 3f);

		// Initialize enemy spawn
		enemySpawn.enemySpawnRect = new Rect (-battlefieldWidth/2, battlefieldHeight/2, battlefieldWidth, 3);
	}

	void Update () {

		if (datastream.pooledNumbers.Count <= 0) {
			Application.LoadLevel (Application.loadedLevel);
		}
	}
}
