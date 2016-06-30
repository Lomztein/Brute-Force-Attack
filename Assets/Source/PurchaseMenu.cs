using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.IO;
using IngameEditors;
using System.Linq;

public class PurchaseMenu : MonoBehaviour {

    [Header ("Menu Stuff")]
    public bool isOpen;
    public RectTransform rect;

    [Header ("Purchaseables")]
    public List<GameObject> standard;
    public List<GameObject> all;

    public List<GameObject> buttons = new List<GameObject>();
    public RectTransform firstTopButton;
    private GameObject[] currentMenu;

    public Dictionary<GameObject, int> stockModules;

    [Header ("References")]
    public GameObject buttonPrefab;
    public Transform buttonMask;
    public GameObject assemblyLoader;
    public GameObject assemblyButton;
    public Transform assemblyButtonStart;
    public List<LoadAssemblyButton> assemblyButtonList = new List<LoadAssemblyButton> ();

    public RectTransform scrollThingie;
    public PlayerInput playerInput;
    public static PurchaseMenu cur;

    public int purchaseButtonSize = 75;
    public static Assembly[] assemblies;
    public Assembly[] specialAssemblies;

    public void Initialize () {
        cur = this;
        stockModules = new Dictionary<GameObject, int> ();
        CollectAllPurchaseables ();

        if (Game.currentScene == Scene.AssemblyBuilder)
            LoadStandardButtons ();
    }

    public void LoadStandardButtons () {
        CloseAssemblyButtons ();
        InitializePurchaseMenu (standard.ToArray ());
    }

    [System.Obsolete ("Special assemblies have been merged with the assembly system", true)]
    public void LoadSpecialButtons () {
        //InitializePurchaseMenu (special.ToArray ());
    }

    public void SetAssemblies ( List<Assembly> _assemblies ) {
        assemblies = _assemblies.ToArray ();
    }

    public List<Assembly> GetAssemblies () {
        return assemblies.ToList ();
    }

    public void InitializeAssemblyButtons () {
        CollectAllPurchaseables ();
        assemblyButtonList.Clear ();
        foreach (Transform child in assemblyButtonStart) {
            Destroy (child.gameObject);
        }

        // Condense these two into a single function later.
        for (int i = 0; i < assemblies.Length; i++) {
            GameObject butt = (GameObject)Instantiate (assemblyButton, assemblyButtonStart.position + Vector3.right * (purchaseButtonSize) * i, Quaternion.identity);
            assemblyButtonList.Add (butt.GetComponent<LoadAssemblyButton> ());
            butt.transform.SetParent (assemblyButtonStart, true);
            LoadAssemblyButton button = butt.GetComponent<LoadAssemblyButton> ();
            button.assembly = assemblies[i];

            AddAssemblyButtonListener (butt.GetComponent<Button> (), button);
            button.Initialize ();
        }

        for (int i = 0; i < specialAssemblies.Length; i++) {
            GameObject butt = (GameObject)Instantiate (assemblyButton, firstTopButton.position + Vector3.left * (purchaseButtonSize) * i, Quaternion.identity);
            assemblyButtonList.Add (butt.GetComponent<LoadAssemblyButton> ());
            butt.transform.SetParent (firstTopButton, true);
            LoadAssemblyButton button = butt.GetComponent<LoadAssemblyButton> ();
            button.assembly = specialAssemblies[i];

            AddAssemblyButtonListener (butt.GetComponent<Button> (), button);
            button.Initialize ();
        }
    }

    public void OpenAssemblyButtons () {
        foreach (GameObject b in buttons) {
            Destroy (b);
        }

        foreach (Transform start in assemblyButtonStart) {
            start.gameObject.SetActive (true);
        }
    }

    public void CloseAssemblyButtons () {
        foreach (Transform start in assemblyButtonStart) {
            if (start.gameObject.activeSelf) {
                start.gameObject.SetActive (false);
            }
        }
    }

    public bool IsModuleAvailable ( string name ) {
        return (bool)GetModulePrefab (name);
    }

    public void LoadAssembly ( Assembly assembly ) {
        GameObject ass = Instantiate (assemblyLoader);
        ModuleAssemblyLoader loader = ass.GetComponent<ModuleAssemblyLoader> ();
        loader.LoadAssembly (assembly);
        Destroy (ass);
    }

    public GameObject GetModulePrefab ( string name ) {

        foreach (GameObject obj in all) {
            if (obj.GetComponent<Module> ().moduleName == name) {
                return obj;
            }
        }

        return null;
    }

    public void CollectAllPurchaseables () {
        all = new List<GameObject> ();
        foreach (GameObject b in standard) {
            all.Add (b);
        }

        /*foreach (GameObject a in special) {
			all.Add (a);
		}*/

        if (Game.currentScene == Scene.Play) {
            foreach (GameObject a in Game.game.researchMenu.unlockableModules) {
                all.Add (a);
            }
        }
    }

    //[System.Obsolete ("Try and use the assembly system instead plox.")]
    public void InitializePurchaseMenu (GameObject[] purchaseables) {
		if (Game.currentScene == Scene.Play) {
			CollectAllPurchaseables ();
		} else {
			all = standard;
		}

		// Remove previous buttons;
		foreach (GameObject b in buttons) {
			Destroy (b);
		}

		buttons = new List<GameObject>();
		currentMenu = purchaseables;

		// Itereate through each purchaseable, and instantiate a button for it.
		for (int i = 0; i < purchaseables.Length; i++ ) {

			//Module m = purchaseables[i].GetComponent<Module>();

			// Instantiate weapon buttons on lower button row,
			// and all other types on top row.

			GameObject newButton = (GameObject)Instantiate (buttonPrefab, firstTopButton.position + Vector3.left * purchaseButtonSize * i, Quaternion.identity);

			buttons.Add (newButton);
			Button button = newButton.GetComponent<Button>();
			newButton.transform.FindChild ("Image").GetComponent<Image>().sprite = purchaseables[i].transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
			newButton.transform.SetParent (firstTopButton, true);
			newButton.transform.localScale = Vector3.one;

			AddPurchaseButtonListener (button, i);
		}

		UpdateButtons ();
	}

	public static void UpdateButtons () {
		//PurchaseMenu menu = PurchaseMenu.cur;
		//int index = 0;
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

		foreach (LoadAssemblyButton butt in cur.assemblyButtonList) {
            butt.ButtonUpdate ();
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

	void Update () {

		// Detect mouse position

		if (isOpen && Input.mousePosition.y > 75) {
			isOpen = false;
		}

		if (Input.mousePosition.y < 75) {
			isOpen = true;
		}
	}
}
