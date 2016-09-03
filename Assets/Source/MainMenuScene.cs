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

    private bool isCreditsOpen;
    public GameObject creditsScreen;
    public SettingsMenu settingsScreen;
    public AudioSource audioSource;

	public void Play () {
        SceneManager.LoadScene ("pv_play");
        Game.currentScene = Scene.Play;
	}

    public void ToggleLoadGameMenu () {
        FileBrowser.OpenFileBrowser (Game.SAVED_GAME_DIRECTORY, gameObject, "OnSaveChosen");
        Game.UpdateDarkOverlay ();
    }

    public void OnSaveChosen (string saveName) {
        Game.saveToLoad = saveName;
        Play ();
    }

    public void OnFileBrowserClosed () {
        Game.UpdateDarkOverlay ();
    }

    public void ToggleCreditsScreen () {
        isCreditsOpen = !isCreditsOpen;
        creditsScreen.SetActive (isCreditsOpen);
        Game.UpdateDarkOverlay ();
    }

    public void ToggleSettingsScreen () {
        settingsScreen.gameObject.SetActive (!settingsScreen.gameObject.activeSelf);
        Game.UpdateDarkOverlay ();
    }

    void Start () {
        Game.darkOverlayActive = true;
        Game.InitializeDirectories ();
        Game.currentScene = Scene.Menu;
        Game.DeleteAssemblySave ();
        Game.UpdateDarkOverlay ();
        audioSource.Play ();
        SettingsMenu.cur = settingsScreen;  
        SettingsMenu.LoadSettings ();
    }

    public void Build () {
        IngameEditors.AssemblyEditorScene.openedFromIngame = false;
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
        audioSource.volume = settingsScreen.musicSlider.value;
    }
}
