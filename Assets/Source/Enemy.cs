﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamagable {

	[Header ("Stats")]
	public Colour type;

	public float speed;
	public int health;
	public int damage;
	public int value;
	public bool isFlying;
    public float flyingRotationSpeed;
	public int researchDropChance;
    public float difficultyMultiplier = 1f;

    public string enemyName;
    [TextArea]
    public string description;

	public EnemySpawnPoint spawnPoint;

	public bool rotateSprite;
	public float freezeMultiplier = 1f;

	[Header ("Pathing")]
	public Vector2[] path;
	public int pathIndex;
	public Vector3 offset;

    [Header ("Death Particle")]
	public ParticleSystem deathParticle;
    public int particleAmount = 100;
    public float particleLifetime = 1f;

	[Header ("Other")]
    public GameObject researchPoint;
    public UpcomingElement upcomingElement;
	private bool isDead;

	public Slider healthSlider;

    public static int GetHealth (int startingHealth, float progress, float multiplier) {
        return Mathf.RoundToInt (startingHealth * progress * multiplier);
    }

    public static int GetHealth (int startingHealth, float progress) {
        return Mathf.RoundToInt (startingHealth * progress * Game.difficulty.healthMultiplier);
    }

    public static int GetValue (int startingValue, int wave) {
        return Mathf.RoundToInt (startingValue + wave * 0.2f);
    }

	void Start () {
		Vector3 off = Random.insideUnitSphere / 2f;
		offset = new Vector3 (off.x, off.y, 0f);
        health = GetHealth (health, EnemyManager.gameProgress);

        if (isFlying)
            offset *= 5f;

        if (Game.game && Game.game.gamemode == Gamemode.GlassEnemies) {
            health /= 10;
        }else if (Game.game && Game.game.gamemode == Gamemode.TitaniumEnemies) {
            health *= 10;
        }

        if (Game.game)
            CreateHealthMeter ();

        SendMessage ("OnSpawn", SendMessageOptions.DontRequireReceiver);
    }

    void CreateHealthMeter () {
		GameObject loc = (GameObject)Instantiate (Game.game.enemyHealthSlider, transform.position + Vector3.up, Quaternion.identity);
		healthSlider = loc.GetComponent<Slider>();
		healthSlider.maxValue = health;
		loc.transform.SetParent (Game.game.worldCanvas.transform, true);
	}

	public int GetPathDistanceRemaining () {
		return path.Length - pathIndex;
	}

    /*
    void CombinedUpdate () {

        // Movement code.
        while (pathIndex == path.Length - 1 || isFlying) {
            transform.position += Vector3.down * Time.fixedDeltaTime * speed;
            if (rotateSprite)
                transform.rotation = Quaternion.Euler (0, 0, 270);
            return;
        }
        Vector3 loc = new Vector3 (path[pathIndex].x, path[pathIndex].y) + offset;
        float dist = Vector3.Distance (transform.position, loc);

        if (dist < speed * Time.fixedDeltaTime) {
            pathIndex++;
        }

        transform.position = Vector3.MoveTowards (transform.position, loc, speed * Time.fixedDeltaTime * freezeMultiplier);

        if (rotateSprite)
            transform.rotation = Quaternion.Euler (0, 0, Angle.CalculateAngle (transform.position, loc));

        if (freezeMultiplier < 1f) {
            freezeMultiplier += 0.5f * Time.fixedDeltaTime;
        } else {
            freezeMultiplier = 1f;
        }

        // Healthslider Code
        healthSlider.value = health;
        healthSlider.transform.position = transform.position + Vector3.up;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
        mousePos.z = 0;

        if (Vector3.Distance (transform.position, mousePos) > 5) {
            if (healthSlider.gameObject.activeSelf) {
                healthSlider.gameObject.SetActive (false);
            }
        } else {
            if (!healthSlider.gameObject.activeSelf) {
                healthSlider.gameObject.SetActive (true);
            }
        }

    }
    */

	public void OnTakeDamage (Projectile.Damage damage) {
		if (damage.effectiveAgainst == type) {
			health -= damage.damage;
		}else{
			health -= Mathf.RoundToInt (damage.damage * Game.difficulty.weaponDamageToDifferentColor);
		}

		if (health < 0) {

			if (!isDead) {
				isDead = true;
                Game.credits += GetValue (value, EnemyManager.externalWaveNumber);
				if (upcomingElement) upcomingElement.Decrease ();
				SendMessage ("OnDeath", SendMessageOptions.DontRequireReceiver);

                if (damage.weapon)
                    damage.weapon.AddKill ();

                if (deathParticle) {
                    deathParticle.transform.parent = null;
                    deathParticle.Emit (particleAmount);
                    Invoke ("DisableParticle", particleLifetime);
                }

                if (EnemyManager.spawnedResearch < Game.difficulty.researchPerRound) {
                    if ((Random.Range (0, EnemyManager.chanceToSpawnResearch) == 0)
                    || EnemyManager.cur.currentEnemies <= Game.difficulty.researchPerRound - EnemyManager.spawnedResearch) {

                        Instantiate (researchPoint, transform.position, Quaternion.identity);
                        EnemyManager.spawnedResearch++;
                    }
                }
                EnemyManager.cur.OnEnemyDeath ();
                EnemyManager.AddKill (enemyName);
            }
            if (healthSlider) healthSlider.transform.SetParent (transform);
            gameObject.SetActive (false);
        }
    }

    void DisableParticle () {
        deathParticle.gameObject.SetActive (false);
    }

	void OnCollisionEnter (Collision col) {

		Datastream stream = col.gameObject.GetComponent<Datastream>();
		List<Transform> nearest = new List<Transform>();

		if (Datastream.enableFirewall)
			damage = Mathf.RoundToInt ((float)damage * 0.4f);

        Datastream.healthAmount -= damage;
        stream.EmitCorruptionParticles (transform.position, damage / 10f);

		for (int i = 0; i < nearest.Count; i++) {
			nearest[i].GetComponent<SpriteRenderer>().color = Color.red;
		}

        if (healthSlider) healthSlider.transform.SetParent (transform);
        gameObject.SetActive (false);
		EnemyManager.cur.OnEnemyDeath ();

        SplitterEnemySplit split = GetComponent<SplitterEnemySplit>();
        if (split)
            for (int i = 0; i < split.spawnPos.Length; i++) {
                EnemyManager.cur.OnEnemyDeath();
            }
	}
}
