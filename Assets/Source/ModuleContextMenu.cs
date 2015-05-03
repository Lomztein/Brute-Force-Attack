using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ModuleContextMenu : MonoBehaviour {

	public Image moduleImage;
	public Text moduleName;
	public Text moduleDesc;
	public Text moduleStats;
	public GameObject treeButtonPrefab;
	public Transform treeButtonParent;
	public RectTransform treeScrollContext;
	public Text assemblyName;

	private Module module;
	private Module[] moduleTree;
	private GameObject[] moduleTreeButtons;

	// Update is called once per frame
	void Update () {
		if (!module)
			ExitMenu ();

		if (Input.GetButtonDown ("Cancel"))
			ExitMenu ();
	}

	public void SellModule () {
		module.SellModule ();
	}

	public void StockModule () {
		module.StockModule ();
	}

	public void SaveModuleAssembly () {
		module.SaveModuleAssembly (module.rootModule.assemblyName);
	}

	public void ExitMenu () {
		gameObject.SetActive (false);
	}

	public void OnAssemblyNameChange () {
		module.rootModule.assemblyName = assemblyName.text;
	}

	public void UpdateModuleTree () {
		moduleTree = module.GetModuleTree ();

		if (moduleTreeButtons != null)
			foreach (GameObject butt in moduleTreeButtons) {
				Destroy (butt);
			}

		GameObject nButton = null;
		moduleTreeButtons = new GameObject[moduleTree.Length];
		for (int i = 0; i < moduleTree.Length; i++) {

			nButton = (GameObject)Instantiate (treeButtonPrefab, treeButtonParent.position + Vector3.down * 60f * i, Quaternion.identity);

			nButton.transform.SetParent (treeButtonParent, true);
			Button b = nButton.GetComponent<Button>();
			AddListenerToModuleButton (b, i);
			b.transform.GetChild (0).GetComponent<Image>().sprite = moduleTree[i].transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
			b.GetComponent<HoverContextElement>().text = moduleTree[i].moduleName;
			moduleTreeButtons[i] = nButton;

		}

		treeScrollContext.sizeDelta = new Vector2 (treeScrollContext.sizeDelta.x,moduleTree.Length * 60f + 5 - treeScrollContext.rect.height);
	}

	public void OpenModule (Module module) {
		this.module = module;
		moduleImage.sprite = module.transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
		moduleName.text = module.moduleName;
		moduleStats.text = module.ToString ();
		moduleDesc.text = "A class " + module.moduleClass + " " + module.moduleType.ToString () + " module - " + module.moduleDesc;
		if (module.assemblyName != "") {
			assemblyName.text = module.rootModule.assemblyName;
		}else{
			module.rootModule.assemblyName = assemblyName.text;
		}
		UpdateModuleTree ();
	}

	void AddListenerToUpgradeButton (Button button, Upgrade upgrade) {
		button.onClick.AddListener (() => {
			module.SendMessage (upgrade.upgradeFunction);
		});
	}

	void AddListenerToModuleButton (Button button, int index) {
		button.onClick.AddListener (() => {
			OpenModule (moduleTree[index]);
		});
	}
}
