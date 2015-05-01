using UnityEngine;
using System.Collections;

public class HealingEnemyHealing : MonoBehaviour {

	public float healSpeed;
	public int healMax;

	public Enemy enemy;
	private float healProgress;

	// Use this for initialization
	void Start () {
		healSpeed *= EnemySpawn.gameProgress;
		healMax = enemy.health;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (enemy.health < healMax) {
			healProgress += healSpeed * Time.fixedDeltaTime;
			if (healProgress > 1f) {
				enemy.health += Mathf.RoundToInt (healProgress);
			}
		}else{
			enemy.health = healMax;
		}
	}

	void OnTakeDamage () {
		healSpeed = 0f;
	}
}
