using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Datastream : MonoBehaviour {

	public Vector3 start;
	public float flyDistance;
	public float flySpeed;

	public GameObject[] numbers;

	public List<GameObject> pooledNumbers;
	public List<GameObject> curruptNumbers;

	public static bool repurposeEnemies;
	public static bool enableFirewall;

	public static int healthAmount = 100;
	private float healProgress;
	public static float healSpeed;

	// TODO Implement pooling of ones and zeros.

	void Start () {
		StartCoroutine ("InitializeNumbers");
	}

	void FixedUpdate () {
		healProgress *= healSpeed * Time.fixedDeltaTime;
		if (healProgress > 1f) {
			GameObject n = curruptNumbers[Random.Range (0, curruptNumbers.Count)];
			n.GetComponent<Renderer>().material.color = Color.green;
			pooledNumbers.Add (n);
		}
	}

	IEnumerator InitializeNumbers () {

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
