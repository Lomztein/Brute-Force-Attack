using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HoverContext : MonoBehaviour {

	public Text text;
	public static HoverContext cur;
	public RectTransform rectTransform;

    public LayerMask worldLayers;
    public LayerMask GUILayers;

	void Start () {
		cur = this;
		gameObject.SetActive (false);
	}

	void OnEnabled () {
		SetPos ();
	}

    public static void StaticUpdate () {
        if (cur) {
            Vector3 pos = Input.mousePosition;
            RaycastHit hit;

            // Handle GUI raycasting first, so that world raycasting can overwrite
            Ray ray = new Ray((Vector3)(Vector2)pos + Vector3.back * 5f, Vector3.forward * 10f);
            bool raycast = Physics.Raycast(ray, out hit, Mathf.Infinity, cur.GUILayers);

            // Handle world raycasting
            if (!raycast) {
                ray = Camera.main.ScreenPointToRay(pos);
                raycast = Physics.Raycast(ray, out hit, Mathf.Infinity, cur.worldLayers);
            }
            cur.CheckHit(raycast, hit);

            cur.SetPos();
        }
    }

    void CheckHit (bool raycast, RaycastHit hit) {
        if (hit.collider) {
            HoverContextElement hoverHit = hit.collider.GetComponent<HoverContextElement>();
            if (!hoverHit)
                return;

            // This code might cause cancer, need optimizations if possible. I mean, you might as well just use Rect.Contains ().
            if (HoverContextElement.activeElement == null) {
                if (raycast) {
                    hit.collider.SendMessage("OnMouseEnterElement");
                    HoverContextElement.activeElement = hoverHit;
                    Debug.Log("Entered!");
                }
            } else {
                if (HoverContextElement.activeElement != hoverHit || !raycast) {
                    HoverContextElement.activeElement.SendMessage("OnMouseExitElement");
                    HoverContextElement.activeElement = null;
                }
            }

            if (HoverContextElement.activeElement == hoverHit && Input.GetMouseButtonDown(0)) {
                hit.collider.SendMessage("OnMouseDownElement", SendMessageOptions.DontRequireReceiver);
            }
        } else if (!PlayerInput.cur.isEditingWalls) {
            HoverContextElement.activeElement = null;
            ChangeText("");
        }
    }

    void SetPos () {
        Debug.Log("Setting position!");
		Vector3 mousePos = Input.mousePosition;
		transform.position = mousePos + new Vector3 (rectTransform.sizeDelta.x / 2f ,0);
		if (mousePos.y > Screen.height/2)
			transform.position += Vector3.down * rectTransform.sizeDelta.y / 2f;
	}

	public static void ChangeText (string t) {
		if (t != "") {
			cur.gameObject.SetActive (true);
			cur.text.text = t;
		}else{
			cur.gameObject.SetActive (false);
		}
	}
}
