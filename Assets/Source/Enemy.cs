using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamagable {

	[Header ("Stats")]
	public Colour type;

	public float speed;
	public long health;
	public int damage;
	public int value;
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
    public Transform thisTransform;
    public Transform healthbarTransform;
    public Rigidbody thisRigidody;
    public bool active;

	public Slider healthSlider;

    public static long GetHealth (long startingHealth, float progress, float multiplier) {
        return (long)(startingHealth * progress * multiplier);
    }

    public static long GetHealth (long startingHealth, float progress) {
        return (long)(startingHealth * progress * Game.difficulty.healthMultiplier);
    }

    public static int GetValue (int startingValue, int wave) {
        return Mathf.RoundToInt (startingValue + wave * 0.2f);
    }

    public void Initialize () {
        thisTransform = transform;
        thisRigidody = GetComponent<Rigidbody> ();
    }

    public virtual void Start () {
        active = true;
		Vector3 off = Random.insideUnitSphere / 2f;
		offset = new Vector3 (off.x, off.y, 0f);
        health = GetHealth (health, EnemyManager.gameProgress);

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
        healthSlider.transform.SetParent (transform);
        healthbarTransform = healthSlider.transform;
	}

	public int GetPathDistanceRemaining () {
		return path.Length - pathIndex;
	}

    public void UpdateHealthbar() {
        if (healthSlider) {
            if (healthbarTransform.parent != thisTransform) {
                healthSlider.value = health;
                healthbarTransform.position = thisTransform.position + Vector3.up;

                if (((Vector2)thisTransform.position - PlayerInput.worldMousePos).sqrMagnitude > 25f) {
                    if (healthbarTransform.parent != thisTransform) {
                        healthbarTransform.SetParent (thisTransform);
                    }
                }
            } else if (((Vector2)thisTransform.position - PlayerInput.worldMousePos).sqrMagnitude < 25f) {
                healthbarTransform.SetParent (Game.game.worldCanvas.transform);
                healthbarTransform.rotation = Quaternion.identity;
            }
        }
    }

    public void UpdateFreeze() {
        if (freezeMultiplier < 1f) {
            freezeMultiplier += 0.5f * Time.fixedDeltaTime;
        } else {
            freezeMultiplier = 1f;
        }
    }

    public virtual Vector3 DoMovement() {
        if (pathIndex == path.Length - 1) {
            pathIndex--;
        }

        Vector3 loc = new Vector3 (path [ pathIndex ].x, path [ pathIndex ].y) + offset;
        float dist = Vector3.Distance (thisTransform.position, loc);

        if (dist < speed * Time.fixedDeltaTime * 2f) {
            pathIndex++;
        }

        thisRigidody.MovePosition (Vector3.MoveTowards (thisTransform.position, loc, speed * Time.fixedDeltaTime * freezeMultiplier));
        return loc;
    }

	public void OnTakeDamage (Projectile.Damage damage) {
		if (damage.effectiveAgainst == type) {
			health -= damage.damage;
		}else{
			health -= Mathf.RoundToInt (damage.damage * Game.difficulty.weaponDamageToDifferentColor);
		}

		if (health < 0) {

			if (!isDead) {
				isDead = true;
                Game.credits += GetValue (value, EnemyManager.ExternalWaveNumber);
				if (upcomingElement) upcomingElement.Decrease ();
				SendMessage ("OnDeath", SendMessageOptions.DontRequireReceiver);
                active = false;

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

        Datastream stream = Datastream.cur;

		if (Datastream.enableFirewall)
			damage = Mathf.RoundToInt ((float)damage * 0.4f);

        Datastream.healthAmount -= damage;
        stream.EmitCorruptionParticles (transform.position, damage / 10f);

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
