using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ResearchMenu : MonoBehaviour {

	public List<Research> research = new List<Research>();
	public RectTransform scrollThingie;
	public RectTransform startRect;
	public GameObject buttonPrefab;
	public GameObject prerequisiteLine;

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

	public Vector3 GetPos (Research u) {
		return new Vector3 (u.x,u.y);
	}

	Rect GetScrollRect () {
		Rect ans = new Rect ();
		Vector2 avg = new Vector2 ();
		for (int i = 0; i < research.Count; i++) {

			if (research[i].x > ans.width)
				ans.width = research[i].x;

			if (research[i].y > ans.height)
				ans.height = research[i].y;

			avg += new Vector2 (research[i].x, research[i].y);

		}

		avg /= research.Count;
		ans.x = avg.x;
		ans.y = avg.y;

		return ans;
	}

	public void InitializeResearchMenu () {

		Rect newRect = GetScrollRect ();
		scrollThingie.sizeDelta = new Vector2 (newRect.width, newRect.height) * 100;
		startRect.position = transform.position - new Vector3 (newRect.x, newRect.y, 0) * 100 + Vector3.down * 150f;

		for (int i = 0; i < research.Count; i++) {

			Research u = research[i];

			Vector3 pos = GetPos (u) * 100f;
			GameObject newU = (GameObject)Instantiate (buttonPrefab, startRect.position + pos, Quaternion.identity);
			newU.GetComponent<HoverContextElement>().text = u.name;
			newU.transform.SetParent (startRect.parent, true);
			newU.transform.GetChild (0).GetComponent<Image>().sprite = u.sprite;
			u.button = newU;
			buttons.Add (newU);

		}

		/*for (int i = 0; i < buttons.Count; i++) {
			Research u = research[i];
			if (u.prerequisiteID >= 0) {
				GameObject line = (GameObject)Instantiate (prerequisiteLine, buttons[i].transform.position, Quaternion.identity);
				LineRenderer l = line.GetComponent<LineRenderer>();

				l.SetPosition (0, Camera.main.ScreenToWorldPoint (buttons[i].transform.position) + Vector3.forward);
				l.SetPosition (1, Camera.main.ScreenToWorldPoint (research[u.prerequisiteID].button.transform.position) + Vector3.forward);
				line.transform.parent = Camera.main.transform;
			}
		}*/
	}
}
