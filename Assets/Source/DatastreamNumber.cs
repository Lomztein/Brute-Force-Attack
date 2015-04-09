using UnityEngine;
using System.Collections;

public class DatastreamNumber : MonoBehaviour {

	public float speed;
	public Datastream datastream;

	void FixedUpdate () {
		transform.Translate (Vector3.right * speed * Time.fixedDeltaTime);
	}

	public void ResetNumber () {
		GameObject n = gameObject;
		n.transform.position = datastream.start + Vector3.up * Random.Range (-2f,2f);
		float speedMul = Random.Range (0.9f, 2.2f);
		float sizeMul = Random.Range (0.8f, 1.4f);
		n.transform.localScale = new Vector3 (sizeMul / transform.parent.parent.localScale.x, sizeMul / transform.parent.parent.localScale.y, 1f);
		n.GetComponent<DatastreamNumber>().speed = datastream.flySpeed * speedMul;

		Invoke ("ResetNumber", datastream.flyDistance / (datastream.flySpeed * speedMul));
	}
}
