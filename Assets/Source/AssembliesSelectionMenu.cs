using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class AssembliesSelectionMenu : MonoBehaviour {

    public Game game;

    public GameObject assemblyButtonPrefab;
    public RectTransform assemblyButtonStart;
    public float assemblyButtonSize;
    public float extraSize;

    public BattlefieldSelectionMenu battlefieldSelector;

	public Assembly[] tempLoaded;

    [Header ("Buttons")]
    public List<Button> buttons;
    public List<Button> selected;
    public List<Button> remaining;
	public List<Assembly> assemblies;

    public ColorBlock unselectedColors;
    public ColorBlock selectedColors;
    public Button startButton;

    [Header ("Graphics")]
	public Text header;
	public Text footer;
    public RectTransform scrollRect;
    public Scrollbar scrollBar;

    [Header ("Scrolling")]
    public int maxEntriesOnScreen = 8;

	// Use this for initialization
	void Start () {
        game = Game.game;
        InitializeArray ();
	}

    List<Assembly> SortAssemblies (List<Assembly> assemblies) {
        int classTypes = 4;

        List<Assembly>[] newList = new List<Assembly>[classTypes];
        for (int i = 0; i < classTypes; i++) {
            newList[i] = new List<Assembly> ();
        }

        int curCost = int.MaxValue;
        Assembly curAssembly = null;
        int rootClass = 1;

        while (assemblies.Count > 0) {

            for (int i = 0; i < assemblies.Count; i++) {
                Assembly cur = assemblies[i];

                int co;
                int rc;
                int tl;

                GetAssemblyDescData (cur, out co, out rc, out tl);

                if (co <= curCost) {
                    curAssembly = cur;
                    rootClass = rc;
                }
            }

            assemblies.Remove (curAssembly);
            newList[rootClass].Add (curAssembly);
        }

        List<Assembly> endList = new List<Assembly> ();
        for (int i = 0; i < classTypes; i++) {
            endList.AddRange (newList[i]);
        }

        return endList;
    }

    void InitializeArray () {
		header.text = "Select " + game.assembliesAllowed.ToString () + " assemblies";
		footer.text = selected.Count.ToString () + " / " + game.assembliesAllowed.ToString () + " selected";

        string[] files = Directory.GetFiles (Game.MODULE_ASSEMBLY_SAVE_DIRECTORY, "*" + Module.MODULE_FILE_EXTENSION);
        tempLoaded = new Assembly[files.Length];

        for (int i = 0; i < files.Length; i++) {
            tempLoaded[i] = Assembly.LoadFromFile (files[i], true);
        }

        tempLoaded = SortAssemblies (tempLoaded.ToList ()).ToArray ();

        RectTransform rect = GetComponent<RectTransform> ();
        scrollRect.sizeDelta = new Vector2 (rect.sizeDelta.x, assemblyButtonSize * files.Length + 2);
        rect.sizeDelta = new Vector2 (rect.sizeDelta.x, assemblyButtonSize * Mathf.Min (files.Length, maxEntriesOnScreen) + extraSize);

        scrollBar.value = 0f;
        scrollBar.numberOfSteps = files.Length - maxEntriesOnScreen + 1;
        scrollRect.transform.position += Vector3.up * -assemblyButtonSize * files.Length / 2f;


        for (int i = 0; i < files.Length; i++) {
            GameObject butt = (GameObject)Instantiate (assemblyButtonPrefab, assemblyButtonStart.position + Vector3.down * (assemblyButtonSize) * i, Quaternion.identity);
            Button button = butt.GetComponent<Button> ();
            butt.transform.SetParent (assemblyButtonStart, true);

            buttons.Add (button);
            remaining.Add (button);

			AddAssemblyButtonListener (button, i);

			buttons[buttons.Count - 1].transform.FindChild ("Image").GetComponent<RawImage>().texture = tempLoaded[i].texture;
			buttons[buttons.Count - 1].transform.FindChild ("NameText").GetComponent<Text>().text = tempLoaded[i].assemblyName;

			int cost = 0;
			int rootClass = 0;
			int tech = 0;

			GetAssemblyDescData (tempLoaded[i], out cost, out rootClass, out tech);

			buttons[buttons.Count - 1].transform.FindChild ("DescText").GetComponent<Text>().text = "Cost: " + cost.ToString () + " - Class: " + rootClass.ToString () + " - Tech Level: " + tech.ToString();
            buttons[buttons.Count - 1].transform.name = tech.ToString ();
        }
    }

    void AddAssemblyButtonListener (Button button, int index) {
        button.onClick.AddListener (() => {
            OnClickedButton (index);
        });
    }

    public void SelectAssemblies () {
        // battlefieldSelector.StartGame ();
		game.purchaseMenu.GetAssemblies (assemblies);
		game.StartGame ();

        game.purchaseMenu.InitializeAssemblyButtons ();
        game.purchaseMenu.LoadSpecialButtons ();
		gameObject.SetActive (false);
    }

	void GetAssemblyDescData (Assembly assembly, out int cost, out int rootClass, out int techLevel) {
		rootClass = PurchaseMenu.cur.GetModulePrefab (assembly.parts[0].type).GetComponent<Module>().moduleClass;
		cost = 0;
		techLevel = 0;

		for (int i = 0; i < assembly.parts.Count; i++) {
			GameObject mod = PurchaseMenu.cur.GetModulePrefab (assembly.parts[i].type);
			cost += mod.GetComponent<Module>().moduleCost;
			for (int j = 0; j < ResearchMenu.cur.research.Count; j++) {
				Research research = ResearchMenu.cur.research[j];

				if (research.func == "UnlockModule") {
					if (mod == ResearchMenu.cur.unlockableModules[research.value] && techLevel < research.y)
						techLevel = research.y;
				}
			}
		}
	}

    void OnClickedButton (int index) {
        Button button = buttons[index];
        if (remaining.Contains (button)) {

            selected.Add (button);
            remaining.Remove (button);
            button.colors = selectedColors;

			assemblies.Add (tempLoaded[index]);
        } else {

            selected.Remove (button);
            remaining.Add (button);
            button.colors = unselectedColors;

			assemblies.Remove (tempLoaded[index]);
        }

        if (selected.Count >= game.assembliesAllowed) {
            for (int i = 0; i < remaining.Count; i++) {
                remaining[i].interactable = false;
            }
        } else {
            for (int i = 0; i < remaining.Count; i++) {
                remaining[i].interactable = true;
            }
        }

        string message = selected.Count.ToString () + " / " + game.assembliesAllowed.ToString () + " selected";
        startButton.interactable = true;

        bool anyZero = false;
        for (int i = 0; i < selected.Count; i++) {
            if (int.Parse (selected[i].name) == 0)
                anyZero = true;
        }
        if (!anyZero) {
            message = "No tier 0 detected";
            startButton.interactable = false;
        }

        footer.text = message;
    }
}
