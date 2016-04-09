using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class ResearchMenu : MonoBehaviour {

	public Transform lineParent;
	public Transform buttonParent;

	public List<Research> research = new List<Research>();
	public RectTransform scrollThingie;
	public RectTransform startRect;
    public ScrollRect scrollRect;
	public GameObject buttonPrefab;
	public GameObject prerequisiteLine;
	public static bool isOpen = true;
	private Vector3 startPos;

    public float buttonSize;
    public float buttonMargin;

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
        isOpen = true;
		InitializeResearchMenu ();
		startPos = transform.localPosition;
		ToggleResearchMenu ();
		UpdateButtons ();
	}

	public static void InitializeAllStatics () {
		InitializeStatics ();
		InitializeMultipliers ();
	}

	public static void InitializeStatics () {
		Datastream.healSpeed = 0f;
		Datastream.repurposeEnemies = false;
		Datastream.enableFirewall = false;
		FireProjectile.fireWidth = 0.2f;
		BaseModule.enableAdvancedTracking = false;
		HomingProjectile.autoFindTarget = false;
		FocusBeamWeapon.chargeSpeedMultiplier = 1f;
		SplitterHomingProjectile.amount = 0;
		SlowfieldArea.staticMultiplier = 1f;
		TeslaProjectile.chainAmount = 0;

        GameObject[] weaponModules = Resources.LoadAll<GameObject>("Modules/Weapons/");
        for (int i = 0; i < weaponModules.Length; i++) {
            Weapon wep = weaponModules[i].GetComponentInChildren<Weapon>();
            if (!Weapon.bulletIndex.ContainsKey (wep.weaponIdentifier)) {
                Weapon.bulletIndex.Add(wep.weaponIdentifier, 0);
            } else {
                Weapon.bulletIndex[wep.weaponIdentifier] = 0;
            }
        }
	}

	public void ToggleResearchMenu () {
		if (isOpen) {
			isOpen = false;
			transform.localPosition += Vector3.right * 100000;
            Game.ForceDarkOverlay(false);
		}else{
			isOpen = true;
			transform.localPosition = startPos;
            Game.ForceDarkOverlay(true);
        }

        UpdateButtonActiveness ();
	}

    void UpdateButtonActiveness () {
        for (int i = 0; i < buttons.Count; i++) {
            buttons[i].SetActive (isOpen);
        }
    }

	public static void InitializeMultipliers () {
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
			RocketLauncherWeapon.doubleRocketsEnabled[i] = false;
		}
	}

	public Vector3 GetPos (Research u) {
		return new Vector3 (u.x,u.y);
	}

	Rect GetScrollRect () {
        Rect minMax = new Rect ();
		for (int i = 0; i < research.Count; i++) {

			if (research[i].x * buttonSize > minMax.x && research[i].x > 0) {
                minMax.x = research[i].x * buttonSize;
            }

			if (research[i].y * buttonSize > minMax.y && research[i].y > 0) {
                minMax.y = research[i].y * buttonSize;
            }

            if (research[i].x * buttonSize < minMax.width && research[i].x < 0) {
                minMax.width = research[i].x * buttonSize;
            }

            if (research[i].y * buttonSize < minMax.height && research[i].y < 0) {
                minMax.height = research[i].x * buttonSize;
            }

		}

		return new Rect (minMax.x - buttonMargin, minMax.y - buttonMargin, minMax.x - minMax.width + buttonMargin, minMax.y - minMax.height + buttonMargin);
	}

    Vector4 GetButtonMinMaxPos () {
        Vector4 minMax = new Vector4 ();
        for (int i = 0; i < research.Count; i++) {

            Transform button = research[i].button.transform;

            if (button.localPosition.x > minMax.x && button.localPosition.x > 0) {
                minMax.x = button.localPosition.x;
            }

            if (button.localPosition.y > minMax.y && button.localPosition.y > 0) {
                minMax.y = button.localPosition.y;
            }

            if (button.localPosition.x < minMax.z && button.localPosition.x < 0) {
                minMax.z = button.localPosition.x;
            }

            if (button.localPosition.y < minMax.w && button.localPosition.y < 0) {
                minMax.w = button.localPosition.y;
            }
        }

        return minMax;
    }

    public void InvalidateButton (GameObject b, int index) {
		Button button = b.GetComponent<Button>();
		button.interactable = false;
		// IncreaseAllCost ();
		b.GetComponent<HoverContextElement> ().text = research [index].name + ", Researched";

		foreach (LoadAssemblyButton butt in PurchaseMenu.cur.assemblyButtonList) {
            butt.OnResearchUnlocked ();
		}

        foreach (Module mod in Game.currentModules) {
            mod.UpdateHoverContextElement ();
        }

        //Destroy (research[index]);
		UpdateButtons ();
		research[index].name = "RESEARCHED";
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
			if (research[i].name != "RESEARCHED") {
				bool temp = CheckButtonAvailable (research[i]);
                research[i].button.GetComponent<Button> ().interactable = temp;
				if (temp)
					isButtonAvailable = temp;
            } else {
                research[i].button.GetComponent<Button> ().interactable = false;
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

        if (research.isBought) {
			UpdateImageColor (research, image);
            image.color /= 2f;
            return false;
        }

        if (research.y > Game.research) {
			image.color = Color.black;
			return false;
		}

		if (research.prerequisite != -1) {
			if (!research.GetPrerequisite ().isBought) {
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
		scrollThingie.sizeDelta = new Vector2 (newRect.width, newRect.height);

        for (int i = 0; i < research.Count; i++) {

			Research u = research[i];

			Vector3 pos = GetPos (u) * buttonSize;
			GameObject newU = Instantiate (buttonPrefab);

            newU.transform.localPosition = buttonParent.position + pos;
            newU.transform.SetParent (buttonParent, true);
			Image image = newU.transform.GetChild (0).GetComponent<Image>();

			u.button = newU;
			image.sprite = u.sprite;
			buttons.Add (newU);
			u.index = i;
			u.y++;

            u.highlighter = newU.transform.FindChild ("Highlighter").GetComponent<Image> ();
			newU.GetComponent<HoverContextElement>().text = u.name + ", " + u.y.ToString () + " Research";
			AddPurchaseButtonListener (newU.GetComponent<Button>(), i);
			if (u.name == "")
				newU.SetActive (false);

            newU.transform.localScale = Vector3.one;
		}

		for (int i = 0; i < research.Count; i++) {
			Research r = research[i];
            if (r.prerequisite != -1 && r.name != "") {

                Vector3 pPos = r.button.transform.localPosition + (r.GetPrerequisite ().button.transform.localPosition - r.button.transform.localPosition) / 2;
                Quaternion pRot = Quaternion.Euler (0, 0, Angle.CalculateAngle (r.button.transform.localPosition, r.GetPrerequisite ().button.transform.localPosition));
                GameObject line = (GameObject)Instantiate (prerequisiteLine, pPos, pRot);
                RectTransform lr = line.GetComponent<RectTransform> ();
                lr.sizeDelta = new Vector2 (Vector3.Distance (r.button.transform.localPosition, r.GetPrerequisite ().button.transform.localPosition), 10);
                line.transform.SetParent (lineParent, true);

                lr.transform.localScale = Vector3.one;
            }
		}

        Vector4 minMax = GetButtonMinMaxPos ();
        buttonParent.localPosition = new Vector3 (-(minMax.z + minMax.x) / 2f, (minMax.w - minMax.y) / 2f);
        lineParent.localPosition = buttonParent.localPosition;
        scrollRect.verticalNormalizedPosition = 0f;

        UpdateButtonActiveness ();
    }

    [System.Serializable]
    public class SimpleResearch {
        public string name;
        public string desc;
        public string func;
        public string meta;

        public Colour colour;

        public int x;
        public int y;

        public int prerequisite;

        public SimpleResearch (Research res) {
            name = res.name;
            desc = res.desc;
            func = res.func;
            meta = res.meta;
            colour = res.colour;
            x = res.x;
            y = res.y;
            prerequisite = res.prerequisite;
        }
    }
    public void SaveBackup () {
        for (int i = 0; i < research.Count; i++) {
            BinaryFormatter bf = new BinaryFormatter ();
            FileStream file = File.Create (Game.RESEARCH_BACKUP_DATA_DIRECTORY + research[i].name + ".dat");

            bf.Serialize (file, new SimpleResearch (research[i]));
            file.Close ();
        }
    }

    /*public void LoadResearchBackup () {

        string fullFile = Game.RESEARCH_BACKUP_DATA_DIRECTORY + "BACKUP.dat";

        if (File.Exists (fullFile)) {

            BinaryFormatter bf = new BinaryFormatter ();
            FileStream file = File.Open (fullFile, FileMode.Open);

            Research[] data = (Research[])bf.Deserialize (file);
            file.Close ();
        }
    }*/

    public void ResearchAll () {
        for (int i = 0; i < research.Count; i++) {
            research[i].Purchase ();
        }
    }

    void AddPurchaseButtonListener (Button button, int index) {
		button.onClick.AddListener (() => {
			research[index].Purchase ();
		});
	}

	// Put research code here
	public void UnlockModule (Research research) {
		Game.game.purchaseMenu.standard.Add (unlockableModules[int.Parse (research.meta)]);
	}

	public void UnlockSpecialModule (Research research) {
        UnlockModule (research);
	}

	public void IncreaseFirerate (Research research) {
		float loc = 1f/firerateMul [(int)research.colour];
		loc *= (float)int.Parse(research.meta) / 100f + 1f;
		firerateMul[(int)research.colour] = 1f/loc;
	}

	public void IncreaseDamage (Research research) {
		damageMul[(int)research.colour] *= (float)int.Parse(research.meta)/100 + 1f;
	}

	public void DecreaseCost (Research research) {
		damageMul[(int)research.colour] *= 1f - (float)int.Parse(research.meta)/100;

		float loc = 1f/costMul[(int)research.colour];
		loc *= (float)int.Parse(research.meta) / 100f + 1f;
		costMul[(int)research.colour] = 1f/loc;
	}

	public void IncreaseTurnrate (Research research) {
		turnrateMul *= (float)int.Parse(research.meta)/100 + 1f;
	}

	public void IncreaseRange (Research research) {
		rangeMul *= (float)int.Parse(research.meta)/100 + 1f;
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

	public void DoubleRocketLauncherRockets (Research research) {
		RocketLauncherWeapon.doubleRocketsEnabled[(int)research.colour] = true;
	}

	public void EnableAutoFindTarget (Research research) {
		HomingProjectile.autoFindTarget = true;
	}

	public void IncreaseBeamCannonChargeSpeed (Research research) {
		FocusBeamWeapon.chargeSpeedMultiplier *= (float)int.Parse(research.meta)/100 + 1f;
	}

	public void EnableSmallRocketLauncherSplit (Research research) {
		SplitterHomingProjectile.amount = 6;
	}

	public void DecreaseSlowfieldSpeedMultiplier (Research research) {
		SlowfieldArea.staticMultiplier = 0.5f;
	}

	public void EnableChainLightning (Research research) {
		TeslaProjectile.chainAmount = 6;
	}

	public void AddAbility (Research research) {
		AbilityBar.AddAbility (int.Parse(research.meta));
	}

    public void UpgradeWeapon (Research research) {
        Weapon.TechUpWeapon(research.meta);
    }
}
	