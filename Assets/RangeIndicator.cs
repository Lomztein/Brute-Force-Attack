using UnityEngine;
using System.Collections;

public class RangeIndicator : MonoBehaviour {

	// TL;DR of this code: DO NOT READ, YOU WILL DIE FROM HORRIBLECODENESS.
	// It's pretty much hacked together over a month of vecation, whenever I was away from Mjølnir.

	public GameObject parent;
	public float range;
	public bool destroyOnParentLost = true;
	public bool autoUpdate = true;

	public static GameObject CreateRangeIndicator (GameObject go, Vector3 pos, bool destroyOnParentLost, bool autoUpdate) {
		GameObject newRange = (GameObject)Instantiate (Game.game.rangeIndicator, pos, Quaternion.identity);
		newRange.GetComponent<RangeIndicator> ().destroyOnParentLost = destroyOnParentLost;
		newRange.GetComponent<RangeIndicator> ().autoUpdate = autoUpdate;
		newRange.GetComponent<RangeIndicator> ().ForceParent (go, pos);
		return newRange;
	}

	public void ForceParent (GameObject newParent, Vector3 pos) {
		parent = newParent;
		FixedUpdate ();
	}

	public void NullifyParent () {
		parent = null;
		range = 0f;
		transform.localScale = new Vector3 (range, range, 1);
	}

	public void FixedUpdate () {
		if (autoUpdate) {
			if (parent) {
				ForceRequestRange (parent, gameObject);
				transform.position = parent.transform.position;
			} else if (destroyOnParentLost) {
				Destroy ();
			} else {
				NullifyParent ();
			}
		}
	}

	public static void ForceRequestRange (GameObject _gameObject, GameObject toGetRange) {
		_gameObject.SendMessage ("RequestRange", toGetRange, SendMessageOptions.RequireReceiver);
	}

	public void GetRange (float _range) {
		range = _range;
		transform.localScale = new Vector3 (range, range, 1);
	}

	public void Destroy () {
		Destroy (gameObject);
	}
}
