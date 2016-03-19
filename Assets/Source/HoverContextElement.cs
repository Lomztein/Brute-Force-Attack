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

	void OnMouseEnterElement () {
		HoverContext.ChangeText (text);
	}

	void OnMouseExitElement () {
		HoverContext.ChangeText ("");
    }

	void Update () {
		RaycastHit hit;
        Vector3 pos = Input.mousePosition;  

        if (isWorldElement) {
            pos = Camera.main.ScreenToWorldPoint (pos);
        }

		Ray ray = new Ray ((Vector3)(Vector2)pos + Vector3.back * 5f, Vector3.forward * 10f);
        Debug.DrawRay (ray.origin, ray.direction + Vector3.right);
        bool raycast = collider.Raycast (ray, out hit, 100);

        // This code might cause cancer, need optimizations if possible. I mean, you might as well just use Rect.Contains ().
        if (activeElement == null) {
            if (raycast) {
                SendMessage ("OnMouseEnterElement");
                activeElement = this;
            }
		}else if (activeElement == this) {
            if (!raycast) {
                SendMessage ("OnMouseExitElement");
                activeElement = null;
            }
		}

        if (activeElement == this && Input.GetMouseButtonDown (0)) {
            SendMessage ("OnMouseDownElement", SendMessageOptions.DontRequireReceiver);
        }
	}

    public void ForceUpdate () {
        activeElement = null;
    }
}
