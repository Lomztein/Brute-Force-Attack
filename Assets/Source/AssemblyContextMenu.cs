using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class AssemblyContextMenu : MonoBehaviour {

    [Header ("Stats")]
	public RawImage moduleImage;
	public Text moduleName;
	public Text moduleDesc;
	public Text moduleStats;
    public Text upgradeStats;

	public Text assemblyName;
    public HoverContextElement sellContextElement;

    [Header ("Modules")]
	private Module rootModule;
	public List<Module> modules = new List<Module> ();
	private RangeIndicator rangeIndicator;
	private float indicatorRange;

	public Button[] upgradeButton;

    [Header ("Module Mods")]
    public float buttonSize;

	private GameObject[] moduleTreeButtons;
    public GameObject treeButtonPrefab;
    public Transform treeButtonParent;
    public RectTransform treeScrollContext;
    public RectTransform treeScrollParent;

    public GameObject subModMenu;
    public RectTransform subModScrollContext;
    public Transform subModButtonParent;
    public GameObject subModButtonPrefab;
    public GameObject[] subModButtons;


    // Update is called once per frame
    void Update () {
		if (modules.Count == 0) {
			ExitMenu ();
		}

		if (Input.GetButtonDown ("Cancel"))
			ExitMenu ();

		if (Game.currentScene == Scene.AssemblyBuilder && rootModule) {
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
        if (Game.currentScene == Scene.Play) {
            for (int i = 0; i < 3; i++) {
                if (rootModule.upgradeDescReplacement[i].Length == 0) {
                    switch (i) {
                        case 0:
                            rootModule.upgradeDescReplacement[i] = "Range: ";
                            break;

                        case 2:
                            rootModule.upgradeDescReplacement[i] = "Damage per second: ";
                            break;

                        case 1:
                            rootModule.upgradeDescReplacement[i] = "Avarage turnspeed: ";
                            break;

                    }
                }
            }
            moduleStats.text = rootModule.upgradeDescReplacement[0] + ((int)rootModule.GetRange()).ToString() +
                "\n\n" + rootModule.upgradeDescReplacement[2] + ((int)rootModule.GetAssemblyDPS()).ToString() +
                "\n\n" + rootModule.upgradeDescReplacement[1] + ((int)rootModule.GetAssemblyAVGTurnSpeed()).ToString();
            // rangeIndicator.GetRange (rootModule.parentBase.GetRange ());

            upgradeStats.text = "Level: " + rootModule.GetAssemblyUpgradeLevel(Module.Type.Base).ToString() +
                "\n\n Level: " + rootModule.GetAssemblyUpgradeLevel(Module.Type.Weapon).ToString() +
                "\n\n Level: " + rootModule.GetAssemblyUpgradeLevel(Module.Type.Rotator).ToString();
        }
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

    void AddTreeButtonListener ( Button button, string n) {
        button.onClick.AddListener (() => {
            if (ModuleMod.currentMenu[0])
                Destroy (ModuleMod.currentMenu[0]);

            OpenModuleModSubMenu (n);
        });
    }

    void AddTreeButtonListener (Button button, Module m, Transform b, bool disableSubmenu = false ) {
        button.onClick.AddListener (() => {
            OpenModuleMods (m, b);

            if (disableSubmenu)
                subModMenu.SetActive (false);
        });
    }

    void UpdateUpgradeCostText (int t) {
        if (!rootModule.IsAssemblyUpgradeable (t)) {
            ChangeUpgradeCostText (t, "Upgrade " + GetTypeName (t) + " - Maxed Out");
        } else {
            ChangeUpgradeCostTextPrebuild (t, rootModule.GetUpgradeCost (t).ToString ());
        }
    }

    string GetTypeName (int index) {
        string upgradeType = "Type name missing";
        if (rootModule.upgradeNameReplacement[index].Length > 0) {
            upgradeType = rootModule.upgradeNameReplacement[index];
        } else {
            switch ((Module.Type)index) {

                case Module.Type.Base:
                    upgradeType = "range";
                    break;

                case Module.Type.Rotator:
                    upgradeType = "rotation";
                    break;

                case Module.Type.Weapon:
                    upgradeType = "damage";
                    break;

                default:
                    upgradeType = "Replacement string required";
                    break;

            }
        }
        return upgradeType;
    }

	void ChangeUpgradeCostTextPrebuild (int buttonIndex, string newText) {

        if (rootModule.upgradeButtonDisabled[buttonIndex]) {
            ChangeUpgradeCostText (buttonIndex, "Disabled for this assembly");
            return;
        }

		ChangeUpgradeCostText (buttonIndex, "Upgrade " + GetTypeName (buttonIndex) + " - " + newText + " credits");
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
		modules.Clear ();
		gameObject.SetActive (false);
        subModMenu.SetActive (false);
        Game.ForceDarkOverlay(false);
        if (ModuleMod.currentMenu[0])
            Destroy (ModuleMod.currentMenu[0]);
    }

	public void GetRange (float range) {
		indicatorRange = range;
	}

	public void OpenAssembly (Module _rootModule) {
        Game.ForceDarkOverlay(true);
        gameObject.SetActive(true);
		rootModule = _rootModule;
		modules = rootModule.GetModuleTree ().ToList ();
		UpdateModuleTree ();

        if (Game.currentScene == Scene.Play) {
            for (int i = 0; i < upgradeButton.Length; i++) {
                Module.Type type = (Module.Type)i;
                ChangeUpgradeCostText(i, rootModule.GetUpgradeCost(i).ToString());

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
                if (rootModule.upgradeImageReplacement[i]) {
                    upgradeButton[i].transform.GetChild(0).GetComponent<Image>().sprite = rootModule.upgradeImageReplacement[i];
                }
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

			GameObject button = (GameObject)Instantiate (treeButtonPrefab, treeButtonParent.position + Vector3.down * buttonSize * i, Quaternion.identity);
			button.transform.SetParent (treeButtonParent, true);

			button.transform.FindChild ("Image").GetComponent<Image>().sprite = m.transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
			button.GetComponentInChildren<Text>().text = "x " + loc[lName];

            Button butt = button.GetComponent<Button> ();
            HoverContextElement element = button.GetComponent<HoverContextElement> ();
            if (m.moduleMods.Length == 0) {
                butt.interactable = false;
                element.text = "No mods available";
            } else if (GetModulesOfName (lName).Length == 1) {
                AddTreeButtonListener (butt, GetModulesOfName (lName)[0], button.transform, true);
                element.text = "Open mods for this module";
            } else {
                AddTreeButtonListener (button.GetComponent<Button> (), lName);
                element.text = "Open submenu for these modules";
            }

			moduleTreeButtons[i] = button;
		}

        float dist = count * buttonSize + 5;
        treeScrollContext.sizeDelta = new Vector2 (subModScrollContext.sizeDelta.x, Mathf.Max (dist, treeScrollParent.sizeDelta.y));
        if (Game.currentScene == Scene.Play) {
            UpdateStats();
            UpdateDescText();
        }
	}

    Module[] GetModulesOfName (string moduleName) {
        List<Module> found = new List<Module> ();
        for (int i = 0; i < modules.Count; i++) {
            if (modules[i].moduleName == moduleName)
                found.Add (modules[i]);
        }
        return found.ToArray ();
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

    void OpenModuleModSubMenu ( string n ) {
        subModMenu.SetActive (true);

        if (subModButtons != null)
            foreach (GameObject go in subModButtons) {
                Destroy (go);
            }

        Module[] locModules = GetModulesOfName (n);
        subModButtons = new GameObject[locModules.Length];

        for (int i = 0; i < locModules.Length; i++) {
            Vector3 pos = subModButtonParent.position + Vector3.down * buttonSize * i;
            GameObject button = (GameObject)Instantiate (subModButtonPrefab, pos, Quaternion.identity);
            button.transform.SetParent (subModButtonParent, true);

            button.transform.FindChild ("Image").GetComponent<Image> ().sprite = locModules[i].transform.FindChild ("Sprite").GetComponent<SpriteRenderer> ().sprite;
            button.GetComponent<SubModuleModMenuButton> ().module = locModules[i];
            Button butt = button.GetComponent<Button> ();

            button.GetComponent<HoverContextElement>().text = "Open mods for this module";
            AddTreeButtonListener (butt, locModules[i], button.transform);

            subModButtons[i] = button;
        }
        float dist = locModules.Length * buttonSize + 5;
        subModScrollContext.sizeDelta = new Vector2 (treeScrollParent.sizeDelta.x, Mathf.Max (dist, treeScrollParent.sizeDelta.y));
    }

    public void OpenModuleMods (Module m, Transform button) {
        ModuleMod.OpenMods (button.position + Vector3.right * buttonSize, m.moduleMods, 0, m);
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
