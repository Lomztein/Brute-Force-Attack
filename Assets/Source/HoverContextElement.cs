using UnityEngine;
using System.Collections;

public class HoverContextElement : MonoBehaviour {

    public static HoverContextElement activeElement;
	public string text;
	private bool prevHit;
    public bool isWorldElement;

	void OnMouseEnterElement () {
		HoverContext.ChangeText (text);
	}

	void OnMouseExitElement () {
		HoverContext.ChangeText ("");
    }

    public void ForceUpdate () {
        activeElement = null;
    }
}
