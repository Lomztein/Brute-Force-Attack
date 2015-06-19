using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace IngameEditors {

	public class WavesetEditor : MonoBehaviour {

		[Header ("References")]
		public GameObject wavePrefab;
		public GameObject subwavePrefab;
		public GameObject enemyPrefab;
		private List<GameObject> currentObjects = new List<GameObject>();

		[Header ("Wave Stuffs")]
		public List<Wave> waves = new List<Wave>();

		[Header ("Sizes")]
		public int startHeight;
		public int borderX;
		public int enemySize;
		public int subwaveBorderSize;
		public int waveBorderSize;

		void Awake () {
			waves = EnemySpawn.LoadWaveset (Game.WAVESET_SAVE_DIRECTORY + "DEFAULT");
			UpdateEditor ();
		}

		public void AddWave () {
			waves.Add (new Wave ());
		}

		private void UpdateEditor () {

			foreach (GameObject obj in currentObjects) {
				Destroy (obj);
			}

			int y = startHeight;
			foreach (Wave wave in waves) {
				GameObject newWave = (GameObject)Instantiate (wavePrefab, new Vector3 (Screen.width / 2f, startHeight), Quaternion.identity);
				foreach (Wave.Subwave sub in wave.subwaves) {
					foreach (Wave.Enemy ene in sub.enemies) {
					}
				}
			}

		}
	}
}