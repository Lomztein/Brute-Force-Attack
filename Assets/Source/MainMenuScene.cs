using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class MainMenuScene : MonoBehaviour {

    public float spawnWidth;
    public float enemySpawnHeight;
    public float enemyDestroyHeight;

    public GameObject[] fakeEnemies;
    public int[] spawnChances;
    public List<FakeEnemy> enemies;

    public float progress;
    public float maxProgress;

    public string[] loadableGames;
    public GameObject loadGameMenu;
    public GameObject loadGameButtonPrefab;

	public void Play () {
        SceneManager.LoadScene ("pv_play");
	}

    public void ToggleLoadGameMenu () {
        loadGameMenu.SetActive (!loadGameMenu.activeSelf);
        Game.ForceDarkOverlay (loadGameMenu.activeSelf);
    }

    void Start () {
        Game.InitializeDirectories ();
        Game.ForceDarkOverlay (false);

        loadableGames = Directory.GetFiles (Game.SAVED_GAME_DIRECTORY, "*.dat");
        for (int i = 0; i < loadableGames.Length; i++) {
            string name = ExtractSaveName (loadableGames[i]);
            string date = File.GetCreationTime (loadableGames[i]).ToString ();
            GameObject button = Instantiate (loadGameButtonPrefab);
            button.transform.SetParent (loadGameMenu.transform);
            button.transform.FindChild ("Name").GetComponent<Text> ().text = "  " + name;
            button.transform.FindChild ("Date").GetComponent<Text> ().text = date + "  ";
            AddLoadGameButtonListener (button.GetComponent<Button> (), i);
        }
    }

    void AddLoadGameButtonListener (Button button, int index) {
        button.onClick.AddListener (() => {
            Game.saveToLoad = ExtractSaveName (loadableGames[index]);
            Play ();
        });
    }

    string ExtractSaveName (string file) {
        string f = file.Substring (file.LastIndexOf ('/') + 1);
        return f.Substring (0, f.Length - ".dat".Length);
    }

    public void Build () {
        SceneManager.LoadScene ("pv_assemblybuilder");
	}

	public void Quit () {
		Application.Quit ();
	}

    void FixedUpdate () {
           
        for (int i = 0; i < fakeEnemies.Length; i++) {
            float reqProgress = (float)i / fakeEnemies.Length * maxProgress;
            if (progress > reqProgress && Random.Range (0, spawnChances[i]) == 0) {
                enemies.Add(((GameObject)Instantiate(fakeEnemies[i], new Vector3 (Random.Range (-spawnWidth, spawnWidth), enemySpawnHeight), Quaternion.identity)).GetComponent<FakeEnemy>());
            }
        }

        for (int i = 0; i < enemies.Count; i++) {
            enemies[i].transform.position += Vector3.up * enemies[i].speed * -Time.fixedDeltaTime;
            if (enemies[i].transform.position.y < enemyDestroyHeight) {
                Destroy(enemies[i].gameObject);
                enemies.RemoveAt(i);
            }
        }

        progress += Time.fixedDeltaTime;
    }
}
