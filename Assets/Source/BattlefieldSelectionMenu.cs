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
                switch (data.walls[x,y]) {
                    case Game.WallType.Level:
                        tex.SetPixel (x, y, Color.red);
                        break;

                    case Game.WallType.None:
                        tex.SetPixel (x, y, Color.black);
                        break;
                }
            }
        }

        tex.filterMode = FilterMode.Point;
        tex.Apply ();

        return tex;
    }

    public void StartGame () {
        if (loadedBattlefields.Length > 0) {
            Game game = Game.game;
            Game.BattlefieldData data = loadedBattlefields[index];

            game.battlefieldWidth = data.width;
            game.battlefieldHeight = data.height;
            game.enemySpawnPoints = data.spawns;
            Game.isWalled = data.walls;
        }
    }

    public void ChangePreview (int movement) {
        index += movement;

        Debug.Log (loadedBattlefields.Length + ", " + textures.Length);

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
        string[] files = Directory.GetFiles (path);
        Debug.Log ("Files: " + files.Length);
        loadedBattlefields = new Game.BattlefieldData[files.Length];
        textures = new Texture2D[files.Length];

        for (int i = 0; i < files.Length; i++) {
            loadedBattlefields[i] = Game.game.LoadBattlefieldData (files[i], true);
        }

        ChangePreview (0);
    }
}
