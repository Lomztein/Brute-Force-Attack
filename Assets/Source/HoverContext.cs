using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HoverContext : MonoBehaviour {

	public Text text;
	public static HoverContext cur;

	void Start () {
		cur = this;
		gameObject.SetActive (false);
	}

	// Update is called once per frame
	void Update () {
		Vector3 mousePos = Input.mousePosition;
		transform.position = mousePos;
		if (mousePos.y > Screen.height/2)
			transform.position += Vector3.down * 65f;
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
