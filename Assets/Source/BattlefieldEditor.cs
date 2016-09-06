using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattlefieldEditor : MonoBehaviour {

    public GameObject battlefieldDefiner;
    public Slider widthSlider;
    public Slider heightSlider;
    public Transform battlefieldQuad;
    public Transform placePosVisualizer;

    public GameObject spawnpointPrefab;
    public List<GameObject> spawnpoints = new List<GameObject>();

    public GameObject savePanel;
    public Text saveName;
    private string saveDesc;
    private bool continueSave;

    public static BattlefieldEditor cur;

    void Awake () {
        cur = this;
        Game.currentScene = Scene.BattlefieldEditor;
    }

	void Update () {
        if (PlayerInput.cur.isPlacing) {
            Vector3 pos = (Vector2)PlayerInput.cur.RoundPos (Camera.main.ScreenToWorldPoint (Input.mousePosition), 1);
            placePosVisualizer.transform.position = pos + Vector3.back;
            if (Input.GetMouseButtonDown (0) && Game.IsInsideBattlefield (pos)) {
                spawnpoints.Add ((GameObject)Instantiate (spawnpointPrefab, (Vector2)pos, Quaternion.identity));
                PlayerInput.cur.isPlacing = false;
            }
        } else {
            placePosVisualizer.transform.position = Vector3.right * 10000;
        }
    }

    public void OnWidthChanged () {
        widthSlider.GetComponentInChildren<Text> ().text = "Width: " + widthSlider.value * 2;
    }

    public void OnHeightChanged () {
        heightSlider.GetComponentInChildren<Text> ().text = "Height: " + heightSlider.value * 2;
    }

    public void CreateBattlefield () {
        battlefieldQuad.transform.localScale = new Vector3 (widthSlider.value * 2, heightSlider.value * 2);
        battlefieldQuad.GetComponent<Renderer> ().material.mainTextureScale = battlefieldQuad.transform.localScale / 2;
        battlefieldDefiner.SetActive (false);
        Game.game.battlefieldWidth = Mathf.RoundToInt (widthSlider.value * 2);
        Game.game.battlefieldHeight = Mathf.RoundToInt (heightSlider.value * 2);
        Game.credits = 1337;
        Game.isWalled = new Game.WallType[Game.game.battlefieldWidth, Game.game.battlefieldHeight];
    }

    public void SaveBattlefieldData () {
        savePanel.SetActive (true);
        continueSave = false;

        Game.game.enemySpawnPoints = new List<EnemySpawnPoint> ();
        for (int i = 0; i < spawnpoints.Count; i++) {
            Game.game.enemySpawnPoints.Add (spawnpoints[i].GetComponent<EditorEnemySpawnPoint> ().Convert ());
        }

        StartCoroutine (SAVE ());
    }

    public void EditSpawnPoint () {
        PlayerInput.cur.isPlacing = true;
        if (PlayerInput.cur.isEditingWalls)
            PlayerInput.cur.EditWalls (false);
    }

    public void ContinueSave () {
        continueSave = true;
    }

    IEnumerator SAVE () {
        while (!continueSave)
            yield return null;

        Game.game.SaveBattlefieldData (saveName.text);
        savePanel.SetActive (false);
    }

    public void OnPressedLoad () {
        FileBrowser.OpenFileBrowser (Game.BATTLEFIELD_SAVE_DIRECTORY, gameObject, "EditorLoadBattlefield");
    }

    void EditorLoadBattlefield (string path) {
        foreach (GameObject sp in spawnpoints) {
            Destroy (sp);
        }
        spawnpoints.Clear ();

        Game.BattlefieldData data = Game.game.LoadBattlefieldData (path, true);

        Game.game.battlefieldWidth = data.width;
        Game.game.battlefieldHeight = data.height;
        Game.isWalled = data.walls;
        
        foreach (EnemySpawnPoint point in data.spawns) {
            GameObject newSpawn = (GameObject)Instantiate (spawnpointPrefab, point.worldPosition, Quaternion.identity);
            EditorEnemySpawnPoint spawnPoint = newSpawn.GetComponent<EditorEnemySpawnPoint> ();

            spawnPoint.endPlaced = false;
            spawnPoint.endPoint = point.endPoint.worldPosition;
            spawnPoint.endPlaced = true;
        }

        Game.game.SetBackgoundGraphic ();
        Game.game.GenerateWallMesh ();
    }
}
