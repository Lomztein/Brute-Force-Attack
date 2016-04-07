using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour {

    public Game.DifficultySettings[] difficulties;
    public int difficultySetting = 1;

    public Text header;
    public Text desciption;

    // Use this for initialization
    void Start () {
        ChangeDifficulty (0);
	}

    public void ChangeDifficulty (int movement) {
        difficultySetting += movement;

        if (difficultySetting < 0) {
            difficultySetting = difficulties.Length - 1;
        } else if (difficultySetting >= difficulties.Length) {
            difficultySetting = 0;
        }

        header.text = difficulties[difficultySetting].name;
        desciption.text = difficulties[difficultySetting].desc;
    }

    public void ApplyDifficulty () {
        Game.difficulty = difficulties[difficultySetting];
    }
}
