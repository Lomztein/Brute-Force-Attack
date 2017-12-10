using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class Highscore : MonoBehaviour {

    public static Highscore instance;
    public Text names;
    public Text scores;
    public Text header;

    public InputField inputName;
    public Button submitButton;

    public static void Add ( string playerName, int score) {
        List<Entry> entries = Utility.LoadObjectFromFile<List<Entry>> (GetFileName ());
        if (entries == null)
            entries = new List<Entry> ();

        entries.Add (new Entry (playerName, score));
        entries.Sort (new Entry.Comparer ());
        Utility.SaveObjectToFile (GetFileName (), entries);
    }

    private static string GetFileName() {
        return Game.GAME_DATA_DIRECTORY + Game.difficulty.name + "_" + Game.game.battlefieldName + ".dat";
    }

    private static string GetHeaderName() {
        return Game.game.battlefieldName + " - " +Game.difficulty.name;
    }

    public void AddPlayer () {
        if (IsInputEmpty ()) {
            Game.game.QuitToMenu ();
        } else {
            Add (inputName.text, EnemyManager.ExternalWaveNumber);
            inputName.interactable = false;
            Display (header, names, scores);
        }
        OnInputNameChanged ();
    }

    public void InstanceDisplay () {
        instance = this;
        gameObject.SetActive (true);
        Display (header, names, scores);
        OnInputNameChanged ();
    }

    public static void Display (Text header, Text names, Text scores) {
        List<Entry> entries = Utility.LoadObjectFromFile<List<Entry>> (GetFileName ());
        if (entries == null)
            entries = new List<Entry> ();

        string nameText = "";
        string scoreText = "";
        

        for (int i = 0; i < Mathf.Min (entries.Count, 10); i++) {
            nameText += (i+1) + " - " + entries[i].playerName + "\n\n";
            scoreText += entries[i].scoreValue + "\n\n";
        }
        for (int i = entries.Count; i < 10; i++) {
            nameText += (i + 1) + " - N/A\n\n";
        }

        header.text = GetHeaderName ();
        names.text = nameText;
        scores.text = scoreText;
    }

    public void OnInputNameChanged () {
        Text text = submitButton.GetComponentInChildren<Text> ();
        if (IsInputEmpty ()) {
            text.text = "Quit";
        }else {
            text.text = "Submit";
        }
    }

    private bool IsInputEmpty () {
        if (!inputName.interactable)
            return true;
        return inputName.text == "";
    }

    [System.Serializable]
    public class Entry {

        public string playerName;
        public int scoreValue;

        public Entry (string n, int s) {
            playerName = n;
            scoreValue = s;
        }

        public class Comparer : IComparer<Entry> {
            public int Compare(Entry x, Entry y) {
                return Mathf.Clamp (y.scoreValue - x.scoreValue, -1, 1); // Unsure if clamping is neccesary, but why not I guess /shrug.
            }
        }
    }
}
