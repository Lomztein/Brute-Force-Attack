using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class AssemblyContextMenu : MonoBehaviour {

	public RawImage moduleImage;
	public Text moduleName;
	public Text moduleDesc;
	public Text moduleStats;
    public Text upgradeStats;
	public GameObject treeButtonPrefab;
	public Transform treeButtonParent;
	public RectTransform treeScrollContext;
	public Text assemblyName;
    public HoverContextElement sellContextElement;

	private Module rootModule;
	public List<Module> modules = new List<Module> ();
	private GameObject[] moduleTreeButtons;
	private RangeIndicator rangeIndicator;
	private float indicatorRange;

	public Button[] upgradeButton;

	// Update is called once per frame
	void Update () {
		if (modules.Count == 0) {
			ExitMenu ();
		}

		if (Input.GetButtonDown ("Cancel"))
			ExitMenu ();

		if (Game.currentScene == Scene.AssemblyBuilder) {
			rootModule.assemblyDesc = moduleDesc.text;
			rootModule.assemblyName = moduleName.text;
		}

        if (Input.GetMouseButtonDown (1))
            if (ModuleMod.currentMenu[0])
                Destroy (ModuleMod.currentMenu[0]);
	}

	public void SellAssembly () {
		foreach (Module module in modules) {
			module.SellModule ();
		}
		ExitMenu ();
	}

	void OpenRangeIndicator () {
		if (!rangeIndicator)
			rangeIndicator = RangeIndicator.CreateRangeIndicator (null, Vector3.zero, false, false).GetComponent<RangeIndicator>();
		rangeIndicator.transform.GetChild (0).GetComponent<Renderer>().material.color = Color.green;
	}

    void UpdateStats () {
        moduleStats.text = "Root Range: " + ((int)rootModule.parentBase.GetRange ()).ToString () +
            "\n\nDamage per Second: " + ((int)rootModule.GetAssemblyDPS ()).ToString () +
            "\n\nAvarage Turnspeed: " + ((int)rootModule.GetAssemblyAVGTurnSpeed ()).ToString();
        // rangeIndicator.GetRange (rootModule.parentBase.GetRange ());

        upgradeStats.text = "Level: " + rootModule.GetAssemblyUpgradeLevel(Module.Type.Base).ToString() +
            "\n\n Level: " + rootModule.GetAssemblyUpgradeLevel(Module.Type.Weapon).ToString() +
            "\n\n Level: " + rootModule.GetAssemblyUpgradeLevel(Module.Type.Rotator).ToString();
    }

	public void UpgradeAssembly (int t) {
		//Module.Type type = (Module.Type)t;
		if (Game.currentScene == Scene.Play && Game.credits >= rootModule.GetUpgradeCost (t)) {
            bool canUpgrade = rootModule.UpgradeAssembly (t);
		
			upgradeButton[t].interactable = canUpgrade;
            UpdateUpgradeCostText (t);
		}

        UpdateStats ();
	}

    void AddTreeButtonListener (Button button, int index) {
        button.onClick.AddListener (() => {
            OpenModuleMods (index);
        });
    }

    void UpdateUpgradeCostText (int t) {
		Module.Type type = (Module.Type)t;
        if (!rootModule.IsAssemblyUpgradeable (t)) {
            ChangeUpgradeCostText (t, "Upgrade " + type.ToString () + "s, Maxed Out");
        } else {
            ChangeUpgradeCostTextPrebuild (t, rootModule.GetUpgradeCost (t).ToString ());
        }
    }

	void ChangeUpgradeCostTextPrebuild (int buttonIndex, string newText) {
		ChangeUpgradeCostText (buttonIndex, "Upgrade " + ((Module.Type)buttonIndex).ToString () + "s: " + newText + " credits");
	}

    void ChangeUpgradeCostText (int buttonIndex, string newText) {
        HoverContextElement el = upgradeButton[buttonIndex].GetComponent<HoverContextElement> ();
        el.text = newText;
        el.ForceUpdate ();
    }

    public void SaveModuleAssembly () {
		rootModule.SaveModuleAssembly (rootModule.rootModule.assemblyName);
	}

	public void ExitMenu () {
		// OpenRangeIndicator ();
		modules.Clear ();
		gameObject.SetActive (false);
		// rangeIndicator.NullifyParent ();
        if (ModuleMod.currentMenu[0])
            Destroy (ModuleMod.currentMenu[0]);
    }

	public void GetRange (float range) {
		indicatorRange = range;
	}

	public void OpenAssembly (Module _rootModule) {
        gameObject.SetActive(true);
		rootModule = _rootModule;
		modules = rootModule.GetModuleTree ().ToList ();
		UpdateModuleTree ();

		for (int i = 0; i < upgradeButton.Length; i++) {
			Module.Type type = (Module.Type)i;
			ChangeUpgradeCostText (i, rootModule.GetUpgradeCost (i).ToString ());
			
			foreach (Module module in modules) {
				if (module.moduleType == type) {
					if (module.upgradeCount >= Module.MAX_UPGRADE_AMOUNT) {
						rootModule.upgradeButtonDisabled[i] = true;
						break;
					}
				}
			}
		}

	}

	void FixedUpdate () {
		if (rootModule) UpdateUpgradeButtons ();
	}

	public void UpdateUpgradeButtons () {
        for (int i = 0; i < upgradeButton.Length; i++) {
            UpdateUpgradeCostText (i);
            if (rootModule.GetUpgradeCost (i) >= Game.credits || rootModule.upgradeButtonDisabled[i]) {
                upgradeButton[i].interactable = false;
            } else {
                upgradeButton[i].interactable = true;
            }
        }
	}

	public void UpdateModuleTree () {
		if (moduleTreeButtons != null)
			foreach (GameObject obj in moduleTreeButtons) {
				Destroy (obj);
			}

		// OpenRangeIndicator ();

		Dictionary<string, int> loc = new Dictionary<string, int>();
		// List<string> indexedNames = new List<string>();
		int count = 0;

		for (int i = 0; i < modules.Count; i++) {
			string lName = modules[i].moduleName;

			if (loc.ContainsKey (lName)) {
				loc[lName]++;
			}else{
				count++;
				loc.Add (lName, 1);
			}
		}

		moduleTreeButtons = new GameObject[count];
		for (int i = 0; i < count; i++) {
			string lName = loc.Keys.ElementAt (i);
			Module m = PurchaseMenu.cur.GetModulePrefab (lName).GetComponent<Module>();

			GameObject button = (GameObject)Instantiate (treeButtonPrefab, treeButtonParent.position + Vector3.down * 60f * i, Quaternion.identity);
			button.transform.SetParent (treeButtonParent, true);

			button.transform.FindChild ("Image").GetComponent<Image>().sprite = m.transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
			button.GetComponentInChildren<Text>().text = "x " + loc[lName];
            AddTreeButtonListener (button.GetComponent<Button> (), i);
			moduleTreeButtons[i] = button;
		}

		treeScrollContext.sizeDelta = new Vector2 (treeScrollContext.sizeDelta.x, count * 60f + 5 - treeScrollContext.rect.height);
        UpdateStats ();
        UpdateDescText ();
	}

	void UpdateRangeIndicator () {
		indicatorRange = 0f;
		RangeIndicator.ForceRequestRange (rootModule.gameObject, gameObject);
		rangeIndicator.transform.position = rootModule.transform.position;
		
		if (rootModule.moduleType == Module.Type.Weapon) {
			if (rootModule.parentBase) {
				rangeIndicator.GetRange (rootModule.parentBase.GetRange () * indicatorRange);
			}else{
				rangeIndicator.GetRange (indicatorRange * WeaponModule.indieRange);
			}
		}else{
			rangeIndicator.GetRange (indicatorRange);
		}
	}

    public void OpenModuleMods (int index) {
        ModuleMod.OpenMods (Input.mousePosition, modules[index].moduleMods, 0, modules[index]);
    }

	void UpdateDescText () {
        moduleName.text = rootModule.assemblyName;
		moduleDesc.text = rootModule.assemblyDesc;
        moduleImage.texture = rootModule.assembly.GetSprite();

        // UpdateRangeIndicator ();

        int total = 0;
        foreach (Module mod in modules)
            total += mod.GetSellAmount ();
        sellContextElement.text = "Sell Assembly - " + total.ToString () + "LoC";
    }

    /*public void OpenFocusModule (Module module) {
		this.rootModule = module;
		UpdateRangeIndicator ();

		if (module.upgradeCount >= Module.MAX_UPGRADE_AMOUNT) {
			upgradeButton.GetComponent<HoverContextElement> ().text = "Maxed Out";
			upgradeButton.interactable = false;
		} else {
			upgradeButton.GetComponent<HoverContextElement> ().text = "Upgrade Module: " + module.upgradeCost.ToString () + " LoC";
			upgradeButton.interactable = true;
		}
		
		moduleImage.sprite = module.transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
		moduleName.text = module.moduleName;
		UpdateDescText ();
		moduleStats.text = module.ToString ();
		if (module.assemblyName != "") {
			assemblyName.text = module.rootModule.assemblyName;
		}else{
			module.rootModule.assemblyName = assemblyName.text;
		}
	}*/
}
