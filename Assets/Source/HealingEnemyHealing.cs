using UnityEngine;
using System.Collections;

public class HealingEnemyHealing : MonoBehaviour {

    // Those beneath a range attribute are in percentages.
    [Range (0, 1)]
	public float healSpeed;
    [Range (0, 1)]
    public float healMax;
    public int startHealth;

	public Enemy enemy;
	private float healProgress;

    void Start () {
        startHealth = enemy.health;
    }

	// Update is called once per frame
	void FixedUpdate () {
		if (enemy.health < startHealth * healMax) {
			healProgress += startHealth * healSpeed * Time.fixedDeltaTime;
			if (healProgress > 1f) {
				enemy.health += Mathf.RoundToInt (healProgress);
                healProgress = 0f;
			}
		}else{
            enemy.health = Mathf.RoundToInt (startHealth * healMax);
		}
	}

	void OnTakeDamage () {
		healSpeed = 0f;
	}
}
