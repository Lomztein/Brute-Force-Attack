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

	public void Play () {
        SceneManager.LoadScene ("pv_play");
	}

    public void ToggleLoadGameMenu () {
        FileBrowser.OpenFileBrowser (Game.SAVED_GAME_DIRECTORY, gameObject, "OnSaveChosen");
        Game.ForceDarkOverlay (FileBrowser.currentBrowser);
    }

    public void OnSaveChosen (string saveName) {
        Game.saveToLoad = saveName;
        Play ();
    }

    public void OnFileBrowserClosed () {
        Game.ForceDarkOverlay (false);
    }

    void Start () {
        Game.InitializeDirectories ();
        Game.ForceDarkOverlay (false);
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
