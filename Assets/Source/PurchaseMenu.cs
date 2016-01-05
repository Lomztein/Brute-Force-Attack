using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.IO;
using IngameEditors;

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
	public Transform[] assemblyButtonStart;
	public List<LoadAssemblyButton> assemblyButtonList = new List<LoadAssemblyButton> ();

	public RectTransform scrollThingie;
	public PlayerInput playerInput;
	public static PurchaseMenu cur;

	public int purchaseButtonSize = 75;

	public void Initialize () {
		cur = this;
		stockModules = new System.Collections.Generic.Dictionary<GameObject, int>();
		InitializeAssemblyButtons ();

		if (Game.currentScene == Scene.AssemblyBuilder)
			LoadStandardButtons ();

		UpdateButtons ();
	}

	public void LoadStandardButtons () {
		CloseAssemblyButtons ();
		InitializePurchaseMenu (standard.ToArray ());
	}

	public void LoadSpecialButtons () {
		CloseAssemblyButtons ();
		InitializePurchaseMenu (special.ToArray ());
	}

	public void InitializeAssemblyButtons () {
		CollectAllPurchaseables ();
		for (int i = 0; i < assemblyButtonStart.Length; i++) {
			foreach (Transform child in assemblyButtonStart[i]) {
				Destroy (child.gameObject);
			}
		}

		string[] files = Directory.GetFiles (Game.MODULE_ASSEMBLY_SAVE_DIRECTORY, "*" + Module.MODULE_FILE_EXTENSION);
		int[] index = new int[assemblyButtonStart.Length];

		for (int i = 0; i < files.Length; i++) {
			GameObject butt = (GameObject)Instantiate (assemblyButton, assemblyButtonStart[i % 2].position + Vector3.right * (purchaseButtonSize * index[i % 2]), Quaternion.identity);
			assemblyButtonList.Add (butt.GetComponent<LoadAssemblyButton>());
			butt.transform.SetParent (assemblyButtonStart[i % 2], true);
			LoadAssemblyButton button = butt.GetComponent<LoadAssemblyButton>();
			button.path = files[i];
			button.OnResearchUnlocked ();
			AddAssemblyButtonListener (butt.GetComponent<Button>(), button);
			button.Initialize ();
			index[i % 2]++;
		}

	}

	public void OpenAssemblyButtons () {
		foreach (GameObject b in buttons) {
			Destroy (b);
		}

		foreach (Transform start in assemblyButtonStart) {
			start.gameObject.SetActive (true);
		}

		int size = Mathf.Max (assemblyButtonStart [0].childCount, assemblyButtonStart [1].childCount);
		scrollThingie.sizeDelta = new Vector2 (purchaseButtonSize * size, scrollThingie.sizeDelta.y);
	}

	public void CloseAssemblyButtons () {
		foreach (Transform start in assemblyButtonStart) {
			if (start.gameObject.activeSelf) {
				start.gameObject.SetActive (false);
			}
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

		if (Game.game) {
			foreach (GameObject a in Game.game.researchMenu.unlockableModules) {
				all.Add (a);
			}
		}
	}

	public void InitializePurchaseMenu (GameObject[] purchaseables) {
		if (Game.currentScene == Scene.Play) {
			CollectAllPurchaseables ();
		} else {
			all = standard;
		}

		// Count weapons and other types
		int w = 0;
		int o = 0;

		foreach (GameObject purchaseable in purchaseables) {
			Module locM = purchaseable.GetComponent<Module>();
			if (locM.moduleType == Module.Type.Weapon) {
				w++;
			}else{
				o++;
			}
		}

		int max = Mathf.Max (w,o);
		int newX = max * purchaseButtonSize + 30;
		scrollThingie.sizeDelta = new Vector2 (newX, scrollThingie.sizeDelta.y);
		scrollThingie.localPosition += new Vector3 (scrollThingie.localPosition.x + newX/4f, scrollThingie.localPosition.y);

		w = 0;
		o = 0;

		// Remove previous buttons;
		foreach (GameObject b in buttons) {
			Destroy (b);
		}

		buttons = new List<GameObject>();
		currentMenu = purchaseables;

		// Itereate through each purchaseable, and instantiate a button for it.
		for (int i = 0; i < purchaseables.Length; i++ ) {

			Module m = purchaseables[i].GetComponent<Module>();

			// Instantiate weapon buttons on lower button row,
			// and all other types on top row.

			GameObject newButton = null;
			if (m.moduleType == Module.Type.Weapon) {
				newButton = (GameObject)Instantiate (buttonPrefab, firstButtomButton.position + Vector3.right * purchaseButtonSize * w, Quaternion.identity);
				w++;
			}else{
				newButton = (GameObject)Instantiate (buttonPrefab, firstTopButton.position + Vector3.right * purchaseButtonSize * o, Quaternion.identity);
				o++;
			}

			buttons.Add (newButton);
			Button button = newButton.GetComponent<Button>();
			newButton.transform.FindChild ("Image").GetComponent<Image>().sprite = purchaseables[i].transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
			newButton.transform.SetParent (buttonMask, true);
			newButton.transform.localScale = Vector3.one;
			AddPurchaseButtonListener (button, i);

		}

		UpdateButtons ();
	}

	public static void UpdateButtons () {
		PurchaseMenu menu = PurchaseMenu.cur;
		int index = 0;
		/*foreach (GameObject mod in menu.currentMenu) {

			if (menu.buttons != null && menu.buttons[index]) {

				if (menu.IsOptionAvailable (mod)) {
					menu.buttons[index].transform.FindChild ("Image").GetComponent<Image>().color = Color.white;
					if (menu.stockModules.ContainsKey (mod)) {
						menu.buttons[index].transform.FindChild ("Amount").GetComponent<Text>().text = menu.stockModules[mod].ToString ();
					}else{
						menu.buttons[index].transform.FindChild ("Amount").GetComponent<Text>().text = "";
					}
				}else{
					menu.buttons[index].transform.FindChild ("Image").GetComponent<Image>().color = new Color (0.5f, 0.5f, 0.5f, 0.5f);
				}

				index++;
			
			}
		}*/
		// I'm really just hacking this stuff together at this point.

		foreach (LoadAssemblyButton butt in PurchaseMenu.cur.assemblyButtonList) {
			butt.button.interactable = butt.cost <= Game.credits;
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
		playerInput.SelectPurchaseable (currentMenu[index], true);
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
