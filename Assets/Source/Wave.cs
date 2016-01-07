using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Wave {

	public List<Subwave> subwaves = new List<Subwave>();

	[System.Serializable]
	public class Subwave {
		public List<Wave.Enemy> enemies = new List<Enemy>();
		public float spawnTime;
	}

	[System.Serializable]
	public class Enemy {
		public GameObject enemy;
		public int spawnAmount;
		public int index;
	}
}