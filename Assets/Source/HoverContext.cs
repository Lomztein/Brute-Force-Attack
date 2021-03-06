﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverContext : MonoBehaviour {

	public Text text;
	public static HoverContext cur;
	public RectTransform rectTransform;

    public LayerMask worldLayers;
    public LayerMask GUILayers;
    public static GameObject hoveringButton;

	void Start () {
		cur = this;
		gameObject.SetActive (false);
	}

	void OnEnable () {
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
                raycast = Physics.Raycast (ray, out hit, Mathf.Infinity, cur.worldLayers);
            }

            cur.CheckHit(raycast, hit);
        }
    }

    bool InsideLayerMask (int layer, LayerMask layerMask) {
        return (Mathf.RoundToInt (Mathf.Pow (2, layer)) & layerMask) == layerMask;
    }

    public bool IsReachable (Transform r) {
        if (Game.darkOverlayActive) {
            if (InsideLayerMask (r.gameObject.layer, GUILayers)) {
                while (r.parent && r.parent.parent) {
                    r = r.parent;
                }
                if (r.GetSiblingIndex () < Game.darkOverlaySiblingIndex)
                    return false;
            } else {
                return false;
            }
        }
        return true;
    }

    void CheckHit (bool raycast, RaycastHit hit) {
        if (hit.collider && IsReachable (hit.collider.transform)) {

            hoveringButton = hit.collider.gameObject;

            HoverContextElement hoverHit = hit.collider.GetComponent<HoverContextElement>();
            // This code might cause cancer, need optimizations if possible. I mean, you might as well just use Rect.Contains ().
            if (HoverContextElement.activeElement == null) {
                if (raycast) {
                    hit.collider.SendMessage ("OnMouseEnterElement", SendMessageOptions.DontRequireReceiver);
                    HoverContextElement.activeElement = hoverHit;
                }
            } else {
                if (HoverContextElement.activeElement != hoverHit || !raycast) {
                    HoverContextElement.activeElement.SendMessage ("OnMouseExitElement", SendMessageOptions.DontRequireReceiver);
                    HoverContextElement.activeElement = null;
                }
            }

            if (HoverContextElement.activeElement == hoverHit && Input.GetMouseButtonDown (0)) {
                hit.collider.SendMessage ("OnMouseDownElement", SendMessageOptions.DontRequireReceiver);
            }

        } else if (HoverContextElement.activeElement) {
            HoverContextElement.activeElement.SendMessage ("OnMouseExitElement", SendMessageOptions.DontRequireReceiver);
            HoverContextElement.activeElement = null;
        } else if (!PlayerInput.cur.isEditingWalls && !PlayerInput.cur.isPlacing) {
            HoverContextElement.activeElement = null;
            ChangeText ("");
        }
    }

    void LateUpdate() {
        SetPos ();
    }

    void SetPos () {
        Vector3 mousePos = Input.mousePosition; 
        transform.position = mousePos + new Vector3 (rectTransform.rect.width / 2f, rectTransform.rect.height / 2);
        if (mousePos.y > Screen.height - rectTransform.rect.height) {
            transform.position += Vector3.down * rectTransform.rect.height;
        }
        if (mousePos.x > Screen.width - rectTransform.rect.width) {
            transform.position += Vector3.left * rectTransform.rect.width;
        }
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
