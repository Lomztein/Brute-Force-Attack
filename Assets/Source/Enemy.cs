using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Enemy : MonoBehaviour {

	[Header ("Stats")]
	public Colour type;

	public float speed;
	public int health;
	public int damage;
	public int value;
	public bool isFlying;
	public int researchDropChance;

	public EnemySpawnPoint spawnPoint;

	public bool rotateSprite;
	public float freezeMultiplier = 1f;

	[Header ("Pathing")]
	public Vector2[] path;
	public int pathIndex;
	private Vector3 offset;

	[Header ("Other")]
	public GameObject deathParticle;
	public GameObject researchPoint;
	public UpcomingElement upcomingElement;
	private bool isDead;

	private Slider healthSlider;

	// TODO Add pathfinding and, well, just improve overall

	void Start () {
		Vector3 off = Random.insideUnitSphere / 2f;
		offset = new Vector3 (off.x, off.y, 0f);
		health = Mathf.RoundToInt ((float)health * EnemySpawn.gameProgress);
		CreateHealthMeter ();
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

	void FixedUpdate () {
		Move ();
		UpdateHealthSlider ();
	}

	void UpdateHealthSlider () {
		healthSlider.value = health;
		healthSlider.transform.position = transform.position + Vector3.up;

		Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		mousePos.z = 0;

		if (Vector3.Distance (transform.position, mousePos) > 5) {
			if (healthSlider.gameObject.activeInHierarchy) {
				healthSlider.gameObject.SetActive (false);
			}
		}else{
			if (!healthSlider.gameObject.activeInHierarchy) {
				healthSlider.gameObject.SetActive (true);
			}
		}
	}

	void Move () {

		while (pathIndex == path.Length - 1 || isFlying) {
			transform.position += Vector3.down * Time.fixedDeltaTime * speed;
			if (rotateSprite)
				transform.rotation = Quaternion.Euler (0, 0, 270);
			return;
		}
		Vector3 loc = new Vector3 (path [pathIndex].x, path [pathIndex].y) + offset;
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
	}

	void OnTakeDamage (Projectile.Damage damage) {
		if (damage.effectiveAgainst == type) {
			health -= damage.damage;
		}else{
			health -= Mathf.RoundToInt ((float)damage.damage / 2f);
		}

		if (health < 0) {
			Destroy (gameObject);
			if (healthSlider) Destroy (healthSlider.gameObject);

			if (!isDead) {
				isDead = true;
				Game.credits += Mathf.RoundToInt ((float)value * (float)EnemySpawn.gameProgress * 0.2f);
				if (upcomingElement) upcomingElement.Decrease ();
				SendMessage ("OnDeath", SendMessageOptions.DontRequireReceiver);
				EnemySpawn.cur.OnEnemyDeath ();

				Destroy ((GameObject)Instantiate (deathParticle, transform.position, Quaternion.identity), 1f);
				if (Random.Range (0, researchDropChance) == 0)
					Instantiate (researchPoint, transform.position, Quaternion.identity);
			}
		}
	}

	void OnCollisionEnter (Collision col) {
		
		Datastream stream = col.gameObject.GetComponent<Datastream>();
		List<Transform> nearest = new List<Transform>();

		if (Datastream.enableFirewall)
			damage = Mathf.RoundToInt ((float)damage * 0.4f);

		for (int j = 0; j < damage; j++) {

			float dist = float.MaxValue;
			Transform near = null;
		
			for (int i = 0; i < stream.pooledNumbers.Count; i++) {

				float d = Vector3.Distance (transform.position, stream.pooledNumbers[i].transform.position);
				if (d < dist) {
					dist = d;
					near = stream.pooledNumbers[i].transform;
				}
			}

			if (near) {
				stream.pooledNumbers.Remove(near.gameObject);
				stream.curruptNumbers.Add  (near.gameObject);
				nearest.Add (near);
			}

		}

		for (int i = 0; i < nearest.Count; i++) {
			nearest[i].GetComponent<SpriteRenderer>().color = Color.red;
		}

		Destroy (gameObject);
		EnemySpawn.cur.OnEnemyDeath ();
	}
}
