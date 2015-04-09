using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Datastream : MonoBehaviour {

	public Vector3 start;
	public float flyDistance;
	public float flySpeed;

	public GameObject[] numbers;

	public static int corruptionLevel; // From 0 to 100.

	public List<GameObject> pooledNumbers;

	// TODO Implement pooling of ones and zeros.

	void Start () {
		StartCoroutine ("InitializeNumbers");
	}

	IEnumerator InitializeNumbers () {

		pooledNumbers = new List<GameObject>();

		for (int i = 0; i < 100; i++) {
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
