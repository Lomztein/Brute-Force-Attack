using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour {

	public Colour type;

	public float speed;
	public int health;
	public int damage;
	public int value;
	public bool isFlying;

	public bool rotateSprite;

	public Vector3[] path;
	public int pathIndex;
	private Vector3 offset;

	// TODO Add pathfinding and, well, just improve overall

	void Start () {
		Vector3 off = Random.insideUnitSphere / 2f;
		offset = new Vector3 (off.x, off.y, 0f);
		path = Dijkstra.GetPath (transform.position);
		health = Mathf.RoundToInt ((float)health * EnemySpawn.gameProgress);
	}

	// Update is called once per frame
	void FixedUpdate () {
		Move ();
	}

	void Move () {
		if (pathIndex == path.Length-1 || isFlying) {
			transform.position += Vector3.down * Time.fixedDeltaTime * speed;
			if (rotateSprite)
				transform.rotation = Quaternion.Euler (0,0,270);
			return;
		}

		Vector3 loc = path[pathIndex] + offset;
		float dist = Vector3.Distance (transform.position, loc);

		if (dist < speed * Time.fixedDeltaTime) {
			pathIndex++;
		}

		transform.position = Vector3.MoveTowards (transform.position, loc, speed * Time.fixedDeltaTime);

		if (rotateSprite)
			transform.rotation = Quaternion.Euler (0,0,Angle.CalculateAngle (transform.position, path[pathIndex] + offset));
	}

	void OnTakeDamage (Projectile.Damage damage) {
		if (damage.effectiveAgainst == type) {
			health -= damage.damage;
		}else{
			health -= Mathf.RoundToInt ((float)damage.damage / 2f);
		}

		if (health < 0) {
			Game.credits += Mathf.RoundToInt ((float)value * (int)EnemySpawn.gameProgress * 0.2f);
			SendMessage ("OnDeath", SendMessageOptions.DontRequireReceiver);
			Destroy (gameObject);
		}
	}

	void OnCollisionEnter (Collision col) {
		
		Datastream stream = col.gameObject.GetComponent<Datastream>();
		List<Transform> nearest = new List<Transform>();

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
				nearest.Add (near);
			}

		}

		for (int i = 0; i < nearest.Count; i++) {
			nearest[i].GetComponent<SpriteRenderer>().color = Color.red;
		}

		Destroy (gameObject);
	}
}
