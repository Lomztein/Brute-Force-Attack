using UnityEngine;
using System.Collections;

public class Research : ScriptableObject {

	public string desc;
	public string func;
	public Sprite sprite;
	public int value;

	public Colour colour;
	
	public int x;
	public int y;

	public Research prerequisite;

	public int index;
	public bool isBought;
	public GameObject button;

	public void Purchase () {
		if (Game.research >= y) {
			if (prerequisite) {
				if (prerequisite.isBought) {
					DoPurchase ();
				}
			}else{
				DoPurchase ();
			}
		}
	}

	void DoPurchase () {
		isBought = true;
		ResearchMenu.cur.SendMessage (func, this, SendMessageOptions.RequireReceiver);
		Game.research -= y;
		ResearchMenu.cur.InvalidateButton (button, index);
	}
}
