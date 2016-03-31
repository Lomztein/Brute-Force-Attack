using UnityEngine;
using System.Collections;

public class SlowfieldModule : Module {

	public Transform rotator;
	public float rotateSpeed;
	public float range;

	public GameObject slowfieldArea;
	private SlowfieldArea slowfield;

	void FixedUpdate () {
		rotator.Rotate (Vector3.forward * rotateSpeed * Time.deltaTime * upgradeMul);
		if (!slowfield) {
			GameObject slow = (GameObject)Instantiate (slowfieldArea, transform.position, Quaternion.identity);
			slowfield = slow.GetComponent<SlowfieldArea> ();
			slowfield.slowAmount = 0.5f;
			slowfield.time = 0f;
			slow.transform.parent = transform;
		} else {
			slowfield.range = range * upgradeMul;
		}
	}

	void OnDrawGizmos () {
		Gizmos.DrawWireSphere (transform.position, range * upgradeMul);
	}

    public void RequestRange ( GameObject rangeIndicator ) {
        rangeIndicator.SendMessage ("GetRange", range * upgradeMul);
    }
}
