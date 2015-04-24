using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ResearchMenu : MonoBehaviour {

	public Transform lineParent;
	public Transform buttonParent;

	public List<Research> research = new List<Research>();
	public RectTransform scrollThingie;
	public RectTransform startRect;
	public GameObject buttonPrefab;
	public GameObject prerequisiteLine;
	private bool isOpen = true;
	private Vector3 startPos;

	// Unlock research stuff
	public static float[] damageMul;
	public static float   rangeMul;
	public static float[] costMul;
	public static float[] firerateMul;
	public static float   turnrateMul;

	public GameObject[] unlockableModules;
	public Image researchIndicator;

	private List<GameObject> buttons = new List<GameObject>();
	public static ResearchMenu cur;

	public void Initialize () {
		cur = this;
		InitializeResearchMenu ();
		InitializeMultipliers ();
		startPos = transform.position;
		ToggleResearchMenu ();
		UpdateButtons ();
	}

	public void ToggleResearchMenu () {
		if (isOpen) {
			isOpen = false;
			transform.position += Vector3.right * 100000;
		}else{
			isOpen = true;
			transform.position = startPos;
		}
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
		IncreaseAllCost ();
		b.GetComponent<HoverContextElement>().text = research[index].name + ", Researched";

		Destroy (research[index]);
		research[index] = null;
		UpdateButtons ();
	}

	void IncreaseAllCost () {
		for (int i = 0; i < research.Count; i++) {
			if (research[i] != null) {
				research[i].y++;
				research[i].button.GetComponent<HoverContextElement>().text = research[i].name + ", " + research[i].y + " Research";
			}
		}
	}

	public void UpdateImageColor (Research research, Image image) {

		switch (research.colour) {
			
		case Colour.None:
			image.color = Color.white;
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
		bool isButtonAvailable = false;
		for (int i = 0; i < buttons.Count; i++) {
			if (research[i] != null) {
				bool temp = CheckButtonAvailable (research[i]);
				if (temp)
					isButtonAvailable = temp;
			}
		}

		if (isButtonAvailable) {
			researchIndicator.color = Color.green;
		}else{
			researchIndicator.color = Color.red;
		}

	}

	bool CheckButtonAvailable (Research research) {
		Image image = research.button.transform.GetChild (0).GetComponent<Image>();
		if (research.y > Game.research) {
			image.color = Color.black;
			return false;
		}

		if (research.prerequisite != null) {
			if (!research.prerequisite.isBought) {
				image.color = Color.black;
				return false;
			}
		}

		if (research.y <= Game.research) {
			UpdateImageColor (research, image);
			return true;
		}

		return true;
	}
	                                          

	public void InitializeResearchMenu () {

		Rect newRect = GetScrollRect ();
		scrollThingie.sizeDelta = new Vector2 (newRect.width, newRect.height) * 100;
		startRect.position = transform.position - new Vector3 (newRect.x, newRect.y, 0) * 100 + Vector3.down * 150f;

		for (int i = 0; i < research.Count; i++) {

			Research u = research[i];

			Vector3 pos = GetPos (u) * 100f;
			GameObject newU = (GameObject)Instantiate (buttonPrefab, startRect.position + pos, Quaternion.identity);
			newU.GetComponent<HoverContextElement>().text = u.name + ", " + u.y.ToString () + " Research";
			newU.transform.SetParent (buttonParent, true);
			Image image = newU.transform.GetChild (0).GetComponent<Image>();
			u.button = newU;
			image.sprite = u.sprite;
			buttons.Add (newU);
			u.index = i;

			AddPurchaseButtonListener (newU.GetComponent<Button>(), i);
			if (u.name == "")
				newU.SetActive (false);
		}

		for (int i = 0; i < research.Count; i++) {
			Research r = research[i];
			if (r.prerequisite) {

				Vector3 pPos = r.button.transform.position + (r.prerequisite.button.transform.position - r.button.transform.position) / 2;
				Quaternion pRot = Quaternion.Euler (0,0, Angle.CalculateAngle (r.button.transform, r.prerequisite.button.transform));
				GameObject line = (GameObject)Instantiate (prerequisiteLine, pPos, pRot);
				RectTransform lr = line.GetComponent<RectTransform>();
				lr.sizeDelta = new Vector2 (Vector3.Distance (r.button.transform.position, r.prerequisite.button.transform.position), 10);
				line.transform.SetParent (lineParent, true);

			}
		}
	}

	void AddPurchaseButtonListener (Button button, int index) {
		button.onClick.AddListener (() => {
			research[index].Purchase ();
		});
	}

	// Put research code here
	public void UnlockModule (Research research) {
		Game.game.purchaseMenu.standard.Add (unlockableModules[research.value]);
		Game.game.purchaseMenu.InitializePurchaseMenu (Game.game.purchaseMenu.standard.ToArray ());
	}

	public void UnlockSpecialModule (Research research) {
		Game.game.purchaseMenu.special.Add (unlockableModules[research.value]);
		Game.game.purchaseMenu.InitializePurchaseMenu (Game.game.purchaseMenu.special.ToArray ());
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

	public void EnableAdvancedTracking (Research research) {
		BaseModule.enableAdvancedTracking = true;
	}
}
