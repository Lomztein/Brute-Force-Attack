using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class AssembliesSelectionMenu : MonoBehaviour {

    public Game game;

    public GameObject assemblyButtonPrefab;
    public RectTransform assemblyButtonStart;
    public float assemblyButtonSize;
    public float extraSize;

    public List<Button> buttons;
    public List<Button> selected;
    public List<Button> remaining;

    public ColorBlock unselectedColors;
    public ColorBlock selectedColors;

	// Use this for initialization
	void Start () {
        game = Game.game;
        InitializeArray ();
	}

    void InitializeArray () {
        string[] files = Directory.GetFiles (Game.MODULE_ASSEMBLY_SAVE_DIRECTORY, "*" + Module.MODULE_FILE_EXTENSION);
        Debug.Log (files.Length);

        RectTransform rect = GetComponent<RectTransform> ();
        rect.sizeDelta = new Vector2 (rect.sizeDelta.x, assemblyButtonSize * files.Length + extraSize);
        // rect.transform.position += Vector3.up * -assemblyButtonSize * files.Length / 2f;

        for (int i = 0; i < files.Length; i++) {
            GameObject butt = (GameObject)Instantiate (assemblyButtonPrefab, assemblyButtonStart.position + Vector3.down * (assemblyButtonSize) * i, Quaternion.identity);
            Button button = butt.GetComponent<Button> ();
            butt.transform.SetParent (assemblyButtonStart, true);

            buttons.Add (button);
            remaining.Add (button);
        }
    }

    void AddAssemblyButtonListener (Button button, int index) {
        button.onClick.AddListener (() => {
            OnClickedButton (index);
        });
    }

    void SelectAssemblies () {
        // Save the selected.
    }

    void OnClickedButton (int index) {
        Button button = buttons[index];
        if (remaining.Contains (button)) {

            selected.Add (button);
            remaining.Remove (button);
            button.colors = selectedColors;
        } else {

            selected.Remove (button);
            remaining.Add (button);
            button.colors = unselectedColors;
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
    }
}
