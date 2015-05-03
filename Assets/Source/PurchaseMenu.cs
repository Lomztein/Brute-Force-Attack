using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.IO;

public class PurchaseMenu : MonoBehaviour {

	private float targetHeight = -Screen.height / 2 - 120;

	[Header ("Menu Stuff")]
	public bool isOpen;
	public RectTransform rect;

	[Header ("Purchaseables")]
	public List<GameObject> standard;
	public List<GameObject> special;
	public List<GameObject> all;

	public List<GameObject> buttons = new List<GameObject>();
	public RectTransform firstTopButton;
	public RectTransform firstButtomButton;
	private GameObject[] currentMenu;

	public Dictionary<GameObject, int> stockModules;

	[Header ("References")]
	public GameObject buttonPrefab;
	public Transform buttonMask;
	public GameObject assemblyLoader;
	public GameObject assemblyButton;
	public Transform assemblyButtonStart;

	public RectTransform scrollThingie;
	public PlayerInput playerInput;
	public static PurchaseMenu cur;

	// Use this for initialization
	// TODO Implement multiple raycasts before placing objects.

	public void LoadStandardButtons () {
		CloseAssemblyButtons ();
		InitializePurchaseMenu (standard.ToArray ());
	}

	public void LoadSpecialButtons () {
		CloseAssemblyButtons ();
		InitializePurchaseMenu (special.ToArray ());
	}

	public void InitialzeAssemblyButtons () {
		foreach (Transform child in assemblyButtonStart) {
			Destroy (child.gameObject);
		}

		string[] files = Directory.GetFiles (Game.MODULE_ASSEMBLY_SAVE_DIRECTORY);

		for (int i = 0; i < files.Length; i++) {
			GameObject butt = (GameObject)Instantiate (assemblyButton, assemblyButtonStart.position + Vector3.right * (195 * i), Quaternion.identity);
			butt.transform.SetParent (assemblyButtonStart, true);
			LoadAssemblyButton button = butt.GetComponent<LoadAssemblyButton>();
			button.path = files[i];
			button.OnResearchUnlocked ();
			AddAssemblyButtonListener (butt.GetComponent<Button>(), button);
		}

	}

	public void OpenAssemblyButtons () {
		foreach (GameObject b in buttons) {
			Destroy (b);
		}

		assemblyButtonStart.gameObject.SetActive (true);
		scrollThingie.sizeDelta = new Vector2 (205 * assemblyButtonStart.childCount, scrollThingie.sizeDelta.y);
	}

	public void CloseAssemblyButtons () {
		if (assemblyButtonStart.gameObject.activeSelf) {
			assemblyButtonStart.gameObject.SetActive (false);
		}
	}

	public bool IsModuleAvailable (string name) {
		return (bool)GetModulePrefab (name);
	}

	public void LoadAssembly (string path) {
		if (!playerInput.isPlacing) {
			GameObject ass = (GameObject)Instantiate (assemblyLoader);
			ModuleAssemblyLoader loader = ass.GetComponent<ModuleAssemblyLoader>();
			loader.LoadAssembly (path);
			Destroy (ass);
		}
	}

	public GameObject GetModulePrefab (string name) {

		foreach (GameObject obj in all) {
			if (obj.GetComponent<Module>().moduleName == name) {
				return obj;
			}
		}

		return null;
	}

	void CollectAllPurchaseables () {
		foreach (GameObject b in standard) {
			all.Add (b);
		}
		
		foreach (GameObject a in special) {
			all.Add (a);
		}
	}

