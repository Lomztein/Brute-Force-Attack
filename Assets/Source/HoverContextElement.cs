using UnityEngine;
using System.Collections;

public class HoverContextElement : MonoBehaviour {

    public static HoverContextElement activeElement;
	public string text;
	new private Collider collider;
	private bool prevHit;
    public bool isWorldElement;

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
        Vector3 pos = Input.mousePosition;  

        if (isWorldElement) {
            pos = Camera.main.ScreenToWorldPoint (pos);
        }

		Ray ray = new Ray ((Vector3)(Vector2)pos + Vector3.back * 5f, Vector3.forward * 10f);

        if (collider.Raycast (ray, out hit, 100)) {
            if (!activeElement) {
                SendMessage ("OnMouseEnter");
                activeElement = this;
            }
		}else if (activeElement = this) {

            SendMessage ("OnMouseExit");
			OnMouseExit ();
            activeElement = null;
		}

        if (activeElement == this && Input.GetMouseButtonDown (0)) {
            SendMessage ("OnMouseClick", SendMessageOptions.DontRequireReceiver);
        }
	}

    public void ForceUpdate () {
        activeElement = null;
    }
}
