using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class AssembliesSelectionMenu : MonoBehaviour {

    public static Game game;
    public static AssembliesSelectionMenu cur;

    public GameObject assemblyButtonPrefab;
    public RectTransform assemblyButtonStart;
    public float assemblyButtonSize;
    public float extraSize;

    public BattlefieldSelectionMenu battlefieldSelector;
    public DifficultySelector difficultySelector;

	public List<Assembly> tempLoaded;

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
        cur = this;
        InitializeArray ();
        ToggleAll ();
	}

    void SortAssemblies (ref List<Assembly> assemblies) {
        assemblies.Sort (new Assembly.Comparer ());
    }

    void InitializeArray () {
		header.text = "Select assemblies";
		footer.text = "No tier 0 detected";

        string[] files = Directory.GetFiles (Game.MODULE_ASSEMBLY_SAVE_DIRECTORY, "*" + Module.MODULE_FILE_EXTENSION);
        tempLoaded = new List<Assembly> ();

        for (int i = 0; i < files.Length; i++) {
            tempLoaded.Add (Assembly.LoadFromFile (files[i], true));
        }

        SortAssemblies (ref tempLoaded);

        RectTransform rect = GetComponent<RectTransform> ();
        scrollRect.sizeDelta = new Vector2 (rect.sizeDelta.x, assemblyButtonSize * files.Length + 2);
        rect.sizeDelta = new Vector2 (rect.sizeDelta.x, assemblyButtonSize * Mathf.Min (files.Length, maxEntriesOnScreen) + extraSize);

        scrollBar.value = 0f;
        scrollBar.numberOfSteps = files.Length - maxEntriesOnScreen + 1;
        scrollRect.transform.position += Vector3.up * -assemblyButtonSize * files.Length / 2f;


        for (int i = 0; i < files.Length; i++) {
            GameObject butt = Instantiate (assemblyButtonPrefab, assemblyButtonStart.position + Vector3.down * (assemblyButtonSize) * i, Quaternion.identity);
            Button button = butt.GetComponent<Button> ();
            butt.transform.SetParent (assemblyButtonStart, true);

            buttons.Add (button);
            remaining.Add (button);

			AddAssemblyButtonListener (button, i);

			buttons[buttons.Count - 1].transform.Find ("Image").GetComponent<RawImage>().texture = tempLoaded[i].texture;
			buttons[buttons.Count - 1].transform.Find ("NameText").GetComponent<Text>().text = tempLoaded[i].assemblyName;

			int cost = 0;
			int rootClass = 0;
			int tech = 0;
            int dps = 0;

			Assembly.GetAssemblyDescData (tempLoaded[i], out cost, out rootClass, out tech, out dps);

			buttons[buttons.Count - 1].transform.Find ("DescText").GetComponent<Text>().text = "Cost: " + cost.ToString () + " - DPS: " + dps + " - Class: " + rootClass.ToString () + " - Tier: " + tech.ToString();
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
		game.purchaseMenu.SetAssemblies (assemblies);
        Game.UpdateDarkOverlay ();

        battlefieldSelector.LoadDataToGame ();
        difficultySelector.ApplyDifficulty ();

		game.StartGame ();

        //game.purchaseMenu.InitializeAssemblyButtons ();
        //game.purchaseMenu.LoadSpecialButtons ();

		gameObject.SetActive (false);
    }



    void ToggleAll () {
        // This is quite messy, but thats a theme for this entire class.
        for (int i = 0; i < buttons.Count; i++) {
            if (remaining.Contains (buttons[i])) {
                OnClickedButton (i);
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

        string message = selected.Count.ToString () + " selected";
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