	public void InitializePurchaseMenu (GameObject[] purchaseables) {
		CollectAllPurchaseables ();

		cur = this;
		// Remove previous buttons;
		foreach (GameObject b in buttons) {
			Destroy (b);
		}

		buttons = new List<GameObject>();
		currentMenu = purchaseables;

		// Count weapons and other types
		int w = 0;
		int o = 0;

		// Itereate through each purchaseable, and instantiate a button for it.
		for (int i = 0; i < purchaseables.Length; i++ ) {

			Module m = purchaseables[i].GetComponent<Module>();

			// Instantiate weapon buttons on lower button row,
			// and all other types on top row.

			GameObject newButton = null;
			if (m.moduleType == Module.Type.Weapon) {
				newButton = (GameObject)Instantiate (buttonPrefab, firstButtomButton.position + Vector3.right * 94 * w, Quaternion.identity);
				w++;
			}else{
				newButton = (GameObject)Instantiate (buttonPrefab, firstTopButton.position + Vector3.right * 94 * o, Quaternion.identity);
				o++;
			}

			buttons.Add (newButton);
			Button button = newButton.GetComponent<Button>();
			newButton.transform.GetChild (0).GetComponent<Image>().sprite = purchaseables[i].transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
			newButton.transform.SetParent (buttonMask, true);
			newButton.transform.localScale = Vector3.one;
			AddPurchaseButtonListener (button, i);

		}

		int max = Mathf.Max (w,o);
		int newX = max * 94 + 30;
		scrollThingie.sizeDelta = new Vector2 (newX, scrollThingie.sizeDelta.y);
		scrollThingie.localPosition += new Vector3 (scrollThingie.localPosition.x + newX/4f, scrollThingie.localPosition.y);
		UpdateButtons ();
	}

	public static void UpdateButtons () {
		PurchaseMenu menu = PurchaseMenu.cur;
		int index = 0;
		foreach (GameObject mod in menu.currentMenu) {

			if (menu.buttons != null && menu.buttons[index]) {

				if (menu.IsOptionAvailable (mod)) {
					menu.buttons[index].transform.FindChild ("Image").GetComponent<Image>().color = Color.white;
					if (menu.stockModules.ContainsKey (mod)) {
						menu.buttons[index].transform.FindChild ("Amount").GetComponent<Text>().text = menu.stockModules[mod].ToString ();
					}else{
						menu.buttons[index].transform.FindChild ("Amount").GetComponent<Text>().text = "";
					}
				}else{
					menu.buttons[index].transform.FindChild ("Image").GetComponent<Image>().color /= 2;
				}

				index++;
			
			}
		}
	}

	bool IsOptionAvailable (GameObject purchaseable) {
		if (stockModules.ContainsKey (purchaseable)) {
			if (stockModules[purchaseable] > 0) {
				return true;
			}
		}

		Module m = purchaseable.GetComponent<Module>();
		if (m.moduleCost <= Game.credits) {
			return true;
		}

		return false;
	}

	void AddPurchaseButtonListener (Button button, int index) {
		button.onClick.AddListener (() => {
			SelectPurchaseable (index);
		});

		Module m = currentMenu[index].GetComponent<Module>();
		button.GetComponent<HoverContextElement>().text = m.moduleName + ", " + m.moduleCost.ToString () + " LoC";
	}

	void AddAssemblyButtonListener (Button button, LoadAssemblyButton loadButton) {
		button.onClick.AddListener (() => {
			loadButton.Purchase ();
		});
		
		button.GetComponent<HoverContextElement>().text = loadButton.assemblyName + ", " + loadButton.cost.ToString () + " LoC";
		button.transform.FindChild ("Text").GetComponent<Text>().text = loadButton.assemblyName;
	}

	public void SelectPurchaseable (int index) {
		playerInput.SelectPurchaseable (currentMenu[index]);
	}

	public static void AddStock (Module module) {
		PurchaseMenu menu = PurchaseMenu.cur;
		GameObject obj = menu.GetModulePrefab (module.moduleName);
		if (menu.stockModules.ContainsKey (obj)) {
			menu.stockModules[obj]++;
		}else{
			menu.stockModules.Add (obj, 1);
		}

		UpdateButtons ();
	}

	// Update is called once per frame
	void Update () {

		// Animate closing and opening
		rect.localPosition = new Vector3 (rect.localPosition.x, Mathf.Lerp (rect.localPosition.y, targetHeight, 30f * Time.deltaTime));

		if (isOpen && Input.mousePosition.y > 300) {
			isOpen = false;
			targetHeight = -Screen.height / 2 - 120;
		}

		if (Input.mousePosition.y < 30) {
			isOpen = true;
			targetHeight = -Screen.height / 2 + 150;
		}
	
	}
}
