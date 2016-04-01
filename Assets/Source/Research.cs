using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class Research {

    public string name;
	public string desc;
	public string func;
    public Sprite sprite;
	public string meta;

	public Colour colour;
	
	public int x;
	public int y;

	public int prerequisite;

	public int index;
    [System.NonSerialized]
	public bool isBought;
    [System.NonSerialized]
    public GameObject button;
    [System.NonSerialized]
    public Image highlighter;

    public Research GetPrerequisite () {
        if (Game.game)
            return Game.game.researchMenu.research[prerequisite];
        return GameObject.Find ("ResearchMenu").GetComponent<ResearchMenu> ().research[prerequisite];
    }

	public void Purchase (bool force = false) {
        if (Game.research >= y) {
            if (prerequisite == -1 || GetPrerequisite ().isBought) {
                DoPurchase (force);
            }
        }
        if (force) {
		    DoPurchase (force);
		}
	}

	void DoPurchase (bool force = false) {
		isBought = true;
		ResearchMenu.cur.SendMessage (func, this, SendMessageOptions.RequireReceiver);
		if (!force)
            Game.research -= y;
		ResearchMenu.cur.InvalidateButton (button, index);
	}
}
