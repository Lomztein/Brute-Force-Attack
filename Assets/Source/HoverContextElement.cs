using UnityEngine;
using System.Collections;

public class HoverContextElement : MonoBehaviour {

	public string text;
	private Collider collider;
	private bool prevHit;

	void Start () {
		collider = GetComponent<Collider>();
	}

	void OnMouseEnter () {
		HoverContext.ChangeText (text);
	}

	void OnMouseExit () {
		HoverContext.ChangeText ("");
	}

	void FixedUpdate () {
		RaycastHit hit;
		Ray ray = new Ray (Input.mousePosition + Vector3.back * 5f, Vector3.forward * 10f);
		Debug.DrawRay (ray.origin, ray.direction);
		if (collider.Raycast (ray, out hit, 100)) {
			OnMouseEnter ();
			prevHit = true;
		}else if (prevHit) {
			OnMouseExit ();
			prevHit = false;
		}
	}
}
