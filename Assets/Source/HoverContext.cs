using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HoverContext : MonoBehaviour {

	public Text text;
	public static HoverContext cur;
	public RectTransform rectTransform;

	void Start () {
		cur = this;
		gameObject.SetActive (false);
	}

	void OnEnabled () {
		SetPos ();
	}

	// Update is called once per frame
	void Update () {
		SetPos ();
	}

	void SetPos () {
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
