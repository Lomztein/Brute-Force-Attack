using UnityEngine;
using System.Collections;

public class TODOList : MonoBehaviour {

	public string[] entries;

	void OnGUI () {
		for (int i = 0; i < entries.Length; i++) {
			GUI.Label (new Rect (160, 100 + i * 20, 200, 20), entries[i]);
		}
	}
}
