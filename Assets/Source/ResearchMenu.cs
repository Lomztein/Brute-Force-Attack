using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ResearchMenu : MonoBehaviour {

	public List<Upgrade> upgrades;
	public RectTransform scrollThingie;
	public RectTransform startRect;
	public GameObject buttonPrefab;
	public GameObject prerequisiteLine;
	private Dictionary<int, int> levels = new Dictionary<int, int>();

	public static float[] damageMul;
	public static float[] rangeMul;
	public static float[] costMul;
	public static float[] firerateMul;

	private List<GameObject> buttons = new List<GameObject>();
	public static ResearchMenu cur;

	public void Initialize () {
		cur = this;
		InitializeResearchMenu ();
		InitializeMultipliers ();
	}

	void InitializeMultipliers () {
		int types = 6;

		damageMul   = new float[types];
		rangeMul    = new float[types];
		costMul     = new float[types];
		firerateMul = new float[types];

		for (int i = 0; i < 6; i++) {
			damageMul[i]   = 1f;
			rangeMul[i]    = 1f;
			costMul[i]     = 1f;
			firerateMul[i] = 1f;
		}
	}

	public Vector3 GetPos (Upgrade u) {
		int y = u.upgradeCost;
		int x = 0;
		
		if (levels.ContainsKey (y)) {
			levels[y]++;
			if (levels[y] % 2 == 0) {
				x = -levels[y]/2;
			}else{
				x = levels[y]/2;
				if (x >= 0)
					x++;
			}
		}else{
			levels.Add (y, 0);
		}

		return new Vector3 (x,y);
	}

	public void ResetDictionary () {
		levels = new Dictionary<int, int>();
	}
	
	public void InitializeResearchMenu () {

		for (int i = 0; i < upgrades.Count; i++) {

			Upgrade u = upgrades[i];

			Vector3 pos = GetPos (u) * 100f;
			GameObject newU = (GameObject)Instantiate (buttonPrefab, startRect.position + pos, Quaternion.identity);
			newU.GetComponent<HoverContextElement>().text = u.upgradeName;
			newU.transform.SetParent (startRect.parent, true);
			newU.transform.GetChild (0).GetComponent<Image>().sprite = u.upgradeSprite;
			u.button = newU;
			buttons.Add (newU);

		}

		ResetDictionary ();

		for (int i = 0; i < buttons.Count; i++) {
			Upgrade u = upgrades[i];
			if (u.prerequisiteID >= 0) {
				GameObject line = (GameObject)Instantiate (prerequisiteLine, buttons[i].transform.position, Quaternion.identity);
				LineRenderer l = line.GetComponent<LineRenderer>();

				l.SetPosition (0, Camera.main.ScreenToWorldPoint (buttons[i].transform.position) + Vector3.forward);
				l.SetPosition (1, Camera.main.ScreenToWorldPoint (upgrades[u.prerequisiteID].button.transform.position) + Vector3.forward);
				line.transform.parent = Camera.main.transform;
			}
		}
	}
}
