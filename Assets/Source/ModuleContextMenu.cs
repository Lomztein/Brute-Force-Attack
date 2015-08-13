using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ModuleContextMenu : MonoBehaviour {

	public Image moduleImage;
	public Text moduleName;
	public Text moduleDesc;
	public Text moduleStats;
	public GameObject treeButtonPrefab;
	public Transform treeButtonParent;
	public RectTransform treeScrollContext;
	public Text assemblyName;

	private Module focusModule;
	public List<Module> modules = new List<Module> ();
	private GameObject[] moduleTreeButtons;
	private RangeIndicator rangeIndicator;
	private float indicatorRange;

	public Button upgradeButton;

	// Update is called once per frame
	void Update () {
		if (modules.Count == 0) {
			ExitMenu ();
		} else if (focusModule) {
			focusModule.assemblyName = assemblyName.text;
		} else {
			OpenFocusModule (modules[0]);
			moduleTreeButtons[0].GetComponent<Button>().interactable = false;
		}

		if (Input.GetButtonDown ("Cancel"))
			ExitMenu ();
	}

	public void SellModule () {
		Module[] temp = null;
		if (Input.GetButtonDown ("LShift")) {
			temp = modules.ToArray ();
		} else {
			temp = new Module[1];
			temp [0] = focusModule;
		}
		
		foreach (Module module in temp) {
			module.SellModule ();
			RemoveModule (module);
		}
	}

	void OpenRangeIndicator () {
		if (!rangeIndicator)
			rangeIndicator = RangeIndicator.CreateRangeIndicator (null, Vector3.zero, false, false).GetComponent<RangeIndicator>();
		rangeIndicator.transform.GetChild (0).GetComponent<Renderer>().material.color = Color.green;
	}

	void UpdateDescriptions () {
		moduleStats.text = focusModule.ToString ();
	}

	public void UpgradeModule () {

		Module[] temp = null;
		if (Input.GetButtonDown ("LShift")) {
			temp = modules.ToArray ();
		} else {
			temp = new Module[1];
			temp [0] = focusModule;
		}

		foreach (Module module in temp) {
			if (module.upgradeCost < Game.credits) {
				Game.credits -= module.upgradeCost;
				if (module.UpgradeModule ()) {
					upgradeButton.interactable = false;
					upgradeButton.GetComponent<HoverContextElement> ().text = "Maxed Out";
					UpdateDescriptions ();
					UpdateDescriptions ();
					UpdateRangeIndicator ();
					UpdateDescText ();
					return;
				}
				UpdateDescriptions ();
				upgradeButton.GetComponent<HoverContextElement> ().text = "Upgrade Module: " + module.upgradeCost.ToString () + " LoC";
			}
			UpdateDescriptions ();
			UpdateRangeIndicator ();
			UpdateDescText ();
			return;
		}
	}

	public void StockModule () {
		Module[] temp = null;
		if (Input.GetButtonDown ("LShift")) {
			temp = modules.ToArray ();
		} else {
			temp = new Module[1];
			temp [0] = focusModule;
		}
		
		foreach (Module module in temp) {
			module.StockModule ();
			RemoveModule (module);
		}
	}

	public void SaveModuleAssembly () {
		focusModule.SaveModuleAssembly (focusModule.rootModule.assemblyName);
	}

	public void ExitMenu () {
		OpenRangeIndicator ();
		modules.Clear ();
		gameObject.SetActive (false);
		rangeIndicator.NullifyParent ();
	}

	void GetRange (float range) {
		indicatorRange = range;
	}

	public void AddModule (Module module) {
		if (module) {
			if (!modules.Contains (module)) {
				modules.Add (module);
				UpdateModuleTree ();
			}
		}
	}

	public void RemoveModule (Module module) {
		if (modules.Contains (module)) {
			modules.Remove (module);
			UpdateModuleTree ();
		}
	}

	public void UpdateModuleTree () {
		OpenRangeIndicator ();

		if (moduleTreeButtons != null)
			foreach (GameObject butt in moduleTreeButtons) {
				Destroy (butt);
			}

		GameObject nButton = null;
		moduleTreeButtons = new GameObject[modules.Count];
		for (int i = 0; i < modules.Count; i++) {

			nButton = (GameObject)Instantiate (treeButtonPrefab, treeButtonParent.position + Vector3.down * 60f * i, Quaternion.identity);

			nButton.transform.SetParent (treeButtonParent, true);
			Button b = nButton.GetComponent<Button>();
			AddListenerToModuleButton (b, i);
			b.transform.GetChild (0).GetComponent<Image>().sprite = modules[i].transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
			b.GetComponent<HoverContextElement>().text = modules[i].moduleName;
			moduleTreeButtons[i] = nButton;

		}

		treeScrollContext.sizeDelta = new Vector2 (treeScrollContext.sizeDelta.x,modules.Count * 60f + 5 - treeScrollContext.rect.height);
	}

	void UpdateRangeIndicator () {
		indicatorRange = 0f;
		RangeIndicator.ForceRequestRange (focusModule.gameObject, gameObject);
		rangeIndicator.transform.position = focusModule.transform.position;
		
		if (focusModule.moduleType == Module.Type.Weapon) {
			if (focusModule.parentBase) {
				rangeIndicator.GetRange (focusModule.parentBase.GetRange () * indicatorRange);
			}else{
				rangeIndicator.GetRange (indicatorRange * WeaponModule.indieRange);
			}
		}else{
			rangeIndicator.GetRange (indicatorRange);
		}
	}

	void UpdateDescText () {
		moduleDesc.text = "A class " + focusModule.moduleClass + ", rank " + (focusModule.upgradeCount + 1).ToString () + " " + focusModule.moduleType.ToString () + " module - " + focusModule.moduleDesc;
	}

	public void OpenFocusModule (Module module) {
		this.focusModule = module;
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
	}
	

	void AddListenerToModuleButton (Button button, int index) {
		button.onClick.AddListener (() => {
			OpenFocusModule (modules[index]);
			foreach (GameObject b in moduleTreeButtons) {
				b.GetComponent<Button>().interactable = true;
			}
			button.interactable = false;
		});
	}
}
