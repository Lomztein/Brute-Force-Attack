using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Datastream : MonoBehaviour {

    public static Datastream cur;

	public Vector3 start;
	public float flyDistance;
	public float flySpeed;

	public GameObject[] numbers;

	public List<GameObject> pooledNumbers;
	public List<GameObject> curruptNumbers;

	public static bool repurposeEnemies;
	public static bool enableFirewall;

	public static int healthAmount = 100;
    public const int STARTING_HEALTH = 100;

	private float healProgress;
	public static float healSpeed;

    public LineRenderer lineRenderer;

	// TODO Implement pooling of ones and zeros.

	void Start () {
        cur = this;
	}

    public void Initialize () {
        StartCoroutine("InitializeNumbers");
        Game.currentScene = Scene.Play;
    }

    public void Reset (int life = STARTING_HEALTH) {
        healthAmount = life;
        for (int i = 0; i < pooledNumbers.Count; i++) {
            Destroy (pooledNumbers[i].gameObject);
        }
        for (int i = 0; i < curruptNumbers.Count; i++) {
            Destroy (curruptNumbers[i].gameObject);
        }

        pooledNumbers = new List<GameObject> ();
        curruptNumbers = new List<GameObject> ();
        Initialize ();
    }

	void FixedUpdate () {
        if (EnemyManager.waveStarted) {
            healProgress *= healSpeed * Time.fixedDeltaTime;

            if (healProgress > 1f) {
			    GameObject n = curruptNumbers[Random.Range (0, curruptNumbers.Count)];
			    n.GetComponent<Renderer>().material.color = Color.green;
                curruptNumbers.Remove(n);
			    pooledNumbers.Add (n);
		    }
        }
    }

    IEnumerator InitializeNumbers () {

        lineRenderer.SetPosition(0, start + Vector3.back);
        lineRenderer.SetPosition(1, start + Vector3.right * flyDistance + Vector3.back);

        Rect rect = new Rect(-Game.game.battlefieldWidth / 2, -Game.game.battlefieldHeight / 2 + 6, Game.game.battlefieldWidth / 2, -Game.game.battlefieldHeight / 2);
        Game.ChangeWalls(rect, Game.WallType.Unbuildable);

        if (pooledNumbers == null)
			pooledNumbers = new List<GameObject>();

		for (int i = pooledNumbers.Count; i < healthAmount; i++) {
			pooledNumbers.Add ((GameObject)Instantiate (numbers[Random.Range (0,numbers.Length)]));
			DatastreamNumber num = pooledNumbers[pooledNumbers.Count-1].GetComponent<DatastreamNumber>();
			num.transform.parent = transform;
			num.datastream = this;
			num.ResetNumber ();
			num.GetComponent<SpriteRenderer>().color = Color.green;

			yield return new WaitForSeconds (0.1f);
		}
	}
}
