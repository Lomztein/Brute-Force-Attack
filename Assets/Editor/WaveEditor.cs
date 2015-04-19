using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class WaveEditor : EditorWindow {

	private enum WindowStatus { All, Wave }
	private WindowStatus windowStatus;

	public EnemySpawn spawner;
	public Wave wave;

	[MenuItem ("Project Virus/Wave Editor")]
	public static void ShowWindow () {
		EditorWindow.GetWindow (typeof(WaveEditor));
	}

	void OnGUI () {
		if (!spawner)
			spawner = GameObject.Find ("EnemySpawn").GetComponent<EnemySpawn>();

		if (windowStatus == WindowStatus.All) {
			for (int i = 0; i < spawner.waves.Count; i++) {

				if (GUILayout.Button ("Wave: " + (i + 1).ToString ())) {
					windowStatus = WindowStatus.Wave;
					wave = spawner.waves[i];
				}

			}

			if (GUILayout.Button ("Add wave")) {
				spawner.waves.Add (new Wave ());
			}
		}else{
			if (GUILayout.Button ("Back"))
				windowStatus = WindowStatus.All;

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
	}
}
