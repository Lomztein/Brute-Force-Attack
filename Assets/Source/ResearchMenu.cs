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

	// Unlock research stuff
	public static float[] damageMul;
	public static float   rangeMul;
	public static float[] costMul;
	public static float[] firerateMul;
	public static float   turnrateMul;

	public GameObject[] unlockableModules;

	private List<GameObject> buttons = new List<GameObject>();
	public static ResearchMenu cur;

	public void Initialize () {
		cur = this;
		InitializeResearchMenu ();
		InitializeMultipliers ();
		ToggleResearchMenu ();
	}

	void FixedUpdate () {
		UpdateButtons ();
	}

	public void ToggleResearchMenu () {
		gameObject.SetActive (!cur.gameObject.activeInHierarchy);
	}

	void InitializeMultipliers () {
		int types = 7;

		damageMul   = new float[types];
		firerateMul = new float[types];
		costMul     = new float[types];

		rangeMul = 1f;
		turnrateMul = 1f;

		for (int i = 0; i < types; i++) {
			damageMul[i]   = 1f;
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

	public void InvalidateButton (GameObject b, int index) {
		Button button = b.GetComponent<Button>();
		button.interactable = false;
		b.transform.GetChild (0).GetComponent<Image>().color /= 2f;

		Destroy (research[index]);
	}

	public void UpdateImageColor (Research research, Image image) {

		switch (research.colour) {
			
		case Colour.None:
			break;
			
		case Colour.Blue:
			image.color = Color.blue;
			break;
			
		case Colour.Green:
			image.color = Color.green;
			break;
			
		case Colour.Orange:
			image.color = (Color.red + Color.yellow) / 2;
			break;
			
		case Colour.Purple:
			image.color = new Color (0.5f, 0, 0.5f);
			break;
			
		case Colour.Red:
			image.color = Color.red;
			break;
			
		case Colour.Yellow:
			image.color = Color.yellow;
			break;
			
		default:
			Debug.LogWarning ("Colour not found, for whatever reason");
			break;
		}

	}

	public void UpdateButtons () {
		for (int i = 0; i < buttons.Count; i++) {
			if (research[i] != null) {
				if (research[i].y > Game.research) {
					buttons[i].transform.GetChild (0).GetComponent<Image>().color = Color.black;
				}else{
					UpdateImageColor (research[i], buttons[i].transform.GetChild (0).GetComponent<Image>());
				}
			}
		}
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
			Image image = newU.transform.GetChild (0).GetComponent<Image>();
			u.button = newU;
			image.sprite = u.sprite;
			buttons.Add (newU);
			u.index = i;

			AddPurchaseButtonListener (newU.GetComponent<Button>(), i);
		}
	}

	void AddPurchaseButtonListener (Button button, int index) {
		button.onClick.AddListener (() => {
			research[index].Purchase ();
		});
	}

	// Put research code here
	public void UnlockModule (Research research) {
		Game.game.purchaseMenu.purchaseables.Add (unlockableModules[research.value]);
		Game.game.purchaseMenu.InitializePurchaseMenu ();
	}

	public void IncreaseFirerate (Research research) {
		firerateMul[(int)research.colour] *= (float)research.value/100 + 1f;
	}

	public void IncreaseDamage (Research research) {
		damageMul[(int)research.colour] *= (float)research.value/100 + 1f;
	}

	public void DecreaseCost (Research research) {
		damageMul[(int)research.colour] *= 1f - (float)research.value/100;
	}

	public void IncreaseTurnrate (Research research) {
		turnrateMul *= (float)research.value/100 + 1f;
	}

	public void IncreaseRange (Research research) {
		rangeMul *= (float)research.value/100 + 1f;
	}

	public void UnlockAutoBaseHeal (Research research) {
		Datastream.healSpeed = 0.1f;
	}

	public void EnableBroadbandConnection (Research research) {
		Datastream.healthAmount = 200;
		Game.game.datastream.StartCoroutine ("InitializeNumbers");
	}

	public void EnableRepurposing (Research research) {
		Datastream.repurposeEnemies = true;
	}

	public void EnableFirewall (Research research) {
		Datastream.enableFirewall = true;
	}

	public void IncreasePixelThrowerFireSize (Research research) {
		FireProjectile.fireWidth = 0.5f;
	}
	                                              
}
