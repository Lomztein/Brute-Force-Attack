using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class WaveEditor : EditorWindow {

	private enum WindowStatus { All, Wave, Graph }
	private WindowStatus windowStatus;
    private float graphOffset;
    private int highestDifficulty;

	public EnemyManager spawner;
	public Wave wave;

	[MenuItem ("Project Virus/Wave Editor")]
	public static void ShowWindow () {
		EditorWindow.GetWindow (typeof(WaveEditor));
	}

	void OnGUI () {
		if (!spawner)
			spawner = GameObject.Find ("EnemyManager").GetComponent<EnemyManager>();

		if (windowStatus == WindowStatus.All) {

            if (GUILayout.Button ("Difficulty graph")) {
                windowStatus = WindowStatus.Graph;
            }

            for (int i = 0; i < spawner.waves.Count; i++) {

				if (GUILayout.Button ("Wave: " + (i + 1).ToString () + " - Difficulty: " + CalculateDifficulty (spawner.waves[i], i))) {
					windowStatus = WindowStatus.Wave;
					wave = spawner.waves[i];
				}

			}

            if (GUILayout.Button ("Remove wave")) {
                spawner.waves.RemoveAt (spawner.waves.Count - 1);
            }
            if (GUILayout.Button ("Add wave")) {
				spawner.waves.Add (new Wave ());
			}
        }
        if (windowStatus == WindowStatus.Wave) {
			if (GUILayout.Button ("Back"))
				windowStatus = WindowStatus.Graph;

			for (int i = 0; i < wave.subwaves.Count; i++) {
				GUILayout.Label ("Subwave " + (i+1).ToString ());
				for (int j = 0; j < wave.subwaves[i].enemies.Count; j++) {
			
					wave.subwaves[i].enemies[j].enemy = (GameObject)EditorGUILayout.ObjectField (wave.subwaves[i].enemies[j].enemy, typeof (GameObject), false);
					wave.subwaves[i].enemies[j].spawnAmount = EditorGUILayout.IntField ("Spawn amount",wave.subwaves[i].enemies[j].spawnAmount);
					if (GUILayout.Button ("Remove enemy"))
						wave.subwaves[i].enemies.Remove (wave.subwaves[i].enemies[j]);
				}

				if (GUILayout.Button ("Add enemy"))
					wave.subwaves[i].enemies.Add (new Wave.Enemy ());

				wave.subwaves[i].spawnTime = EditorGUILayout.FloatField ("Subwave length",wave.subwaves[i].spawnTime);
				if (GUILayout.Button ("Remove subwave"))
					wave.subwaves.Remove (wave.subwaves[i]);
			}

			if (GUILayout.Button ("Add subwave"))
				wave.subwaves.Add (new Wave.Subwave ());
		}
        if (windowStatus == WindowStatus.Graph) {
            if (GUILayout.Button ("Back"))
                windowStatus = WindowStatus.All;
            if (GUILayout.Button ("Reset Curve"))
                highestDifficulty = 0;

            graphOffset = GUILayout.HorizontalSlider (graphOffset, 0, spawner.waves.Count * 30);

            for (int i = 0; i < spawner.waves.Count; i++) {
                Rect baseRect = new Rect ((-graphOffset + 10) + i * 30, position.height - 30, 25, 25);
                if (GUI.Button (baseRect, i.ToString ())) {
                    windowStatus = WindowStatus.Wave;
                    wave = spawner.waves[i];
                }
                int difficulty = CalculateDifficulty (spawner.waves[i], i);
                int height = Mathf.RoundToInt ((float)difficulty / Mathf.Max (highestDifficulty, 1) * (position.height - 100));
                GUI.Box (new Rect (baseRect.x + 5, baseRect.y - height, 15, height), "");
                if (difficulty > highestDifficulty) {
                    highestDifficulty = difficulty;
                }
            }
        }
	}

    private int CalculateDifficulty (Wave wave, int waveIndex) {

        int baseline = 25;
        int value = 0;
        
        foreach (Wave.Subwave sub in wave.subwaves) {
            int locValue = 0;
            foreach (Wave.Enemy ene in sub.enemies) {
                if (ene.enemy) {
                    Enemy enemy = ene.enemy.GetComponent<Enemy> ();
                    if (enemy)
                        locValue += Enemy.GetHealth (enemy.health, spawner.GetProgressForWaveFromInstance (waveIndex), 1f) * (int)enemy.speed * ene.spawnAmount;
                }
            }
            locValue *= sub.enemies.Count;
            locValue /= (int)Mathf.Max (sub.spawnTime, 1);
            value += locValue;
        }
        return value / baseline;
    }
}