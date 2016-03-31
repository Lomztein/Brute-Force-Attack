using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class BattlefieldSelectionMenu : MonoBehaviour {

    public Text battlefieldName;
    public RawImage preview;

    public Game.BattlefieldData[] loadedBattlefields;
    public Texture2D[] textures;
    private int index;

	// Use this for initialization
	void Start () {
        Initialize ();
	}

    Texture2D GenerateTexture (Game.BattlefieldData data) {
        int width = data.walls.GetLength (0);
        int height = data.walls.GetLength (1);

        Texture2D tex = new Texture2D (width, height);

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                switch ((Game.WallType)data.walls[x,y]) {
                    case Game.WallType.Level:
                        tex.SetPixel (x, y, Color.red);
                        break;

                    case Game.WallType.None:
                        tex.SetPixel (x, y, new Color (0f, 0.04f, 0f));
                        break;
                }
            }
        }

        for (int i = 0; i < data.spawns.Count; i++) {
            EnemySpawnPoint sp = data.spawns[i];
            Vector3 wsp = sp.worldPosition + new Vector3 (data.width, data.height) / 2f;
            int sx = Mathf.RoundToInt(wsp.x);
            int sy = Mathf.RoundToInt(wsp.y) - 1;

            Vector3 wep = sp.endPoint.worldPosition + new Vector3 (data.width, data.height) / 2f;
            int ex = Mathf.RoundToInt(wep.x);
            int ey = Mathf.RoundToInt(wep.y);

            tex.SetPixel(sx, sy, Color.red);
            tex.SetPixel(ex, ey, Color.green);
        }

        tex.filterMode = FilterMode.Point;
        tex.Apply ();

        return tex;
    }

    public void LoadDataToGame () {
        if (loadedBattlefields.Length > 0) {
            Debug.Log ("Loading battlefield data..");
            Game game = Game.game;
            Game.BattlefieldData data = loadedBattlefields[index];

            game.battlefieldWidth = data.width;
            game.battlefieldHeight = data.height;
            game.enemySpawnPoints = data.spawns;

            Game.isWalled = (Game.WallType[,])data.walls.Clone ();
        }
    }

    public void ChangePreview (int movement) {
        index += movement;

        if (index < 0) {
            index = loadedBattlefields.Length - 1;
        }else if (index >= loadedBattlefields.Length) {
            index = 0;
        }

        if (textures[index] == null) {
            textures[index] = GenerateTexture (loadedBattlefields[index]);
        }

        preview.texture = textures[index];
        battlefieldName.text = loadedBattlefields[index].name;
    }

    void Initialize () {
        string path = Game.BATTLEFIELD_SAVE_DIRECTORY;
        string[] files = Directory.GetFiles (path, "*.dat");
        loadedBattlefields = new Game.BattlefieldData[files.Length];
        textures = new Texture2D[files.Length];

        for (int i = 0; i < files.Length; i++) {
            loadedBattlefields[i] = Game.game.LoadBattlefieldData (files[i], true);
        }

        ChangePreview (0);
    }
}
